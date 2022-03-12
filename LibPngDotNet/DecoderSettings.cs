namespace LibPngDotNet
{
	/// <summary>
	/// Settings used in <see cref="PngDecoder"/>.
	/// </summary>
	public class DecoderSettings
	{
		/// <summary>
		/// If <c>true</c>, the origin of the image is at bottom-left.
		/// Otherwise, the original is at to-left by default.
		/// </summary>
		public bool InvertY;

		/// <summary>
		/// Only used when convert a colorful image to gray.
		/// </summary>
		public RgbToGrayConfidence RgbToGrayConfidence = RgbToGrayConfidence.Default;
	}
}
