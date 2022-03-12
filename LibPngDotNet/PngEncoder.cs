using System;
using System.IO;

namespace LibPngDotNet
{
	using static Native;
	using static PngUtils;

	public unsafe class PngEncoder : IDisposable
	{
		private readonly Stream _stream;
		private readonly bool _ownStream;

		// hold the delegate instances to prevent GC
		private readonly png_error _errorCallback;
		private readonly png_error _warningCallback;
		private readonly png_rw _writeCallback;
		private readonly png_flush _flushCallback;

		private IntPtr _pngPtr;
		private IntPtr _infoPtr;

		private EncoderSettings _settings;

		/// <summary>
		/// Open a file to encode png image.
		/// </summary>
		/// <param name="filePath">Png file path.</param>
		/// <returns><see cref="PngEncoder"/> instance to encode png file.</returns>
		public static PngEncoder Open(string filePath)
		{
			var stream = File.OpenWrite(filePath);
			var encoder = new PngEncoder(stream, true);
			encoder.Initialize();
			return encoder;
		}

		/// <summary>
		/// Use a stream to encode png image.
		/// </summary>
		/// <param name="stream">A stream to write png content.</param>
		/// <returns><see cref="PngEncoder"/> instance for encoding to <paramref name="stream"/></returns>
		public static PngEncoder Open(Stream stream)
		{
			var encoder = new PngEncoder(stream, false);
			encoder.Initialize();
			return encoder;
		}

		private PngEncoder(Stream stream, bool ownStream)
		{
			_stream = stream ?? throw new ArgumentNullException(nameof(stream));
			_ownStream = ownStream;

			_errorCallback = OnLibPngError;
			_warningCallback = OnLibPngWarning;
			_writeCallback = WriteToStream;
			_flushCallback = FlushStream;
		}

		private void Initialize()
		{
			try
			{
				var version = png_get_libpng_ver();
				_pngPtr = png_create_write_struct(version, IntPtr.Zero, _errorCallback, _warningCallback);

				if (_pngPtr == IntPtr.Zero)
					throw new LibPngException("Fail to create write_struct");

				_infoPtr = png_create_info_struct(_pngPtr);
				if (_infoPtr == IntPtr.Zero)
					throw new LibPngException("Fail to create info_struct");

				png_set_write_fn(_pngPtr, IntPtr.Zero, _writeCallback, _flushCallback);
			}
			catch
			{
				Dispose();
				throw;
			}
		}

		~PngEncoder()
		{
			Dispose();
		}

		/// <inheritdoc cref="PngDecoder.PngWarningEvent"/>
		public static event EventHandler<string> PngWarningEvent;

		/// <summary>
		/// The <see cref="Stream"/> where encode to.
		/// </summary>
		public Stream Stream => _stream;

		/// <summary>
		/// Encoder Settings.
		/// </summary>
		/// <seealso cref="EncoderSettings"/>
		public EncoderSettings Settings
		{
			get => _settings ?? (_settings = new EncoderSettings());
			set => _settings = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>
		/// Image <see cref="FilterType"/>.
		/// </summary>
		public FilterType FilterMethod { get; set; } = FilterType.Default;

		/// <summary>
		/// Image <see cref="InterlaceType"/>.
		/// </summary>
		public InterlaceType InterlaceType { get; set; } = InterlaceType.None;

		/// <summary>
		/// Image <see cref="CompressionType"/>.
		/// </summary>
		public CompressionType Compression { get; set; } = CompressionType.Default;

		/// <inheritdoc cref="WriteImage{T}(int,int,PixelLayout,ReadOnlySpan{T})"/>
		public void WriteImage<T>(int width, int height, ReadOnlySpan<T> pixels) where T : unmanaged
		{
			var layout = GetPixelLayoutByAttribute(typeof(T));
			WriteImage(width, height, layout, pixels);
		}

		/// <summary>
		/// Write pixels as Png image.
		/// </summary>
		/// <typeparam name="T">Unmanaged struct to describe the pixels data. If no <see cref="PixelLayout"/> argument passed, this type should has a <see cref="PngPixelAttribute"/> attribute.</typeparam>
		/// <param name="width">Image width in pixels.</param>
		/// <param name="height">Image height in pixels.</param>
		/// <param name="pixelLayout">Describe the layout of pixel in <paramref name="pixels"/>.</param>
		/// <param name="pixels">Image pixels.</param>
		public void WriteImage<T>(int width, int height, PixelLayout pixelLayout, ReadOnlySpan<T> pixels) where T : unmanaged
		{
			AssertValidInput(pixelLayout, pixels, width * height);

			png_set_IHDR(_pngPtr, _infoPtr,
				width, height, pixelLayout.BitDepth, (int)pixelLayout.PngColorType,
				(int)InterlaceType, (int)Compression, (int)FilterMethod);

			// make png_set_IHDR work
			png_write_info(_pngPtr, _infoPtr);

			TransformForWrite(pixelLayout);

			var rowBytes = pixelLayout.PixelBits * width / 8;

			using var rowPointers = new RowPointerArray(rowBytes, height);
			fixed (T* bufferPtr = pixels)
			{
				rowPointers.SetRowPointers(bufferPtr, Settings.InvertY);
				png_write_image(_pngPtr, rowPointers.Pointer);
			}

			png_write_end(_pngPtr, IntPtr.Zero);
		}

		private void TransformForWrite(PixelLayout layout)
		{
			if ((layout.Flags & PixelLayoutFlags.ReverseRgbOrder) != 0)
			{
				png_set_bgr(_pngPtr);
			}

			if ((layout.Flags & PixelLayoutFlags.AlphaAtHead) != 0)
			{
				png_set_swap_alpha(_pngPtr);
			}
		}

		/// <summary>
		/// Free unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			png_destroy_info_struct(_pngPtr, ref _infoPtr);
			png_destroy_write_struct(ref _pngPtr, ref _infoPtr);

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

		private void WriteToStream(IntPtr pngPtr, byte* ptr, int length)
		{
			var span = new Span<byte>(ptr, length);
			_stream.Write(span);
		}

		private void FlushStream(IntPtr pngPtr)
		{
			_stream.Flush();
		}
	}
}
