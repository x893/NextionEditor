namespace NextionEditor
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct InfoPage
    {
        public InfoName Name;
        public byte res0;
        public byte ObjCount;
        public ushort ObjStart;
        public ushort ObjEnd;
        public ushort InstStart;
        public ushort InstEnd;
    }
}

