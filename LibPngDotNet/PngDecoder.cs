using System;
using System.IO;
using System.Runtime.InteropServices;

namespace LibPngDotNet
{
	using static Native;
	using static PngUtils;

	/// <summary>
	/// Decoder to decode png image.
	/// </summary>
	public unsafe class PngDecoder : IDisposable
	{
		private readonly Stream _stream;
		private readonly bool _ownStream;

		// hold the delegate instances to prevent GC
		private readonly png_error _errorCallback;
		private readonly png_error _warningCallback;
		private readonly png_rw _readCallback;

#if !NET
		private byte[] _buffer;
#endif

		private IntPtr _pngPtr;
		private IntPtr _infoPtr;

		private DecoderSettings _settings;

		// It's dangerous between Constructor and Initialize
		// (e.g. access Width property)
		// So use static method to provide instance.

		/// <summary>
		/// Open a file to decode png image.
		/// </summary>
		/// <param name="filePath">Png file path.</param>
		/// <returns><see cref="PngDecoder"/> instance to decode png file.</returns>
		public static PngDecoder Open(string filePath)
		{
			var stream = File.OpenRead(filePath);
			var decoder = new PngDecoder(stream, true);
			decoder.Initialize();
			return decoder;
		}

		/// <summary>
		/// Use a stream to decode png image.
		/// </summary>
		/// <param name="stream">A stream contains png content.</param>
		/// <returns><see cref="PngDecoder"/> instance for decoding from <paramref name="stream"/></returns>
		public static PngDecoder Open(Stream stream)
		{
			var decoder = new PngDecoder(stream, false);
			decoder.Initialize();
			return decoder;
		}

		private PngDecoder(Stream stream, bool ownStream)
		{
			_stream = stream ?? throw new ArgumentNullException(nameof(stream));
			_ownStream = ownStream;

			_errorCallback = OnLibPngError;
			_warningCallback = OnLibPngWarning;
			_readCallback = ReadFromStream;
		}

		private void Initialize()
		{
			try
			{
				var version = png_get_libpng_ver();
				_pngPtr = png_create_read_struct(version, IntPtr.Zero, _errorCallback, _warningCallback);

				if (_pngPtr == IntPtr.Zero)
					throw new LibPngException("Fail to create read_struct");

				_infoPtr = png_create_info_struct(_pngPtr);
				if (_infoPtr == IntPtr.Zero)
					throw new LibPngException("Fail to create info_struct");

				png_set_read_fn(_pngPtr, IntPtr.Zero, _readCallback);
				png_read_info(_pngPtr, _infoPtr);
			}
			catch
			{
				Dispose();
				throw;
			}
		}

		/// <inheritdoc />
		~PngDecoder()
		{
			Dispose();
		}

		/// <summary>
		/// Non-fatal error in libpng.
		/// Can continue, but may have a problem.
		/// </summary>
		public static event EventHandler<string> PngWarningEvent;

		/// <summary>
		/// The <see cref="Stream"/> where decode from.
		/// </summary>
		public Stream Stream => _stream;

		/// <summary>
		/// Decoder settings.
		/// </summary>
		/// <seealso cref="DecoderSettings"/>
		public DecoderSettings Settings
		{
			get => _settings ?? (_settings = new DecoderSettings());
			set => _settings = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>
		/// Image width in pixels.
		/// </summary>
		public int Width => (int) png_get_image_width(_pngPtr, _infoPtr);

		/// <summary>
		/// Image height in pixels.
		/// </summary>
		public int Height => (int) png_get_image_height(_pngPtr, _infoPtr);

		/// <summary>
		/// Number of pixels in image.
		/// </summary>
		public int PixelCount => Width * Height;

		/// <summary>
		/// Bits per channel. (1, 2, 4, 8, or 16)
		/// </summary>
		public byte BitDepth => png_get_bit_depth(_pngPtr, _infoPtr);

		/// <summary>
		/// Image <see cref="T:LibPngDotNet.ColorType"/>
		/// </summary>
		public ColorType ColorType => png_get_color_type(_pngPtr, _infoPtr);

		/// <summary>
		/// Image <see cref="FilterType"/>
		/// </summary>
		public FilterType FilterType => png_get_filter_type(_pngPtr, _infoPtr);

		/// <summary>
		/// Image <see cref="InterlaceType"/>.
		/// </summary>
		public InterlaceType InterlaceType => png_get_interlace_type(_pngPtr, _infoPtr);

		/// <summary>
		/// Image <see cref="CompressionType"/>.
		/// </summary>
		public CompressionType Compression => png_get_compression_type(_pngPtr, _infoPtr);

		/// <summary>
		/// Number of bytes needed to hold a row.
		/// </summary>
		public int RowBytes => png_get_rowbytes(_pngPtr, _infoPtr);

		/// <summary>
		/// Number of color channels in image.
		/// </summary>
		public byte Channels => png_get_channels(_pngPtr, _infoPtr);

		/// <summary>
		/// Read image pixels.
		/// </summary>
		/// <typeparam name="T">Unmanaged struct to hold the result. It type should has a <see cref="PngPixelAttribute"/> attribute.</typeparam>
		/// <returns>The pixels read.</returns>
		public T[] ReadPixels<T>() where T : unmanaged
		{
			var layout = GetPixelLayoutByAttribute(typeof(T));
			return ReadPixels<T>(layout);
		}

		/// <summary>
		/// Read image pixels.
		/// </summary>
		/// <typeparam name="T">Unmanaged struct to hold the result.</typeparam>
		/// <param name="layout">Describe the layout of the resulting pixel.</param>
		/// <returns>The pixels read.</returns>
		public T[] ReadPixels<T>(PixelLayout layout) where T : unmanaged
		{
			// TODO: check input before allocate array
			var bufferSize = (layout.PixelBits * PixelCount + 7) / 8;
			var arraySize = (bufferSize + sizeof(T) - 1) / sizeof(T);
			var result = new T[arraySize];
			ReadPixels(layout, result, 0);
			return result;
		}

		/// <summary>
		/// Read image pixels.
		/// </summary>
		/// <typeparam name="T">Unmanaged struct to hold the result. It type should has a <see cref="PngPixelAttribute"/> attribute.</typeparam>
		/// <param name="buffer">Buffer to hold the result.</param>
		/// <param name="offset">Start index in <paramref name="buffer"/>.</param>
		/// <returns>The number of pixels read, which should be <see cref="PixelCount"/>.</returns>
		public int ReadPixels<T>(T[] buffer, int offset = 0) where T : unmanaged
		{
			var layout = GetPixelLayoutByAttribute(typeof(T));
			return ReadPixels(layout, buffer, offset);
		}

		/// <summary>
		/// Read image pixels.
		/// </summary>
		/// <typeparam name="T">Unmanaged struct to hold the result.</typeparam>
		/// <param name="layout">Describe the layout of pixel in <paramref name="buffer"/>.</param>
		/// <param name="buffer">Buffer to hold the result.</param>
		/// <param name="offset">Start index in <paramref name="buffer"/>.</param>
		/// <returns>The number of pixels read, which should be <see cref="PixelCount"/>.</returns>
		public int ReadPixels<T>(PixelLayout layout, T[] buffer, int offset = 0) where T : unmanaged
#if NET
		{
			return ReadPixels(layout, buffer.AsSpan(offset));
		}

		/// <summary>
		/// Read image pixels.
		/// </summary>
		/// <param name="buffer">Buffer to hold the result.</param>
		/// <typeparam name="T">Unmanaged struct to hold the result. It type should has a <see cref="PngPixelAttribute"/> attribute.</typeparam>
		/// <returns>The number of pixels read, which should be <see cref="PixelCount"/>.</returns>
		public int ReadPixels<T>(Span<T> buffer) where T : unmanaged
		{
			var layout = GetPixelLayoutByAttribute(typeof(T));
			return ReadPixels(layout, buffer);
		}

		/// <summary>
		/// Read image pixels.
		/// </summary>
		/// <typeparam name="T">Unmanaged struct to hold the result.</typeparam>
		/// <param name="layout">Describe the layout of pixel in <paramref name="buffer"/>.</param>
		/// <param name="buffer">Buffer to hold the result.</param>
		/// <returns>The number of pixels read, which should be <see cref="PixelCount"/>.</returns>
		public int ReadPixels<T>(PixelLayout layout, Span<T> buffer) where T : unmanaged
#endif
		{
			AssertValidInput(layout,
#if NET
				(ReadOnlySpan<T>) buffer,
#else
				buffer, offset,
#endif
				PixelCount);

			TransformForRead(layout);

			var rowBytes = layout.PixelBits * Width / 8;
			using (var rowPointers = new RowPointerArray(rowBytes, Height))
			{
				fixed (T* bufferPtr = buffer)
				{
					rowPointers.SetRowPointers(bufferPtr, Settings.InvertY);
					png_read_image(_pngPtr, rowPointers.Pointer);
				}
			}

			return PixelCount;
		}

		private void TransformForRead(PixelLayout layout)
		{
			png_set_expand(_pngPtr);

			switch (layout.BitDepth)
			{
				// two 16 bits to 8 bits method:
				// png_set_scale_16: value * 0xFF / 0xFFFF, accurately
				// png_set_strip_16: value >> 8
				case 8: png_set_scale_16(_pngPtr); break;
				case 16: png_set_expand_16(_pngPtr); break;
			}

			if (layout.HasAlpha)
			{
				// fill with 0xFF or 0xFFFF
				var filler = (uint)(1 << layout.BitDepth) - 1;
				png_set_add_alpha(_pngPtr, filler, (layout.Flags & PixelLayoutFlags.AlphaAtHead) == 0);
			}
			else
			{
				png_set_strip_alpha(_pngPtr);
			}

			if (layout.IsGrayScale)
			{
				var confidence = Settings.RgbToGrayConfidence;
				png_set_rgb_to_gray(_pngPtr, 1 /*no error, no warning*/,
					confidence.R, confidence.G);
			}
			else
			{
				png_set_gray_to_rgb(_pngPtr);
			}

			if ((layout.Flags & PixelLayoutFlags.ReverseRgbOrder) != 0)
			{
				png_set_bgr(_pngPtr);
			}

			if ((layout.Flags & PixelLayoutFlags.AlphaAtHead) != 0)
			{
				png_set_swap_alpha(_pngPtr);
			}

			// call this will affect properties, like ColorType.
			// png_read_update_info(_pngPtr, _infoPtr);
		}

		/// <summary>
		/// Free unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			png_destroy_info_struct(_pngPtr, ref _infoPtr);

			var endInfoPtr = IntPtr.Zero;
			png_destroy_read_struct(ref _pngPtr, ref _infoPtr, ref endInfoPtr);

			if (_ownStream) 
				_stream?.Dispose();

			GC.SuppressFinalize(this);
		}

		private void OnLibPngError(IntPtr pngPtr, string pngError)
		{
			throw new LibPngException(pngError);
		}

		private void OnLibPngWarning(IntPtr pngPtr, string pngError)
		{
			PngWarningEvent?.Invoke(this, pngError);
		}

		private void ReadFromStream(IntPtr pngPtr, byte* ptr, int length)
		{
#if NET
			var span = new Span<byte>(ptr, length);
			var readLength = _stream.Read(span);
#else
			ResizeBuffer(ref _buffer, length);
			var readLength = _stream.Read(_buffer, 0, length);
#endif
			if (readLength != length)
				throw new LibPngException($"Attempted to read {length} bytes from stream but only read {readLength} bytes.");

#if !NET
			Marshal.Copy(_buffer, 0, (IntPtr) ptr, length);
#endif
		}
	}
}
