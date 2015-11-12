using System.Runtime.InteropServices;

namespace NextionEditor
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct InfoSliderParam
	{
		public byte RefFlag;
		public byte Mode;
		public byte BackType;
		public byte CursorType;
		public byte CursorWidth;
		public byte CursorHeight;
		public ushort BackPicId;
		public ushort CutsorPicId;
		public ushort NowVal;
		public ushort MaxVal;
		public ushort MinVal;
		public ushort LastPos;
		public ushort TouchPos;
		public ushort LastVal;
	}
}
