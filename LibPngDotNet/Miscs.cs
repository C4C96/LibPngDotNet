using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LibPngDotNet
{
	internal static unsafe class PngUtils
	{
		public static void AssertValidInput<T>(PixelLayout layout, ReadOnlySpan<T> buffer, int pixelCount) where T : unmanaged
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

			if (sizeof(T) * buffer.Length < layout.PixelBits * pixelCount / 8)
				throw new IndexOutOfRangeException($"The buffer ({sizeof(T)}*{buffer.Length}) is too small for {pixelCount} pixels.");
		}

		public static PixelLayout GetPixelLayoutByAttribute(Type type)
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
	}

	internal unsafe struct RowPointerArray : IDisposable
	{
		private readonly int _stride;
		private readonly int _rows;

		public byte** Pointer { get; private set; }

		public RowPointerArray(int stride, int rows)
		{
			_stride = stride;
			_rows = rows;
			Pointer = (byte**)Marshal.AllocHGlobal(sizeof(byte*) * rows);
			if (Pointer == null)
				throw new OutOfMemoryException($"Fail to allocate unmanaged memory of size {_stride * rows}.");
		}

		public void SetRowPointers(void* buffer, bool inverse)
		{
			var basePtr = (byte*)buffer;
			if (!inverse)
			{
				for (var row = 0; row < _rows; row++)
					Pointer[row] = basePtr + _stride * row;
			}
			else
			{
				for (var row = _rows - 1; row >= 0; row--)
					Pointer[_rows - 1 - row] = basePtr + _stride * row;
			}
		}

		public void Dispose()
		{
			Marshal.FreeHGlobal((IntPtr) Pointer);
			Pointer = null;
		}
	}
}
