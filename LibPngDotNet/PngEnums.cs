using System;

namespace LibPngDotNet
{
	/// <summary>
	/// Color type masks used in libpng.
	/// Note that not all combinations are legal.
	/// </summary>
	[Flags]
	public enum ColorType : byte
	{
		/// <summary>
		/// Image is gray scale.
		/// </summary>
		Gray = 0,

		/// <summary>
		/// Image is using palette.
		/// </summary>
		Palette = 1,

		/// <summary>
		/// Image is colorful, has red, green and blue channels. Otherwise, image is gray.
		/// </summary>
		Color = 2,

		/// <summary>
		/// Image has alpha channel.
		/// </summary>
		Alpha = 4,
	}
}
