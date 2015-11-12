using System;
using System.Runtime.InteropServices;

namespace NextionEditor
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct InfoApp
    {
        public byte IsPotrait;		// 0

        public byte VersionMajor;		// 1
        public byte VersionMinor;

        public byte FileType;			// 3

        public uint FirmwareStart;		// 4
        public uint FirmwareSize;		// 8

        public ushort ScreenWidth;		// C
        public ushort ScreenHeight;		// E

        public uint PictureImageStart;	// 10
        public uint FontImageStart;		// 14
        public uint StringDataStart;	// 18

        public ushort PageCount;		// 1C
        public ushort ObjectCount;		// 1E
        public ushort PictureCount;		// 20
        public ushort FontCount;		// 22
        public uint StringCount;		// 24

        public uint PageStart;			// 28
        public uint ObjectStart;		// 2C
        public uint PictureStart;		// 30
        public uint FontStart;			// 34
        public uint StringStart;		// 38
        public uint FontDataStart;		// 3C
        public uint ResourcesCRC;		// 40
        public uint SystemCRC;			// 44
										// 48 ...
    }
}

