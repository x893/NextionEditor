using System;
using System.Runtime.InteropServices;

namespace NextionEditor
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct InfoFont
	{
		public uint RES0;	// Not used
		public byte State;
		public byte Width;
		public byte Height;
		public byte CodeHStart;
		public byte CodeHEnd;
		public byte CodeLStart;
		public byte CodeLEnd;
		public uint Length;
		public ushort NameStart;
		public ushort NameEnd;
		public uint Size;
		public uint DataOffset;
	}
}
