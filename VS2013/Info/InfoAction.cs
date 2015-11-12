using System;
using System.Runtime.InteropServices;

namespace NextionEditor
{
	[StructLayout(LayoutKind.Sequential)]
	public struct InfoAction
	{
		public ushort EndX;
		public ushort EndY;
		public byte Wrap;
	}
}
