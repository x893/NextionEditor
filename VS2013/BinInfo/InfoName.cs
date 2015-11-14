using System;
using System.Runtime.InteropServices;

namespace NextionEditor
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct InfoName
	{
		public fixed byte Name[14];
		//public ulong h;		// 8
		//public uint a;		// 4
		//public ushort b;	// 2
	}
}
