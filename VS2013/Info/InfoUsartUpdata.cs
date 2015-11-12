using System.Runtime.InteropServices;

namespace NextionEditor
{
	[StructLayout(LayoutKind.Sequential)]
	public struct InfoUsartUpdata
	{
		public uint BaudRate;
		public byte State;
		public uint WriteLength;
		public uint AllLength;
		public ushort PageLength;
	}
}
