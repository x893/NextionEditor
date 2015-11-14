using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace NextionEditor
{
    public static class HmiOptions
    {
		public const ushort MaxCustomDataSize = 0xE00;
		public const uint FlashInfoFwBegin = 0xB4;
		public const uint FlashInfoFwEnd = 0xD3;
		public const byte DataStart_0xBE = 0xBE;

		public static int Language = 0;

		public static byte FirmwareMajor = 0;
		public static byte FirmwareMinor = 0x23;

		public static string AppDataPath;
		public static string AppDataBinPath;
		public static string RunFilePath;

		public static byte VersionMajor = 0;
		public static byte VersionMinor = 30;
		public static string VersionNew = "";
		public static Icon Icon;
		public static Encoding Encoding;

        public static int ClientUpTime = 0;
		public static string DowloadUrl = "";
		public static string DownloadPage = "";
		public static bool FindEndState = false;

        public static ushort ColorTransparent = 0xA8;
        public static ushort ColorTransparentReplace = 0xA9;
        public static bool OpenTransparent = false;
		public static int PopupId = 0;
        public static string PopupUrl = "";
		public static string SoftLogo = "";
		public static string SoftName = "";
		public static string UpText = "";

		public static int InfoAppSize = Marshal.SizeOf(typeof(InfoApp));
		public static int InfoFirmwareSize = Marshal.SizeOf(typeof(InfoFirmware));
		public static int InfoObjectSize = Marshal.SizeOf(typeof(InfoObject));
		public static int InfoPageSize = Marshal.SizeOf(typeof(InfoPage));
		public static int InfoPageObjectSize = Marshal.SizeOf(typeof(InfoPageObject));
		public static int InfoPictureSize = Marshal.SizeOf(typeof(InfoPicture));
		public static int InfoStringSize = Marshal.SizeOf(typeof(InfoString));
		public static int InfoFontSize = Marshal.SizeOf(typeof(InfoFont));
		public static int InfoAttributeSize = Marshal.SizeOf(typeof(InfoAttribute));
		public static int InfoNameSize = Marshal.SizeOf(typeof(InfoName));
    }
}

