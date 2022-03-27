using System;

namespace LibPngDotNet
{
	/// <summary>
	/// Normalized confidence used when convert colorful image to gray scale.
	/// <code>
	/// gray = color.R * <see cref="R"/> + color.G * <see cref="G"/> + color.B * <see cref="B"/>
	/// </code>
	/// </summary>
	public readonly struct RgbToGrayConfidence
	{
		/// <summary>
		/// Normalized red channel confidence
		/// </summary>
		public double R { get; }

		/// <summary>
		/// Normalized green channel confidence
		/// </summary>
		public double G { get; }

		/// <summary>
		/// Normalized blue channel confidence
		/// </summary>
		public double B => 1.0 - R - G;

		/// <summary>
		/// Set normalized confidence.
		/// </summary>
		/// <param name="r">Normalized red channel confidence</param>
		/// <param name="g">Normalized green channel confidence</param>
		public RgbToGrayConfidence(double r, double g)
		{
			R = r;
			G = g;

			if (R < 0 || G < 0 || B < 0) throw new ArgumentOutOfRangeException();
		}

		/// <summary>
		/// Set confidence and auto normalize
		/// </summary>
		/// <param name="r">Relative red channel confidence</param>
		/// <param name="g">Relative green channel confidence</param>
		/// <param name="b">Relative blue channel confidence</param>
		public RgbToGrayConfidence(double r, double g, double b)
		{
			var sum = r + g + b;
			if (Math.Abs(sum) < 1e-6)
				throw new ArgumentException("The sum of input confidence is approximately equal to zero.");

			R = r / sum;
			G = g / sum;

			if (R < 0 || G < 0 || B < 0) throw new ArgumentOutOfRangeException();
		}

		/// <summary>
		/// Default confidence used in libpng.
		/// </summary>
		public static RgbToGrayConfidence Default => new RgbToGrayConfidence(6968.0 / 32768.0, 23434.0 / 32768.0);

		/// <inheritdoc />
		public override string ToString() => $"({R}, {G}, {B})";
	}
}
