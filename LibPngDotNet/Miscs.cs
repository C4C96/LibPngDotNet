using System;
using System.Reflection;
using System.Runtime.InteropServices;
#if !NET
using System.Runtime.CompilerServices;
#endif

namespace LibPngDotNet
{
	internal static unsafe class PngUtils
	{
		public static void AssertValidInput<T>(
			PixelLayout layout,
#if NET
			ReadOnlySpan<T> buffer,
#else
			T[] buffer,
			int offset,
#endif
			int pixelCount) where T : unmanaged
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

			var needBytes = layout.PixelBits * pixelCount / 8;
#if NET
			var bufferLength = buffer.Length;
#else
			var bufferLength = buffer.Length - offset;
#endif
			var bufferBytes = sizeof(T) * bufferLength;
			if (bufferBytes < needBytes)
				throw new IndexOutOfRangeException($"The buffer({bufferBytes} bytes) is too small for pixels ({needBytes} bytes).");
		}

#if !NET
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ResizeBuffer(ref byte[] buffer, int minSize)
		{
			if (buffer != null && buffer.Length >= minSize)
				return;

			var size = NextPowOf2(minSize);
			buffer = new byte[size];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int NextPowOf2(int num)
		{
			num--;
			num |= num >> 1;
			num |= num >> 2;
			num |= num >> 4;
			num |= num >> 8;
			num |= num >> 16;
			num++;
			return num;
		}
#endif

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
