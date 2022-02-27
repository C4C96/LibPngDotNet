using System;
using System.Runtime.InteropServices;

namespace LibPngDotNet
{
	internal struct LibPngVersion
	{
#pragma warning disable CS0649
		private IntPtr _handle;
#pragma warning restore CS0649

		public override string ToString()
		{
			return Marshal.PtrToStringAnsi(_handle);
		}
	}
}
