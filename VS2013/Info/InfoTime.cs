namespace NextionEditor
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct InfoTime
    {
        public uint systemruntime;
        public uint guisystime;
        public uint movetime;
        public uint touchdowntime;
        public uint sleeptime;
        public uint sptime;
    }
}

