using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		[TestCaseSource(nameof(GetPngCases))]
		public void GetBitDepthTest(PngCase png)
		{
			var path = GetPngPath(png.Name);
			using var decoder = PngDecoder.Open(path);
			Assert.False(_hasWarning);
			Assert.AreEqual(png.BitDepth, decoder.BitDepth);
		}

		[TestCaseSource(nameof(GetPngCases))]
		public void GetColorTypeTest(PngCase png)
		{
			var path = GetPngPath(png.Name);
			using var decoder = PngDecoder.Open(path);
			Assert.False(_hasWarning);
			Assert.AreEqual(png.ColorType, decoder.ColorType);
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

		private static IEnumerable<PngCase> GetPngCases()
		{
			foreach (var path in GetPngPaths())
			{
				var fileName = Path.GetFileNameWithoutExtension(path);
				var namePatterns = fileName.Split('-');

				var index = 0;

				var colorTypeText = namePatterns[index++];
				var colorType = colorTypeText switch
				{
					"gray" => ColorType.Gray,
					"palette" => ColorType.Palette | ColorType.Color,
					"rgb" => ColorType.Color,
					_ => throw new FormatException(colorTypeText),
				};

				if (namePatterns[index] == "alpha")
				{
					colorType |= ColorType.Alpha;
					index++;
				}

				var bitDepth = int.Parse(namePatterns[index]);

				yield return new PngCase(fileName, colorType, bitDepth);
			}
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
			yield return PixelLayout.Gray;
			yield return PixelLayout.GrayAlpha;
			yield return PixelLayout.AlphaGray;
			yield return PixelLayout.Rgb;
			yield return PixelLayout.Bgr;
			yield return PixelLayout.Rgba;
			yield return PixelLayout.Argb;
			yield return PixelLayout.Gbra;
			yield return PixelLayout.Agbr;

			yield return PixelLayout.Gray16;
			yield return PixelLayout.GrayAlpha16;
			yield return PixelLayout.AlphaGray16;
			yield return PixelLayout.Rgb16;
			yield return PixelLayout.Bgr16;
			yield return PixelLayout.Rgba16;
			yield return PixelLayout.Argb16;
			yield return PixelLayout.Gbra16;
			yield return PixelLayout.Agbr16;
		}

		public record PngCase(string Name, ColorType ColorType, int BitDepth);

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