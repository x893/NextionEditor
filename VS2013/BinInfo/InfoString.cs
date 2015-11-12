namespace NextionEditor
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct InfoString
    {
        public uint Start;
        public ushort Size;
    }
}

