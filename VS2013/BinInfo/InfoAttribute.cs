using System;
using System.Runtime.InteropServices;

namespace NextionEditor
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct InfoAttribute
    {
        public ushort Length;
        public ushort DataLength;
		public byte AttrType;		// HmiAttributeType
        public byte DataStart;
        public ushort Start;
        public byte IsBinding;
        public byte CanModify;
        public byte IsReturn;
        public uint MaxValue;
        public uint MinValue;
    }
}
