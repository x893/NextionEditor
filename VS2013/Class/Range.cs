using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NextionEditor
{
	public class Range
	{
		public int Begin;
		public int End;

		public Range(ushort begin, ushort end)
		{
			Begin = begin;
			End = end;
		}
		public Range()
		{
		}

		public Range(int begin, int end)
		{
			Begin = begin;
			End = end;
		}

		public static Range[] List(int size)
		{
			List<Range> list = new List<Range>(size);
			while(size > 0)
			{
				--size;
				list.Add(new Range());
			}
			return list.ToArray();
		}

		public override string ToString()
		{
			return Begin.ToString() + ":" + End.ToString();
		}
	}
}
