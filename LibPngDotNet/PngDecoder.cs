using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LibPngDotNet
{
	using static Native;

	/// <summary>
	/// Decode png image.
	/// </summary>
	public unsafe class PngDecoder : IDisposable
	{
		private readonly Stream _stream;
		private readonly bool _ownStream;

		// hold the delegate instances to prevent GC
		private readonly png_error _errorCallback;
		private readonly png_error _warningCallback;
		private readonly png_rw_ptr _readCallback;

		private IntPtr _pngPtr;
		private IntPtr _infoPtr;

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
			var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			var decoder = new PngDecoder(stream, true);
			decoder.Initialize();
			return decoder;
		}

		/// <summary>
		/// Open a stream to decode png image.
		/// </summary>
		/// <param name="stream">A stream contains png content.</param>
		/// <returns><see cref="PngDecoder"/> instance to decode <paramref name="stream"/></returns>
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
				{
					throw new LibPngException("Fail to create read_struct");
				}

				_infoPtr = png_create_info_struct(_pngPtr);
				if (_infoPtr == IntPtr.Zero)
				{
					throw new LibPngException("Fail to create info_struct");
				}

				png_set_read_fn(_pngPtr, IntPtr.Zero, _readCallback);
				png_read_info(_pngPtr, _infoPtr);
			}
			catch
			{
				Dispose();
				throw;
			}
		}

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
		/// Decode settings.
		/// </summary>
		public PngSettings Settings { get; } = new PngSettings();

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
		/// Number of bytes needed to hold a transformed row.
		/// </summary>
		public int RowBytes => png_get_rowbytes(_pngPtr, _infoPtr);

		/// <summary>
		/// Number of color channels in image.
		/// </summary>
		public byte Channels => png_get_channels(_pngPtr, _infoPtr);

		/// <inheritdoc cref="ReadPixels{TPixel}(PixelLayout, TPixel[], int)"/>
		public TPixel[] ReadPixels<TPixel>() where TPixel : unmanaged
		{
			TPixel[] result = null;
			ReadPixels(ref result);
			return result;
		}

		/// <inheritdoc cref="ReadPixels{TPixel}(PixelLayout, TPixel[], int)"/>
		public TPixel[] ReadPixels<TPixel>(PixelLayout layout) where TPixel : unmanaged
		{
			TPixel[] result = null;
			ReadPixels(layout, ref result);
			return result;
		}

		/// <inheritdoc cref="ReadPixels{TPixel}(PixelLayout, TPixel[], int)"/>
		public int ReadPixels<TPixel>(ref TPixel[] buffer) where TPixel : unmanaged
		{
			var layout = GetPixelLayoutByAttribute(typeof(TPixel));
			return ReadPixels(layout, ref buffer);
		}

		/// <inheritdoc cref="ReadPixels{TPixel}(PixelLayout, TPixel[], int)"/>
		public int ReadPixels<TPixel>(PixelLayout layout, ref TPixel[] buffer) where TPixel : unmanaged
		{
			var pixelCount = Width * Height;
			if (buffer == null || buffer.Length < pixelCount)
				buffer = new TPixel[pixelCount];
			return ReadPixels(layout, buffer);
		}

		/// <inheritdoc cref="ReadPixels{TPixel}(PixelLayout, TPixel[], int)"/>
		public int ReadPixels<TPixel>(TPixel[] buffer, int offset = 0) where TPixel : unmanaged
		{
			var layout = GetPixelLayoutByAttribute(typeof(TPixel));
			return ReadPixels(layout, buffer, offset);
		}

		/// <summary>
		/// Read image pixels.
		/// </summary>
		/// <typeparam name="T">Unmanaged struct to hold the result. If no <see cref="PixelLayout"/> argument passed, this type should has a <see cref="PngPixelAttribute"/> attribute.</typeparam>
		/// <param name="layout">Describe the layout of pixel.</param>
		/// <param name="buffer">Buffer to hold the result. If passed by <c>ref</c> keyword, it wii be created or resized if necessary. Otherwise, it should not be <c>null</c> and size from <paramref name="offset"/> to end should at least <see cref="PixelCount"/></param>
		/// <param name="offset">The index in <paramref name="buffer"/> to start the pixel data.</param>
		/// <returns>If pass a buffer, the return value is the number of pixels read, which should be <see cref="PixelCount"/>. Otherwise, the return value is pixels read.</returns>
		public int ReadPixels<T>(PixelLayout layout, T[] buffer, int offset = 0) where T : unmanaged
		{
			AssertValidPixelLayout(layout, sizeof(T));
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));

			var height = Height;
			var pixelCount = PixelCount;

			if (buffer.Length < offset + pixelCount)
				throw new ArgumentException($"The buffer (offset: {offset}, length: {buffer.Length}) is too small for {pixelCount} pixels.");

			TransformForRead(layout);

			var rowBytes = RowBytes;
			var rowPtrArray = (byte**) Marshal.AllocHGlobal(sizeof(byte*) * height);
			if (rowPtrArray == null)
				throw new OutOfMemoryException("Fail to allocate unmanaged memory.");

			try
			{
				fixed (T* bufferPtr = buffer)
				{
					var basePtr = (byte*)(bufferPtr + offset);

					if (Settings.RevertImageY)
					{
						for (var row = height - 1; row >= 0; row--)
							rowPtrArray[height - 1 - row] = basePtr + rowBytes * row;
					}
					else
					{
						for (var row = 0; row < height; row++)
							rowPtrArray[row] = basePtr + rowBytes * row;
					}

					png_read_image(_pngPtr, rowPtrArray);
				}
			}
			finally
			{
				Marshal.FreeHGlobal((IntPtr)rowPtrArray);
			}

			return pixelCount;
		}

		private static PixelLayout GetPixelLayoutByAttribute(Type type)
		{
			var attribute = type.GetCustomAttribute<PngPixelAttribute>();
			if (attribute == null)
			{
				throw new InvalidOperationException(
					$"Cannot find {nameof(PngPixelAttribute)} on {type}. " +
					$"Add {nameof(PngPixelAttribute)} on {type} or pass a {nameof(PixelLayout)} struct.");
			}

			return attribute.PixelLayout;
		}

		private static void AssertValidPixelLayout(PixelLayout layout, int structSize)
		{
			if (layout.Channels <= 0 || layout.Channels > 4)
			{
				throw new NotSupportedException($"Not support for color struct with {layout.Channels} channels.");
			}

			if (layout.BitDepth != 8 && layout.BitDepth != 16)
			{
				// TODO: Support 1, 2, 4 bits gray scale
				throw new NotSupportedException($"Not support for color struct with {layout.BitDepth} bit depth.");
			}

			// TODO: Support using multiple structs to represent single pixel
			// e.g. fill byte[] with rgb format
			var pixelSize = layout.PixelBits / 8;
			if (pixelSize != structSize)
			{
				throw new ArgumentException($"Size of pixel struct({structSize}) does not match width {nameof(PixelLayout)}({pixelSize})");
			}
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

			png_read_update_info(_pngPtr, _infoPtr);
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
			var span = new Span<byte>(ptr, length);
			var readLength = _stream.Read(span);
			if (readLength != length)
				throw new LibPngException($"Attempted to read {length} bytes from stream but only read {readLength} bytes.");
		}

		public class PngSettings
		{
			/// <summary>
			/// Only used when convert a colorful image to gray.
			/// </summary>
			public RgbToGrayConfidence RgbToGrayConfidence = RgbToGrayConfidence.Default;

			/// <summary>
			/// If <c>true</c>, the origin of the image is at bottom-left.
			/// Otherwise, the original is at to-left by default.
			/// </summary>
			public bool RevertImageY;

			internal PngSettings()
			{
			}
		}
	}
}
