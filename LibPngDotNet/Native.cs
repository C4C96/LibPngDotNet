// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
using System.Runtime.InteropServices;
using png_voidp = System.IntPtr;
using png_const_charp = System.IntPtr;
using png_const_structrp = System.IntPtr;
using png_structp = System.IntPtr;
using png_structrp = System.IntPtr;
using png_structpp = System.IntPtr;
using png_infop = System.IntPtr;
using png_inforp = System.IntPtr;
using png_infopp = System.IntPtr;
using png_const_inforp = System.IntPtr;

namespace LibPngDotNet
{
	/// <summary>
	/// Native API in libpng16.dll
	/// Referencing png.h
	/// </summary>
	internal static class Native
	{
		private const string DLL_NAME = @"libpng16";

		public delegate void png_error(png_structp png_ptr, [MarshalAs(UnmanagedType.LPStr)] string png_error);

		public unsafe delegate void png_rw(png_structp png_ptr, byte* ptr, int size);

		public delegate void png_flush(png_structp png_ptr);

		[DllImport(DLL_NAME)] // 4
		public static extern png_structp png_create_read_struct(png_const_charp user_png_ver, png_voidp error_ptr, png_error error_fn, png_error warn_fn);

		[DllImport(DLL_NAME)] // 5
		public static extern png_structp png_create_write_struct(png_const_charp user_png_ver, png_voidp error_ptr,
			png_error error_fn, png_error warn_fn);

		[DllImport(DLL_NAME)] // 18
		public static extern png_infop png_create_info_struct(png_const_structrp png_ptr);

		[DllImport(DLL_NAME)] // 20
		public static extern void png_write_info_before_PLTE(png_structrp png_ptr, png_const_inforp info_ptr);

		[DllImport(DLL_NAME)] // 21
		public static extern void png_write_info(png_structrp png_ptr, png_const_inforp info_ptr);

		[DllImport(DLL_NAME)] // 22
		public static extern void png_read_info(png_structrp png_ptr, png_inforp info_ptr);

		/// <summary>
		/// Expand paletted images to RGB,
		/// expand grayscale images of less than 8-bit depth to 8-bit depth,
		/// and expand tRNS chunks to alpha channels.
		/// </summary>
		[DllImport(DLL_NAME)] // 26
		public static extern void png_set_expand(png_structrp png_ptr);

		[DllImport(DLL_NAME)] // 221
		public static extern void png_set_expand_16(png_structrp png_ptr);

		[DllImport(DLL_NAME)] // 30
		public static extern void png_set_bgr(png_structrp png_ptr);

		[DllImport(DLL_NAME)] // 31
		public static extern void png_set_gray_to_rgb(png_structrp png_ptr);

		[DllImport(DLL_NAME)] // 32
		public static extern void png_set_rgb_to_gray(png_structrp png_ptr, int error_action, double red, double green);

		[DllImport(DLL_NAME)] // 36
		public static extern void png_set_strip_alpha(png_structrp png_ptr);

		[DllImport(DLL_NAME)] // 37
		public static extern void png_set_swap_alpha(png_structrp png_ptr);

		[DllImport(DLL_NAME)] // 39
		public static extern void png_set_filler(png_structrp png_ptr, uint filler, bool fillAfter);

		[DllImport(DLL_NAME)] // 40
		public static extern void png_set_add_alpha(png_structrp png_ptr, uint filler, bool fillAfter);

		[DllImport(DLL_NAME)] // 229
		public static extern void png_set_scale_16(png_structrp png_ptr);

		[DllImport(DLL_NAME)] // 54
		public static extern void png_read_update_info(png_structrp png_ptr, png_inforp info_ptr);

		[DllImport(DLL_NAME)] // 57
		public static extern unsafe void png_read_image(png_structrp png_ptr, byte** image);

		[DllImport(DLL_NAME)] // 60
		public static extern unsafe void png_write_image(png_structrp png_ptr, byte** image);

		[DllImport(DLL_NAME)] // 61
		public static extern void png_write_end(png_structrp png_ptr, png_inforp info_ptr);

		[DllImport(DLL_NAME)] // 63
		public static extern void png_destroy_info_struct(png_const_structrp png_ptr, ref png_infopp info_ptr_ptr);

		[DllImport(DLL_NAME)] // 64
		public static extern void png_destroy_read_struct(ref png_structpp png_ptr_ptr, ref png_infopp info_ptr_ptr, ref png_infopp end_info_ptr_ptr);

		[DllImport(DLL_NAME)] // 65
		public static extern void png_destroy_write_struct(ref png_structpp png_ptr_ptr, ref png_infopp info_ptr_ptr);

		[DllImport(DLL_NAME)] // 66
		public static extern void png_set_crc_action(png_structrp png_ptr, CrcAction crit_action, CrcAction ancil_action);

		[DllImport(DLL_NAME)] // 77
		public static extern void png_set_write_fn(png_structrp png_ptr, png_voidp io_ptr, png_rw write_data_fn, png_flush output_flush_fn);

		[DllImport(DLL_NAME)] // 78
		public static extern void png_set_read_fn(png_structrp png_ptr, png_voidp io_ptr, png_rw read_data_fn);

		[DllImport(DLL_NAME)] // 111
		public static extern int png_get_rowbytes(png_const_structrp png_ptr, png_const_inforp info_ptr);

		[DllImport(DLL_NAME)] // 113
		public static extern unsafe void png_set_rows(png_const_structrp png_ptr, png_inforp info_ptr, byte** row_pointers);

		[DllImport(DLL_NAME)] // 114
		public static extern byte png_get_channels(png_const_structrp png_ptr, png_const_inforp info_ptr);

		[DllImport(DLL_NAME)] // 115
		public static extern uint png_get_image_width(png_const_structrp png_ptr, png_const_inforp info_ptr);

		[DllImport(DLL_NAME)] // 116
		public static extern uint png_get_image_height(png_const_structrp png_ptr, png_const_inforp info_ptr);

		[DllImport(DLL_NAME)] // 117
		public static extern byte png_get_bit_depth(png_const_structrp png_ptr, png_const_inforp info_ptr);

		[DllImport(DLL_NAME)] // 118
		public static extern ColorType png_get_color_type(png_const_structrp png_ptr, png_const_inforp info_ptr);

		[DllImport(DLL_NAME)] // 119
		public static extern FilterType png_get_filter_type(png_const_structrp png_ptr, png_const_inforp info_ptr);

		[DllImport(DLL_NAME)] // 120
		public static extern InterlaceType png_get_interlace_type(png_const_structrp png_ptr, png_const_inforp info_ptr);

		[DllImport(DLL_NAME)] // 121
		[return: MarshalAs(UnmanagedType.U1)]
		public static extern CompressionType png_get_compression_type(png_const_structrp png_ptr, png_const_inforp info_ptr);

		// Notice: enums like color_type, here is int, which is byte in png_get_color_type
		[DllImport(DLL_NAME)] // 144
		public static extern void png_set_IHDR(png_const_structrp png_ptr, png_inforp info_ptr, int width, int height, int bit_depth, int color_type, int interlace_method, int compression_method, int filter_method);

		[DllImport(DLL_NAME)] // 183
		public static extern png_const_charp png_get_libpng_ver(png_const_structrp png_ptr = default);
	}
}
