using System;
using System.Xml.Serialization;

namespace NextionEditor
{
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
		public InfoUsartUpdata Usart;
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
		public ushort HexStrIndex;
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
