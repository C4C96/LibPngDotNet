using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace LibPngDotNet.Tests
{
	public class PngDecoderTests
	{
		private const string TestPngFolder = @"..\..\..\testpngs";

		private bool _hasWarning;

		[SetUp]
		public void SetUp()
		{
			_hasWarning = false;
			PngDecoder.PngWarningEvent += OnPngWarningEvent;
		}

		[TearDown]
		public void Cleanup()
		{
			PngDecoder.PngWarningEvent -= OnPngWarningEvent;
		}

#nullable enable
		private void OnPngWarningEvent(object? sender, string message)
		{
			_hasWarning = true;
		}
#nullable disable

		[Test]
		public void NullArgumentTest()
		{
			Assert.Throws<ArgumentNullException>(() => PngDecoder.Open((Stream) null));
			Assert.Throws<ArgumentNullException>(() => PngDecoder.Open((string) null));
		}

		[Test]
		public void NotPngFileTest()
		{
			var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
			using var stream = new MemoryStream(bytes);
			Assert.Throws<LibPngException>(() => PngDecoder.Open(stream));
		}

		[TestCaseSource(nameof(GetCrasherCases))]
		public void LibPngCrashTest(string path, bool fatal)
		{
			if (fatal)
				Assert.Throws<LibPngException>(() => PngDecoder.Open(path));
			else
			{
				PngDecoder.Open(path);
				Assert.True(_hasWarning);
			}
		}

		[TestCase("rgb-16", ExpectedResult = 16)]
		[TestCase("rgb-8", ExpectedResult = 8)]
		[TestCase("gray-4", ExpectedResult = 4)]
		[TestCase("gray-2", ExpectedResult = 2)]
		[TestCase("gray-1", ExpectedResult = 1)]
		public byte GetBitDepthTest(string pngName)
		{
			return GetDecoderProperty(pngName, d => d.BitDepth);
		}

		[TestCase("rgb-16", ExpectedResult = ColorType.Color)]
		[TestCase("rgb-alpha-16", ExpectedResult = ColorType.Rgba)]
		[TestCase("gray-16", ExpectedResult = ColorType.Gray)]
		[TestCase("gray-alpha-16", ExpectedResult = ColorType.GrayAlpha)]
		[TestCase("palette-8", ExpectedResult = ColorType.Palette | ColorType.Color)]
		public ColorType GetColorTypeTest(string pngName)
		{
			return GetDecoderProperty(pngName, d => d.ColorType);
		}

		[TestCase("rgb-16", ExpectedResult = CompressionType.Default)]
		public CompressionType GetCompressionTypeTest(string pngName)
		{
			return GetDecoderProperty(pngName, d => d.Compression);
		}

		[TestCase("rgb-16", ExpectedResult = FilterType.Default)]
		public FilterType GetFilterTypeTest(string pngName)
		{
			return GetDecoderProperty(pngName, d => d.FilterType);
		}

		[TestCase("rgb-16", ExpectedResult = InterlaceType.None)]
		public InterlaceType GetInterlaceTypeTest(string pngName)
		{
			return GetDecoderProperty(pngName, d => d.InterlaceType);
		}

		[TestCase("rgb-16")]
		public void InvertYTest(string pngName)
		{
			Rgb[] pixels, invertedPixels;
			int width, height;

			var path = GetPngPath(pngName);
			using (var decoder = PngDecoder.Open(path))
			{
				width = decoder.Width;
				height = decoder.Height;
				pixels = decoder.ReadPixels<Rgb>();
			}

			using (var decoder = PngDecoder.Open(path))
			{
				decoder.Settings.InvertY = true;
				invertedPixels = decoder.ReadPixels<Rgb>();
			}

			Assert.AreEqual(width * height, pixels.Length);
			Assert.AreEqual(width * height, invertedPixels.Length);
			
			for (var y = 0; y < height; y++)
			for (var x = 0; x < width; x++)
			{
				var index = x + y * width;
				var invertedIndex = x + (height - 1 - y) * width;
				Assert.AreEqual(pixels[index], invertedPixels[invertedIndex]);
			}
		}

		[TestCaseSource(nameof(GetReadPixelsTestCases))]
		public void ReadPixelsTest(string fileName, PixelLayout pixelLayout)
		{
			var path = GetPngPath(fileName);

			using var decoder = PngDecoder.Open(path);
			ReadPixels(decoder, pixelLayout);

			Assert.False(_hasWarning);
		}

		private static IEnumerable<object[]> GetReadPixelsTestCases()
		{
			//var fileName = @"rgb-alpha-16";
			//var layout = PixelLayout.Gray;

			foreach (var fileName in GetPngNames())
			foreach (var layout in GetPixelLayouts())
			{
				yield return new object[] { fileName, layout };
			}
		}

		private void ReadPixels(PngDecoder decoder, PixelLayout layout)
		{
			var _8Bit = layout.BitDepth == 8;
			switch (layout.Channels)
			{
				case 1:
					if (_8Bit)
						decoder.ReadPixels<byte>(layout);
					else
						decoder.ReadPixels<ushort>(layout);
					break;
				case 2:
					if (_8Bit)
						decoder.ReadPixels<ValueTuple<byte, byte>>(layout);
					else
						decoder.ReadPixels<ValueTuple<ushort, ushort>>(layout);
					break;
				case 3: // sizeof(ValueTuple<byte,byte,byte>) = 4
					if (_8Bit)
						decoder.ReadPixels<Channel3Bits8>(layout);
					else
						decoder.ReadPixels<Channel3Bits16>(layout);
					break;
				case 4:
					if (_8Bit)
						decoder.ReadPixels<ValueTuple<byte, byte, byte, byte>>(layout);
					else
						decoder.ReadPixels<ValueTuple<ushort, ushort, ushort, ushort>>(layout);
					break;
			}
		}

		private T GetDecoderProperty<T>(string pngName, Func<PngDecoder, T> getProperty)
		{
			var path = GetPngPath(pngName);
			using var decoder = PngDecoder.Open(path);
			Assert.False(_hasWarning);
			return getProperty(decoder);
		}

		private static string GetPngPath(string fileName)
		{
			return Path.Combine(TestPngFolder, fileName + ".png");
		}

		private static IEnumerable<string> GetPngNames()
		{
			return GetPngPaths().Select(Path.GetFileNameWithoutExtension);
		}

		private static IEnumerable<string> GetPngPaths()
		{
			return Directory.GetFiles(TestPngFolder, "*.png");
		}

		private static IEnumerable<object[]> GetCrasherCases()
		{
			var folder = Path.Combine(TestPngFolder, "crashers");
			var paths = Directory.GetFiles(folder, "*.png");
			foreach (var path in paths)
			{
				var fileName = Path.GetFileNameWithoutExtension(path);
				bool fatal;
				switch (fileName)
				{
					// TODO: these 2 case should cause warning?
					case "badadler": continue;
					case "badcrc": continue;
					case "empty_ancillary_chunks":
						fatal = false;
						break;
					default:
						fatal = true;
						break;
				}

				yield return new object[] { path, fatal };
			}
		}

		private static IEnumerable<PixelLayout> GetPixelLayouts()
		{
			var properties = typeof(PixelLayout).GetProperties(BindingFlags.Public | BindingFlags.Static);

			foreach (var property in properties)
			{
				if (property.PropertyType == typeof(PixelLayout))
					yield return (PixelLayout) property.GetValue(null);
			}
		}

		public struct Channel3Bits8
		{
			public byte First;
			public byte Second;
			public byte Third;
			public override string ToString() => $"({First}, {Second}, {Third})";
		}

		public struct Channel3Bits16
		{
			public ushort First;
			public ushort Second;
			public ushort Third;
			public override string ToString() => $"({First}, {Second}, {Third})";
		}
	}
}