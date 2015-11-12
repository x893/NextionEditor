using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NextionEditor
{
	// [StructLayout(LayoutKind.Sequential)]
	public class InfoRange
	{
		public int Begin;
		public int End;

		public InfoRange(ushort begin, ushort end)
		{
			Begin = begin;
			End = end;
		}
		public InfoRange()
		{
		}

		public InfoRange(int begin, int end)
		{
			Begin = begin;
			End = end;
		}

		public static InfoRange[] List(int size)
		{
			List<InfoRange> list = new List<InfoRange>(size);
			while(size > 0)
			{
				--size;
				list.Add(new InfoRange());
			}
			return list.ToArray();
		}

		public override string ToString()
		{
			return Begin.ToString() + ":" + End.ToString();
		}
	}
}
