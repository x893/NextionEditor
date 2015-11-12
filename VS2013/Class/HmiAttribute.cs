using System;

namespace NextionEditor
{
	public class HmiAttribute
	{
		public InfoAttribute InfoAttribute;

		public byte[] Name = new byte[15];
		public byte[] Data = new byte[1];
		public byte[] Note = new byte[1];

		public HmiAttribute Clone()
		{
			HmiAttribute attr = new HmiAttribute();
			attr.InfoAttribute = this.InfoAttribute;

			attr.Name = new byte[Name.Length];
			attr.Data = new byte[Data.Length];
			attr.Note = new byte[Note.Length];

			Name.CopyTo(attr.Name, 0);
			Data.CopyTo(attr.Data, 0);
			Note.CopyTo(attr.Note, 0);

			return attr;
		}
	}
}
