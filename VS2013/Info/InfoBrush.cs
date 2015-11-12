namespace NextionEditor
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct InfoBrush
    {
        public byte sta;
        public ushort X;
        public ushort Y;
        public ushort EndX;
        public ushort EndY;
        public byte SpacingX;
        public byte SpacingY;
        public ushort PointColor;
        public ushort BackColor;
        public byte FontId;
        public byte XCenter;
        public byte YCenter;
        public InfoPicture pic;
    }
}

