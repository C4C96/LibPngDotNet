using System;
using System.IO;
using NUnit.Framework;

namespace LibPngDotNet.Tests
{
	public class PngEncoderTests
	{
		private bool _hasWarning;

		[SetUp]
		public void SetUp()
		{
			_hasWarning = false;
			PngEncoder.PngWarningEvent += OnPngWarningEvent;
			PngDecoder.PngWarningEvent += OnPngWarningEvent;
		}

		[TearDown]
		public void CleanUp()
		{
			PngEncoder.PngWarningEvent -= OnPngWarningEvent;
			PngDecoder.PngWarningEvent -= OnPngWarningEvent;
		}

#nullable enable
		private void OnPngWarningEvent(object? sender, string e)
		{
			_hasWarning = true;
		}
#nullable disable

		[Test]
		public void EncodeTest()
		{
			var pixels = new[]
			{
				new Rgb(1, 2, 3),
				new Rgb(233, 66, 123),
			};

			EncodeAndDecodeTest<Rgb, Rgb>(pixels, (lhs, rhs) => Assert.AreEqual(lhs, rhs));
		}

		[Test]
		public void ReverseRgbTest()
		{
			var pixels = new[]
			{
				new Bgr(1, 2, 3),
				new Bgr(233, 66, 123),
			};

			EncodeAndDecodeTest<Bgr, Rgb>(pixels, (bgr, rgb) => Assert.AreEqual(bgr.ToRgb(), rgb));
		}

		[Test]
		public void AlphaAtHeadTest()
		{
			var pixels = new[]
			{
				new Argb(123, 233, 43, 152),
				new Argb(1, 2, 3, 4),
			};

			EncodeAndDecodeTest<Argb, Rgba>(pixels, (argb, rgba) => Assert.AreEqual(argb.ToRgba(), rgba));
		}

		[Test]
		public void Encode16DepthTest()
		{
			var pixels = new[]
			{
				new Rgb16(1, 2, 3),
				new Rgb16(233, 66, 123),
				new Rgb16(2333, 6666, 1234),
			};

			EncodeAndDecodeTest<Rgb16, Rgb16>(pixels, (lhs, rhs) => Assert.AreEqual(lhs, rhs));
		}

		[Test]
		public void InvertYTest()
		{
			var pixels = new[]
			{
				new Rgb(1, 2, 3),
				new Rgb(233, 66, 123),
			};

			EncodeAndDecodeTest<Rgb, Rgb>(pixels, (lhs, rhs) => Assert.AreEqual(lhs, rhs), true);
		}

		private void EncodeAndDecodeTest<TEncode, TDecode>(TEncode[] source, Action<TEncode, TDecode> assert, bool invertY = false)
			where TEncode : unmanaged
			where TDecode : unmanaged
		{
			using var stream = new MemoryStream();
			using var encoder = PngEncoder.Open(stream);
			encoder.Settings.InvertY = invertY;
			encoder.Width = 1;
			encoder.Height = source.Length;
			encoder.WriteImage(source);

			stream.Seek(0, SeekOrigin.Begin);
			using var decoder = PngDecoder.Open(stream);
			var decoded = decoder.ReadPixels<TDecode>();

			Assert.AreEqual(source.Length, decoded.Length);
			for (var i = 0; i < source.Length; i++)
			{
				var sourceIndex = invertY ? source.Length - 1 - i : i;
				assert(source[sourceIndex], decoded[i]);
			}
			Assert.False(_hasWarning);
		}
	}
}
