using System.Runtime.InteropServices;

namespace NextionEditor
{
	[StructLayout(LayoutKind.Sequential)]
	public struct InfoTimer
	{
		public byte State;
		public ushort Value;
		public ushort MaxValue;
		public ushort CodeBegin;
	}
}