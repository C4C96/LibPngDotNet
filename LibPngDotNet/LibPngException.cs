using System;

namespace LibPngDotNet
{
	/// <summary>
	/// Fatal error in PNG image of libpng - can't continue
	/// </summary>
	public class LibPngException : Exception
	{
		internal LibPngException(string message) : base(message)
		{
		}
	}
}
