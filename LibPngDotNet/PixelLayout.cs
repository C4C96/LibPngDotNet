using System;
using System.Reflection;

namespace LibPngDotNet
{
	/// <summary>
	/// Describe pixel layout.
	/// </summary>
	public readonly struct PixelLayout
	{
		/// <summary>
		/// Channel count.
		/// 1: gray scale, 2: gray scale with alpha,
		/// 3: rgb, 4: rgba
		/// </summary>
		public readonly byte Channels;

		/// <summary>
		/// Bits per channel.
		/// </summary>
		public readonly byte BitDepth;

		/// <summary>
		/// Flags to describe pixel layout.
		/// </summary>
		public readonly PixelLayoutFlags Flags;

		/// <summary>
		/// Create a <see cref="PixelLayout"/>
		/// </summary>
		/// <param name="channels"></param>
		/// <param name="bitDepth"></param>
		/// <param name="flags"></param>
		public PixelLayout(byte channels, byte bitDepth, PixelLayoutFlags flags = PixelLayoutFlags.Default)
		{
			Channels = channels;
			BitDepth = bitDepth;
			Flags = flags;
		}

		/// <summary>
		/// Bits per pixel.
		/// </summary>
		public int PixelBits => BitDepth * Channels;

		/// <summary>
		/// Is gray scale, otherwise, is colorful.
		/// </summary>
		public bool IsGrayScale => Channels <= 2;

		/// <summary>
		/// Has alpha channel or not.
		/// </summary>
		public bool HasAlpha => (Channels & 0x1) == 0;

		/// <summary>
		/// Color type in png format.
		/// </summary>
		public ColorType PngColorType
		{
			get
			{
				var type = (ColorType) 0;
				if (!IsGrayScale)
					type |= ColorType.Color;
				if (HasAlpha)
					type |= ColorType.Alpha;
				return type;
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return GetNiceName() ??
			       $"{Channels} channels, {BitDepth} bits, {Flags}";
		}

		private string GetNiceName()
		{
			string suffix;
			switch (BitDepth)
			{
				case 8: suffix = null; break;
				case 16: suffix = "16"; break;
				default: return null;
			}

			string name;
			switch (Channels)
			{
				case 1:
					name = "Gray";
					break;
				case 2:
					name = (Flags & PixelLayoutFlags.AlphaAtHead) != 0 ? "AlphaGray" : "GrayAlpha";
					break;
				case 3:
					name = (Flags & PixelLayoutFlags.ReverseRgbOrder) != 0 ? "Bgr" : "Rgb";
					break;
				case 4:
					switch (Flags)
					{
						case PixelLayoutFlags.Default:
							name = "Rgba";
							break;
						case PixelLayoutFlags.ReverseRgbOrder:
							name = "Bgra";
							break;
						case PixelLayoutFlags.AlphaAtHead:
							name = "Argb";
							break;
						case PixelLayoutFlags.Agbr:
							name = "Abgr";
							break;
						default:
							throw new NotSupportedException();
					}

					break;
				default:
					return null;
			}

			return name + suffix;
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Channels << 24 | BitDepth << 16 | (int) Flags;
		}

		/// <summary>
		/// 1 channel(gray scale), 8 bits per channel.
		/// </summary>
		public static PixelLayout Gray => new PixelLayout(1, 8);
		/// <summary>
		/// 2 channels(gray scale, alpha), 8 bits per channel.
		/// </summary>
		public static PixelLayout GrayAlpha => new PixelLayout(2, 8);
		/// <summary>
		/// 2 channels(alpha, gray scale), 8 bits per channel.
		/// </summary>
		public static PixelLayout AlphaGray => new PixelLayout(2, 8, PixelLayoutFlags.AlphaAtHead);
		/// <summary>
		/// 3 channels(red, green blue), 8 bits per channel.
		/// </summary>
		public static PixelLayout Rgb => new PixelLayout(3, 8);
		/// <summary>
		/// 3 channels(blue, green, red), 8 bits per channel.
		/// </summary>
		public static PixelLayout Bgr => new PixelLayout(3, 8, PixelLayoutFlags.ReverseRgbOrder);
		/// <summary>
		/// 4 channels(red, green blue, alpha), 8 bits per channel.
		/// </summary>
		public static PixelLayout Rgba => new PixelLayout(4, 8);
		/// <summary>
		/// 4 channels(alpha, red, green blue), 8 bits per channel.
		/// </summary>
		public static PixelLayout Argb => new PixelLayout(4, 8, PixelLayoutFlags.AlphaAtHead);
		/// <summary>
		/// 4 channels(blue, green, red, alpha), 8 bits per channel.
		/// </summary>
		public static PixelLayout Bgra => new PixelLayout(4, 8, PixelLayoutFlags.ReverseRgbOrder);
		/// <summary>
		/// 4 channels(alpha, blue, green, red), 8 bits per channel.
		/// </summary>
		public static PixelLayout Agbr => new PixelLayout(4, 8, PixelLayoutFlags.Agbr);

		/// <summary>
		/// 1 channel(gray scale), 16 bits per channel.
		/// </summary>
		public static PixelLayout Gray16 => new PixelLayout(1, 16);
		/// <summary>
		/// 2 channels(gray scale, alpha), 16 bits per channel.
		/// </summary>
		public static PixelLayout GrayAlpha16 => new PixelLayout(2, 16);
		/// <summary>
		/// 2 channels(alpha, gray scale), 16 bits per channel.
		/// </summary>
		public static PixelLayout AlphaGray16 => new PixelLayout(2, 16, PixelLayoutFlags.AlphaAtHead);
		/// <summary>
		/// 3 channels(red, green blue), 16 bits per channel.
		/// </summary>
		public static PixelLayout Rgb16 => new PixelLayout(3, 16);
		/// <summary>
		/// 3 channels(blue, green, red), 16 bits per channel.
		/// </summary>
		public static PixelLayout Bgr16 => new PixelLayout(3, 16, PixelLayoutFlags.ReverseRgbOrder);
		/// <summary>
		/// 4 channels(red, green blue, alpha), 16 bits per channel.
		/// </summary>
		public static PixelLayout Rgba16 => new PixelLayout(4, 16);
		/// <summary>
		/// 4 channels(alpha, red, green blue), 16 bits per channel.
		/// </summary>
		public static PixelLayout Argb16 => new PixelLayout(4, 16, PixelLayoutFlags.AlphaAtHead);
		/// <summary>
		/// 4 channels(blue, green, red, alpha), 16 bits per channel.
		/// </summary>
		public static PixelLayout Bgra16 => new PixelLayout(4, 16, PixelLayoutFlags.ReverseRgbOrder);
		/// <summary>
		/// 4 channels(alpha, blue, green, red), 16 bits per channel.
		/// </summary>
		public static PixelLayout Agbr16 => new PixelLayout(4, 16, PixelLayoutFlags.Agbr);

		internal static PixelLayout GetLayout(string layoutName)
		{
			var property = typeof(PixelLayout).GetProperty(layoutName, BindingFlags.Public | BindingFlags.Static);
			if (property != null && property.PropertyType == typeof(PixelLayout))
				return (PixelLayout)property.GetValue(null);

			throw new InvalidOperationException($"Unknown layout name: {layoutName}");
		}
	}

	/// <summary>
	/// Flags for <see cref="PixelLayout"/>
	/// </summary>
	[Flags]
	public enum PixelLayoutFlags : ushort
	{
		/// <summary>
		/// Use default settings.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Only used when pixel has 3 or 4 channels.
		/// Set the order of channels to bgr.
		/// By default, the order is rgb.
		/// </summary>
		ReverseRgbOrder = 1,

		/// <summary>
		/// Only used when pixel has alpha channel.
		/// Set alpha channel to the head of pixel, like ARGB.
		/// By default, alpha channel is at end of pixel, like RGBA.
		/// </summary>
		AlphaAtHead = 2,

		/// <summary>
		/// <see cref="ReverseRgbOrder"/> and <see cref="AlphaAtHead"/>.
		/// So it must be Agbr format.
		/// </summary>
		Agbr = ReverseRgbOrder | AlphaAtHead,
	}

	/// <summary>
	/// Provide the <see cref="T:LibPngDotNet.PixelLayout"/> info about custom pixel struct.
	/// </summary>
	[AttributeUsage(AttributeTargets.Struct)]
	public class PngPixelAttribute : Attribute
	{
		private readonly PixelLayout? _pixelLayout;
		private readonly string _layoutName;

		internal PixelLayout PixelLayout => _pixelLayout ?? PixelLayout.GetLayout(_layoutName);

		/// <summary>
		/// Add an attribute to describe the pixel layout of custom struct
		/// </summary>
		public PngPixelAttribute(byte channels, byte bitDepth, PixelLayoutFlags flags)
		{
			_pixelLayout = new PixelLayout(channels, bitDepth, flags);
		}


		/// <summary>
		/// Add an attribute to describe the pixel layout of custom struct
		/// </summary>
		/// <param name="layoutName">The name of a build-in <see cref="T:LibPngDotNet.PixelLayout"/>, consider use <c>nameof</c> key word, like <c>nameof(PixelLayout.Rgb)</c></param>
		public PngPixelAttribute(string layoutName)
		{
			_layoutName = layoutName;
		}
	}
}
