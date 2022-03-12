namespace LibPngDotNet.Tests
{
	[PngPixel(nameof(PixelLayout.Rgb))]
	public struct Rgb
	{
		public byte R;
		public byte G;
		public byte B;

		public Rgb(byte r, byte g, byte b)
		{
			R = r;
			G = g;
			B = b;
		}

		public override string ToString()
		{
			return $"({R}, {G}, {B})";
		}
	}

	[PngPixel(nameof(PixelLayout.Rgba))]
	public struct Rgba
	{
		public byte R;
		public byte G;
		public byte B;
		public byte A;

		public Rgba(byte r, byte g, byte b, byte a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public override string ToString()
		{
			return $"({R}, {G}, {B}, {A})";
		}
	}

	[PngPixel(nameof(PixelLayout.Argb))]
	public struct Argb
	{
		public byte A;
		public byte R;
		public byte G;
		public byte B;

		public Argb(byte a, byte r, byte g, byte b)
		{
			A = a;
			R = r;
			G = g;
			B = b;
		}

		public Rgba ToRgba()
		{
			return new Rgba(R, G, B, A);
		}

		public override string ToString()
		{
			return $"({A}, {R}, {G}, {B})";
		}
	}

	[PngPixel(nameof(PixelLayout.Bgr))]
	public struct Bgr
	{
		public byte B;
		public byte G;
		public byte R;

		public Bgr(byte b, byte g, byte r)
		{
			B = b;
			G = g;
			R = r;
		}

		public Rgb ToRgb()
		{
			return new Rgb(R, G, B);
		}

		public override string ToString()
		{
			return $"({B}, {G}, {R})";
		}
	}

	[PngPixel(nameof(PixelLayout.Rgb16))]
	public struct Rgb16
	{
		public ushort R;
		public ushort G;
		public ushort B;

		public Rgb16(ushort r, ushort g, ushort b)
		{
			R = r;
			G = g;
			B = b;
		}

		public override string ToString()
		{
			return $"({R}, {G}, {B})";
		}
	}
}
