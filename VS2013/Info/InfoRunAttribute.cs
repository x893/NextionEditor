using System;
using System.Runtime.InteropServices;

namespace NextionEditor
{
	[StructLayout(LayoutKind.Sequential)]
	public struct InfoRunAttribute
	{
		public InfoAttribute AttInfo;
		public unsafe byte* Pz;
		public uint Value;
	}
}
