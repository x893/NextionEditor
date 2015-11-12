using System.Collections.Generic;
using System.Windows.Forms;

namespace NextionEditor
{
	public static class HmiObjType
	{
		public static byte OBJECT_TYPE_CURVE = 0;
		public static byte OBJECT_TYPE_SLIDER = 1;

		public static byte OBJECT_TYPE_END = 0x32;

		public static byte TIMER = 0x33;
		public static byte VAR = 0x34;
		public static byte BUTTON_T = 0x35;
		public static byte NUMBER = 0x36;
		public static byte BUTTON = 0x62;
		public static byte PROG = 0x6A;
		public static byte TOUCH = 0x6D;
		public static byte PICTURE = 0x70;
		public static byte PICTUREC = 0x71;
		public static byte TEXT = 0x74;
		public static byte PAGE = 0x79;
		public static byte POINTER = 0x7A;

		private static Dictionary<byte, string> Marks;

		public static string GetNamePrefix(byte mark)
		{
			if (Marks == null)
			{
				Marks = new Dictionary<byte, string>();
				Marks.Add(OBJECT_TYPE_CURVE, "s");
				Marks.Add(OBJECT_TYPE_SLIDER, "h");
				Marks.Add(PAGE, "page");
				Marks.Add(BUTTON, "b");
				Marks.Add(TEXT, "t");
				Marks.Add(PROG, "j");
				Marks.Add(PICTURE, "p");
				Marks.Add(PICTUREC, "q");
				Marks.Add(TOUCH, "m");
				Marks.Add(POINTER, "z");
				Marks.Add(TIMER, "tm");
				Marks.Add(VAR, "va");
				Marks.Add(BUTTON_T, "bt");
				Marks.Add(NUMBER, "n");
			}

			if (Marks.ContainsKey(mark))
				return Marks[mark];

			MessageBox.Show(string.Format("Unknown Object Type {0} 0x{0:X2}", mark));
			return string.Empty;
		}
	}
}
