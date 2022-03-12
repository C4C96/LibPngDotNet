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

		/// <summary>
		/// Image is gray scale with alpha channel.
		/// </summary>
		GrayAlpha = Gray | Alpha,

		/// <summary>
		/// Image is colorful, has red, green, blue and alpha channels.
		/// </summary>
		Rgba = Color | Alpha,
	}

	/// <summary>
	/// Interlacing types used in libpng.
	/// </summary>
	public enum InterlaceType : byte
	{
		/// <summary>
		/// Non-interlaced image.
		/// </summary>
		None = 0,

		/// <summary>
		/// Adam7 interlacing.
		/// </summary>
		Adam7 = 1,

		/// <summary>
		/// Not a valid value
		/// </summary>
		Last = 2,
	}

	/// <summary>
	/// Compression type(s?) used in libpng.
	/// </summary>
	public enum CompressionType : byte
	{
		/// <summary>
		/// Deflate method 8, 32K window.
		/// The only choice.
		/// </summary>
		Default = 0,
	}

	/// <summary>
	/// Filter types used in libpng.
	/// </summary>
	public enum FilterType : byte
	{
		/// <summary>
		/// Single row per-byte filtering.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Used only in MNG datastreams.
		/// </summary>
		IntrapixelDifferencing = 64,
	}

	/// <summary>
	/// How to handle CRC errors in ancillary and critical chunks, and whether to use the data contained therein.
	/// Note that it is impossible to "discard" data in a critical chunk.
	/// </summary>
	public enum CrcAction
	{
		/// <summary>
		/// error/quit for critical chunks.
		/// warn/discard data for ancillary chunks.
		/// </summary>
		Default,

		/// <summary>
		/// error/quit for critical chunks.
		/// error/quit for ancillary chunks.
		/// </summary>
		ErrorQuit,

		/// <summary>
		/// INVALID for critical chunks.
		/// warn/discard data for ancillary chunks.
		/// </summary>
		WarnDiscard,

		/// <summary>
		///  warn/use data for critical chunks.
		///  warn/use data for ancillary chunks.
		/// </summary>
		WarnUse,

		/// <summary>
		///  quiet/use data for critical chunks.
		///  quiet/use data for ancillary chunks.
		/// </summary>
		QuietUse,

		/// <summary>
		/// use current value for critical chunks.
		/// use current value for ancillary chunks.
		/// </summary>
		NoChange,
	}
}
