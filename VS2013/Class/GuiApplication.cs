using System;
using System.Xml.Serialization;

namespace NextionEditor
{
	public class UsartUpdata
	{
		public uint BaudRate;
		public byte State;
		public uint WriteLength;
		public uint AllLength;
		public ushort PageLength;
	}

	[XmlRoot("HmiApplication")]
	public class GuiApplication
	{
		[XmlElement("Application")]
		public InfoApp App;
		[XmlIgnore]
		public InfoPage PageInfo;
		[XmlIgnore]
		public InfoTouch Touch;
		[XmlIgnore]
		public InfoBrush BrushInfo;
		[XmlIgnore]
		public UsartUpdata Usart = new UsartUpdata();
		[XmlIgnore]
		public unsafe InfoPageObject* PageObjects;

		[XmlIgnore]
		public byte[] CustomData;
		[XmlIgnore]
		public uint AppDataBegin;
		[XmlIgnore]
		public ushort Delay;
		[XmlIgnore]
		public byte DownObjId;
		[XmlIgnore]
		public ushort Page;
		[XmlIgnore]
		public ushort PageDataPos;
		[XmlIgnore]
		public uint FlashClearadd;
		[XmlIgnore]
		public ushort HexIndex;
		[XmlIgnore]
		public byte MoveObjId;
		[XmlIgnore]
		public uint[] System;
		[XmlIgnore]
		public ushort OveMerrys;
		[XmlIgnore]
		public byte SendReturn;
		[XmlIgnore]
		public byte TimerIndex;
	}
}
