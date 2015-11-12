using System;
using System.Runtime.InteropServices;

namespace NextionEditor
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct InfoPageObject
	{
		public byte Visible;
		public byte TouchState;
		public byte RefreshFlag;
	}
}
