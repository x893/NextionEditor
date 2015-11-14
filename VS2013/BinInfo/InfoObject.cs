namespace NextionEditor
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct InfoObject
    {
        public InfoName Name;
        public byte ObjType;
        public byte IsCustomData;
        public ushort AttributeStart;
        public ushort AttributeLength;
        public InfoPanel Panel;
        public ushort StringInfoStart;
        public ushort StringInfoEnd;
    }
}

