namespace NextionEditor
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct InfoFirmware
    {							// Offset
        public uint TypeCRC;	//	0xB4	0x64116DFF
        public ushort Version;	//	0xB8	0x0023 (0.35)
        public uint Size;		//	0xBA	0x0000DCC8
        public uint CRC;		//	0xBE	0xE737BAE6
        public byte Pass;		//	0xC2	4D (need XOR for normal)
        public uint Reserve0;	//	0xC3	0
        public uint Reserve1;	//	0xC7	0
        public uint Reserve2;	//	0xCB	0
        public uint Reserve3;	//	0xCF	0
								//	0xD3
    }
}

