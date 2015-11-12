using System;
using System.Runtime.InteropServices;

namespace NextionEditor
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct InfoPicture
    {
        public byte IsPotrait;
        public ushort appid;
        public ushort name;
        public uint DataStart;
        public ushort W;
        public ushort H;
        public uint Size;
        public byte IsOne;
        public uint SizeZi;
        public byte ShowTime;
    }
}

