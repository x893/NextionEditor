using System;
using System.Runtime.InteropServices;

namespace NextionEditor
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct InfoPanel
	{
		public ushort X;
		public ushort Y;
		public ushort EndX;
		public ushort EndY;
		public byte loadlei;
		public byte SendKey;
		public ushort Ref;
		public ushort Load;
		public ushort Down;
		public byte Up;
		public byte Slide;
	}
}