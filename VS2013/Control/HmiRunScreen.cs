using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace NextionEditor
{
	public class HmiRunScreen : UserControl
	{
		#region Classes
		private class HmiTime
		{
			public uint SystemRuntime;
			public uint Timer20ms;
			public uint MoveTime;
		}
		private class HmiSysTimer
		{
			public byte ThSleepUp;
			public uint ThSp;
			public uint UsSp;
		}
		private class ComQueue
		{
			public byte State;
			public byte CodePause;
			public Range[] Queue;
			public ushort RecvPos;
			public byte DiulieYunxing;
			public byte Current;
		}

		private class LcdDevice
		{
			public ushort Width;
			public ushort Height;
			public byte Draw;
			public ushort DrawColor;
		}
		private class HmiScreen
		{
			public int Xpos;
			public int Ypos;
			public int EndX;
			public int EndY;
			public int DX;
			public int DY;
		}
		private class HmiTimer
		{
			public byte State;
			public ushort Value;
			public ushort MaxValue;
			public ushort CodeBegin;
		}
		private struct AttributeRun
		{
			public InfoAttribute AttrInfo;
			public unsafe byte* Pz;
			public uint Value;
		}
		#endregion

		#region Public
		public event EventHandler ObjChange;
		public event EventHandler ObjMouseUp;
		public event EventHandler SendByte;
		public delegate void SendRunCodeHandler(string cmd);
		public event SendRunCodeHandler SendRunCode;

		public GuiApplication GuiApp = new GuiApplication();
		public GuiObjControl[] GuiObjControl = new GuiObjControl[2];

		public int ThisBmpIndex = 0;
		public Bitmap[] ThisBmp = new Bitmap[2];
		public bool IsEditor = true;
		public bool IsTransparent = false;
		public bool LcdFirst = false;
		public HmiTPDev TPDevInf = new HmiTPDev();
		public HmiObjectEdit HmiObjectEdit;
		#endregion

		#region Private
		private const byte dataFrom_RAM = 0x00;
		private const byte dataFrom_Buf = 0x01;
		private const byte dataFrom_We = 0x14;
		private const byte dataFrom_Sys_Baud = 0x68;
		private const byte dataFrom_Sys_Bauds = 0x69;
		private const byte dataFrom_Sys_Bkcmd = 0x67;
		private const byte dataFrom_Sys_Bl = 0x65;
		private const byte dataFrom_Sys_IntBl = 0x66;
		private const byte dataFrom_Sys_SpaX = 0x6A;
		private const byte dataFrom_Sys_SpaY = 0x6B;
		private const byte dataFrom_Sys_Ussp = 0x6C;
		private const byte dataFrom_Sys_ThSp = 0x6D;
		private const byte dataFrom_Sys_ThUp = 0x6E;
		private const byte dataFrom_Sys_X = 0xC8;
		private const byte dataFrom_Null = 0xFF;

		private ushort m_TPDownEnter = 0;
		private ushort m_TPUpEnter = 0;
		private HmiApplication m_app;
		private HmiSysTimer m_sysTimer = new HmiSysTimer();
		private HmiTime m_hmiTime = new HmiTime();
		private bool m_isPortrait = false;
		private byte m_runState = 1;
		private string m_binPath;
		private StreamReader m_reader = null;
		private byte[] m_comBuffer = new byte[1024];
		private int m_ComQueueLength = 100;
		private byte m_comEnd = 0;
		private ComQueue m_ComQueue = new ComQueue();
		private InfoCodeResults m_cgCode = new InfoCodeResults();

		private Graphics m_gc;
		private GuiCurve m_guiCurve;
		private GuiSlider m_guiSlider;

		private byte[] m_hexStrBuf = new byte[1024];
		private ushort[] m_hexStrPos = new ushort[4];

		private IContainer components = null;
		private Label label1;
		private Label label2;
		private Label label3;
		private Label label4;
		private Label label5;
		private Panel panelScreen;
		private LcdDevice m_lcdDevInfo = new LcdDevice();
		private HmiScreen m_screen = new HmiScreen();
		private HmiTimer[] m_timers = new HmiTimer[]
			{	new HmiTimer(),
				new HmiTimer(),
				new HmiTimer(),
				new HmiTimer(),
				new HmiTimer()
			};
		private InfoFont m_fontInfo;

		private Point m_mouse_pos;
		private byte m_systemLength = 4;
		private Thread m_main_thread;
		private Thread m_timer_ms;

		private ushort m_zoom_m = 8;
		private ushort m_zoom_d = 8;
		#endregion

		#region Constructor
		public HmiRunScreen()
		{
			InitializeComponent();
		}
		#endregion

		#region AttributeAdd
		private unsafe byte AttributeAdd(byte[] buf, ref AttributeRun b1, ref AttributeRun b2, ref AttributeRun b3, byte operation)
		{
			fixed (byte* pb = &buf[0])
			fixed (AttributeRun* aref1 = &b1)
			fixed (AttributeRun* aref2 = &b2)
			fixed (AttributeRun* aref3 = &b3)
				return AttributeAdd(pb, aref1, aref2, aref3, operation);
		}
		private unsafe byte AttributeAdd(byte* buf, AttributeRun* attr1, AttributeRun* attr2, AttributeRun* attr3, byte operation)
		{
			ushort num = 0;
			if (attr1->AttrInfo.AttrType > 9
				&& attr2->AttrInfo.AttrType > 9
				&& attr3->AttrInfo.AttrType > 9
				&& operation == 0x2b
				)
			{
				if ((attr1->AttrInfo.DataStart == dataFrom_RAM) && (attr1->Pz == attr3->Pz))
				{
					if (setAttr(attr2, attr3, operation) > 0)
						return 1;
				}
				else if ((attr2->AttrInfo.DataStart == dataFrom_RAM) && (attr2->Pz == attr3->Pz))
				{
					if (setAttr(attr1, attr3, operation) > 0)
						return 1;
				}
				else
				{
					if (setAttr(attr1, attr3, 0) == 0)
					{
						sendReturnErr(0x1b);
						return 0;
					}
					if (setAttr(attr2, attr3, operation) > 0)
						return 1;
				}
			}
			else
			{
				if (attr2->AttrInfo.AttrType < HmiAttributeType.String
				 && attr3->AttrInfo.AttrType == HmiAttributeType.String
					)
				{
					num = getStringLength(attr3->Pz);
					if (num <= attr2->Value)
					{
						attr3->Pz[0] = 0;
						return 1;
					}
					attr3->Pz[num - attr2->Value] = 0;
					return 1;
				}
				if (attr1->AttrInfo.AttrType < HmiAttributeType.String
				 && attr2->AttrInfo.AttrType < HmiAttributeType.String
				 && attr3->AttrInfo.AttrType < HmiAttributeType.String
					)
				{
					if (operation == '+')
					{
						attr3->Value = attr1->Value + attr2->Value;
						setAttr(attr3, attr3, 0);
						return 1;
					}
					if (operation == '-')
					{
						attr3->Value = attr1->Value - attr2->Value;
						setAttr(attr3, attr3, 0);
						return 1;
					}
					if (operation == '*')
					{
						attr3->Value = attr1->Value * attr2->Value;
						setAttr(attr3, attr3, 0);
						return 1;
					}
					if (operation == '/')
					{
						attr3->Value = (attr2->Value == 0) ? 0 : (attr1->Value / attr2->Value);
						setAttr(attr3, attr3, 0);
						return 1;
					}
				}
			}
			sendReturnErr(0x1b);
			return 0;
		}
		#endregion

		#region attributeConvert
		private unsafe byte attributeConvert(ref AttributeRun b1, ref AttributeRun b2, byte length)
		{
			fixed (AttributeRun* runattinfRef = &b1)
			fixed (AttributeRun* runattinfRef2 = &b2)
				return attributeConvert(runattinfRef, runattinfRef2, length);
		}
		private unsafe byte attributeConvert(AttributeRun* b1, AttributeRun* b2, byte length)
		{
			AttributeRun runAttrInf = new AttributeRun();
			if (b1->AttrInfo.AttrType < HmiAttributeType.String && b2->AttrInfo.AttrType == HmiAttributeType.String)
			{
				if (b2->AttrInfo.DataStart != dataFrom_RAM)
				{
					sendReturnErr(0x1b);
					return 0;
				}
				if (length == 0)
					length = getIntStrLen(b1->Value);

				if (b2->AttrInfo.Length <= length)
					length = (byte)(b2->AttrInfo.Length - 1);

				intToStr(b1->Value, b2->Pz, length, 1);
			}
			else if (b1->AttrInfo.AttrType == HmiAttributeType.String && b2->AttrInfo.AttrType < HmiAttributeType.String)
			{
				b1->Value = StrToInt(b1->Pz, length);
				if (b2->AttrInfo.DataStart == dataFrom_RAM)
				{
					if (b1->Value <= b2->AttrInfo.MaxValue && b1->Value >= b2->AttrInfo.MinValue)
					{
						if (b2->AttrInfo.Length > 4)
							memcpy(b2->Pz, (byte*)&b1->Value, 4);
						else
							memcpy(b2->Pz, (byte*)&b1->Value, b2->AttrInfo.Length);
					}
				}
				else if (b2->AttrInfo.DataStart > 100 && b2->AttrInfo.DataStart < 200)
				{
					runAttrInf.AttrInfo.DataStart = dataFrom_We;
					runAttrInf.Value = b1->Value;
					runAttrInf.AttrInfo.AttrType = HmiAttributeType.Other;
					runAttrInf.AttrInfo.Length = 4;
					runAttrInf.AttrInfo.DataLength = 4;
					return setAttr(&runAttrInf, b2, 0);
				}
			}
			else
			{
				sendReturnErr(0x1b);
				return 0;
			}
			return 1;
		}
		#endregion

		#region clearComCode
		private void clearComCode()
		{
			m_comEnd = 0;
			m_ComQueue.CodePause = 0xff;
			m_ComQueue.DiulieYunxing = 0;
			m_ComQueue.Current = 0;
			m_ComQueue.RecvPos = 0;
			m_ComQueue.State = 0;

			for (int i = 0; i < m_ComQueueLength; i++)
			{
				m_ComQueue.Queue[i].Begin = 0xffff;
				m_ComQueue.Queue[i].End = 0xffff;
			}
			m_ComQueue.Queue[m_ComQueue.Current].Begin = 0;
		}
		#endregion

		#region
		private void clearHexStr()
		{
			for (int i = 0; i < 4; i++)
				m_hexStrPos[i] = 0xffff;
		}
		#endregion
		#region
		private unsafe void setTouchState(byte touchState)
		{
			for (byte i = 0; i < GuiApp.PageInfo.ObjCount; i++)
				GuiApp.PageObjects[i].TouchState = touchState;
		}
		#endregion
		#region
		public void ClearBackground(ushort x, ushort y, ushort w, ushort h)
		{
			if (GuiApp.BrushInfo.sta == 1)
				LCD_Fill(x, y, w, h, GuiApp.BrushInfo.BackColor);
			else if (GuiApp.BrushInfo.sta == 0)
				picq(x, y, w, h, x, y, ref GuiApp.BrushInfo.pic);
			else if (GuiApp.BrushInfo.sta == 2)
				picq(x, y, w, h, (ushort)(x - GuiApp.BrushInfo.X), (ushort)(y - GuiApp.BrushInfo.Y), ref GuiApp.BrushInfo.pic);
		}
		#endregion

		#region clearTimer()
		private void clearTimer()
		{
			for (int i = 0; i < m_timers.Length; i++)
			{
				m_timers[i].State = 0;
				m_timers[i].MaxValue = 0xffff;
			}
			GuiApp.TimerIndex = 0xff;
		}
		#endregion

		#region CodeExecute(byte[] buf, InfoRange posCode, int bitmapIndex)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="range"></param>
		/// <param name="bitmapIndex"></param>
		/// <returns></returns>
		/*
		public unsafe bool CodeExecute(byte[] buf, ref InfoRange range, int bitmapIndex)
		{
			fixed(byte* refBuf = buf)
			{
				return CodeExecute(refBuf, range, bitmapIndex);
			}
		}
		*/
		public unsafe bool CodeExecute(byte[] buf, Range range, int bitmapIndex)
		{
			ushort num4;
			uint num7;
			Range pos = new Range(range.Begin, range.End);
			AttributeRun[] infoRunAttrs = new AttributeRun[3];
			byte[] buffer = new byte[50];
			byte num = 0;
			ushort index = 0;
			ushort num3 = 0;
			byte length = 0;
			ThisBmpIndex = bitmapIndex;

			#region add
			if (Utility.IndexOf(buf, "add ", range) != 0xFFFF)
			{
				pos.Begin += 4;
				if (!m_guiCurve.GuiCruveCmd(buf, range, pos))
					sendReturnErr(0x12);
				return false;
			}
			#endregion

			#region init
			if (RunCode(buf, "init ", 1, range))
			{
				guiObjectInit((byte)GetU16(m_cgCode.CodeResults[0], buf));
				return true;
			}
			#endregion

			#region sleep
			if (RunCode(buf, "sleep=", 1, range))
			{
				return true;
			}
			#endregion

			#region cls
			if (RunCode(buf, "cls ", 1, range))
			{
				GuiApp.BrushInfo.BackColor = GetU16(m_cgCode.CodeResults[0], buf);
				lcdClear(GetU16(m_cgCode.CodeResults[0], buf));
				return true;
			}
			#endregion

			#region picq
			if (RunCode(buf, "picq ", 5, range))
			{
				return showPicQ(
					GetU16(m_cgCode.CodeResults[0], buf),
					GetU16(m_cgCode.CodeResults[1], buf),
					GetU16(m_cgCode.CodeResults[2], buf),
					GetU16(m_cgCode.CodeResults[3], buf),
					GetU16(m_cgCode.CodeResults[4], buf)
					);
			}
			#endregion

			#region xpic
			if (RunCode(buf, "xpic ", 7, range))
			{
				return ShowXPic(
					GetU16(m_cgCode.CodeResults[0], buf),
					GetU16(m_cgCode.CodeResults[1], buf),
					GetU16(m_cgCode.CodeResults[2], buf),
					GetU16(m_cgCode.CodeResults[3], buf),
					GetU16(m_cgCode.CodeResults[4], buf),
					GetU16(m_cgCode.CodeResults[5], buf),
					GetU16(m_cgCode.CodeResults[6], buf)
					);
			}
			#endregion

			#region pic
			if (RunCode(buf, "pic ", 3, range))
			{
				return ShowPic(
					GetU16(m_cgCode.CodeResults[0], buf),
					GetU16(m_cgCode.CodeResults[1], buf),
					GetU16(m_cgCode.CodeResults[2], buf)
					);
			}
			#endregion

			#region xstr
			if (RunCode(buf, "xstr ", 11, range))
			{
				GuiApp.BrushInfo.X = GetU16(m_cgCode.CodeResults[0], buf);
				GuiApp.BrushInfo.Y = GetU16(m_cgCode.CodeResults[1], buf);
				GuiApp.BrushInfo.EndX = (ushort)((GuiApp.BrushInfo.X + GetU16(m_cgCode.CodeResults[2], buf)) - 1);
				GuiApp.BrushInfo.EndY = (ushort)((GuiApp.BrushInfo.Y + GetU16(m_cgCode.CodeResults[3], buf)) - 1);
				GuiApp.BrushInfo.FontId = (byte)GetU16(m_cgCode.CodeResults[4], buf);
				GuiApp.BrushInfo.PointColor = GetU16(m_cgCode.CodeResults[5], buf);

				if ((buf[m_cgCode.CodeResults[6].Begin] == 'N') && (buf[m_cgCode.CodeResults[6].End] == 'L'))
				{
					GuiApp.BrushInfo.sta = 3;
				}
				else
				{
					GuiApp.BrushInfo.BackColor = GetU16(m_cgCode.CodeResults[6], buf);
					GuiApp.BrushInfo.sta = (byte)GetU16(m_cgCode.CodeResults[9], buf);
				}

				GuiApp.BrushInfo.XCenter = (byte)GetU16(m_cgCode.CodeResults[7], buf);
				GuiApp.BrushInfo.YCenter = (byte)GetU16(m_cgCode.CodeResults[8], buf);
				pos.Begin = m_cgCode.CodeResults[10].Begin;
				pos.End = m_cgCode.CodeResults[10].End;
				num4 = getStringAttribute(buf, pos, ref infoRunAttrs[0]);
				if (infoRunAttrs[0].AttrInfo.DataStart == dataFrom_Null)
				{
					sendReturnErr(0x1A);
					return false;
				}

				if (infoRunAttrs[0].AttrInfo.AttrType > 9)
					return XstringHZK(infoRunAttrs[0].Pz);

				fixed (byte* numRef = buffer)
				{
					pos.Begin = (ushort)(m_cgCode.CodeResults[10].End + 2);
					pos.End = range.End;
					if (pos.End >= pos.Begin)
					{
						num7 = GetU32(pos, buf);
						if (num7 == 0)
							length = getIntStrLen(infoRunAttrs[0].Value);
						else
							length = (byte)num7;

						intToStr(infoRunAttrs[0].Value, numRef, length, 1);
						return XstringHZK(numRef);
					}
				}

				sendReturnErr(0x1A);	//!!!
				return false;
			}
			#endregion

			#region load
			if (RunCode(buf, "load ", 1, range))
			{
				length = (byte)GetU16(m_cgCode.CodeResults[0], buf);
				return loadRef(length);
			}
			#endregion

			#region ref
			if (RunCode(buf, "ref ", 1, range))
			{
				if ((buf[m_cgCode.CodeResults[0].Begin] > 0x2f) && (buf[m_cgCode.CodeResults[0].Begin] < 0x3a))
					length = (byte)GetU16(m_cgCode.CodeResults[0], buf);
				else
					length = (byte)getPageName(m_cgCode.CodeResults[0], buf, 1, ref GuiApp.PageInfo);

				if (length >= GuiApp.PageInfo.ObjCount)
				{
					sendReturnErr(2);
					return false;
				}
				GuiApp.PageObjects[length].Visible = 1;
				return refreshObj(length);
			}
			#endregion

			#region get
			if (RunCode(buf, "get ", 1, range))
			{
				pos.Begin = m_cgCode.CodeResults[0].Begin;
				pos.End = m_cgCode.CodeResults[0].End;
				num4 = getStringAttribute(buf, pos, ref infoRunAttrs[0]);
				if (infoRunAttrs[0].AttrInfo.DataStart == dataFrom_Null)
				{
					sendReturnErr(0x1a);
					return false;
				}
				send_va(ref infoRunAttrs[0], true);
				return false;
			}
			#endregion

			#region vmax
			if (RunCode(buf, "vmax ", 3, range))
			{
				pos.Begin = m_cgCode.CodeResults[0].Begin;
				pos.End = m_cgCode.CodeResults[0].End;
				num3 = getStringAttribute(buf, pos, ref infoRunAttrs[0]);
				if (infoRunAttrs[0].AttrInfo.DataStart == dataFrom_Null)
				{
					sendReturnErr(0x1a);
					return false;
				}
				if (infoRunAttrs[0].Value >= GetU32(m_cgCode.CodeResults[1], buf))
				{
					pos.Begin = m_cgCode.CodeResults[2].Begin;
					pos.End = m_cgCode.CodeResults[2].End;
					num3 = getStringAttribute(buf, pos, ref infoRunAttrs[1]);
					if (infoRunAttrs[1].AttrInfo.DataStart == dataFrom_Null)
					{
						sendReturnErr(0x1a);
						return false;
					}
					if (setAttr(ref infoRunAttrs[1], ref infoRunAttrs[0], 0) == 0)
						return false;
				}
				return true;
			}
			#endregion

			#region vmin
			if (RunCode(buf, "vmin ", 3, range))
			{
				pos.Begin = m_cgCode.CodeResults[0].Begin;
				pos.End = m_cgCode.CodeResults[0].End;
				num3 = getStringAttribute(buf, pos, ref infoRunAttrs[0]);
				if (infoRunAttrs[0].AttrInfo.DataStart == dataFrom_Null)
				{
					sendReturnErr(0x1a);
					return false;
				}
				if (infoRunAttrs[0].Value <= GetU32(m_cgCode.CodeResults[1], buf))
				{
					pos.Begin = m_cgCode.CodeResults[2].Begin;
					pos.End = m_cgCode.CodeResults[2].End;
					num3 = getStringAttribute(buf, pos, ref infoRunAttrs[1]);
					if (infoRunAttrs[1].AttrInfo.DataStart == dataFrom_Null)
					{
						sendReturnErr(0x1a);
						return false;
					}
					if (setAttr(ref infoRunAttrs[0], ref infoRunAttrs[1], 0) == 0)
						return false;
				}
				return true;
			}
			#endregion

			#region ussp
			if (RunCode(buf, "ussp=", 1, range))
			{
				m_sysTimer.UsSp = GetU32(m_cgCode.CodeResults[0], buf) * 1000;
				if ((m_sysTimer.UsSp > 0) && (m_sysTimer.UsSp < 3000))
					m_sysTimer.UsSp = 3000;
				return true;
			}
			#endregion

			#region thsp
			if (RunCode(buf, "thsp=", 1, range))
			{
				m_sysTimer.ThSp = GetU32(m_cgCode.CodeResults[0], buf) * 1000;
				if (m_sysTimer.ThSp > 0 && m_sysTimer.ThSp < 3000)
					m_sysTimer.ThSp = 3000;
				return true;
			}
			#endregion

			#region thup
			if (RunCode(buf, "thup=", 1, range))
			{
				m_sysTimer.ThSleepUp = (byte)GetU32(m_cgCode.CodeResults[0], buf);
				return true;
			}
			#endregion

			#region spax
			if (RunCode(buf, "spax=", 1, range))
			{
				GuiApp.BrushInfo.SpacingX = (byte)GetU16(m_cgCode.CodeResults[0], buf);
				return true;
			}
			#endregion

			#region spay
			if (RunCode(buf, "spay=", 1, range))
			{
				GuiApp.BrushInfo.SpacingY = (byte)GetU16(m_cgCode.CodeResults[0], buf);
				return true;
			}
			#endregion

			#region fill
			if (RunCode(buf, "fill ", 5, range))
			{
				LCD_Fill(
					GetU16(m_cgCode.CodeResults[0], buf),
					GetU16(m_cgCode.CodeResults[1], buf),
					GetU16(m_cgCode.CodeResults[2], buf),
					GetU16(m_cgCode.CodeResults[3], buf),
					GetU16(m_cgCode.CodeResults[4], buf)
					);
				return true;
			}
			#endregion

			#region page
			if (RunCode(buf, "page ", 1, range))
			{
				if ((buf[m_cgCode.CodeResults[0].Begin] > 0x2f) && (buf[m_cgCode.CodeResults[0].Begin] < 0x3a))
					return RefreshPage(GetU16(m_cgCode.CodeResults[0], buf));
				index = getPageName(m_cgCode.CodeResults[0], buf, 0, ref GuiApp.PageInfo);
				if (index == 0xffff)
					return RefreshPage(GetU16(m_cgCode.CodeResults[0], buf));
				return RefreshPage(index);
			}
			#endregion

			#region dire
			if (RunCode(buf, "dire ", 1, range))
			{
				if (lcdSetup((byte)GetU16(m_cgCode.CodeResults[0], buf)))
				{
					clearTimer();
					setTouchState(0);
				}
				return true;
			}
			#endregion

			#region line
			if (RunCode(buf, "line ", 5, range))
			{
				lcdDrawLine(GetU16(
					m_cgCode.CodeResults[0], buf),
					GetU16(m_cgCode.CodeResults[1], buf),
					GetU16(m_cgCode.CodeResults[2], buf),
					GetU16(m_cgCode.CodeResults[3], buf),
					GetU16(m_cgCode.CodeResults[4], buf),
					1);
				return true;
			}
			#endregion

			#region draw
			if (RunCode(buf, "draw ", 5, range))
			{
				lcdDrawRectangle(GetU16(m_cgCode.CodeResults[0], buf), GetU16(m_cgCode.CodeResults[1], buf), GetU16(m_cgCode.CodeResults[2], buf), GetU16(m_cgCode.CodeResults[3], buf), GetU16(m_cgCode.CodeResults[4], buf));
				return true;
			}
			#endregion

			#region draw3d
			if (RunCode(buf, "draw3d ", 6, range))
			{
				lcdDrawRectangle3D(GetU16(m_cgCode.CodeResults[0], buf), GetU16(m_cgCode.CodeResults[1], buf), GetU16(m_cgCode.CodeResults[2], buf), GetU16(m_cgCode.CodeResults[3], buf), GetU16(m_cgCode.CodeResults[4], buf), GetU16(m_cgCode.CodeResults[5], buf));
				return true;
			}
			#endregion

			#region cir
			if (RunCode(buf, "cir ", 4, range))
			{
				drawCircle(GetU16(m_cgCode.CodeResults[0], buf), GetU16(m_cgCode.CodeResults[1], buf), GetU16(m_cgCode.CodeResults[2], buf), GetU16(m_cgCode.CodeResults[3], buf));
				return true;
			}
			#endregion

			#region cirs
			if (RunCode(buf, "cirs ", 4, range))
			{
				drawCircles(GetU16(m_cgCode.CodeResults[0], buf), GetU16(m_cgCode.CodeResults[1], buf), GetU16(m_cgCode.CodeResults[2], buf), GetU16(m_cgCode.CodeResults[3], buf));
				return true;
			}
			#endregion

			#region draw_h
			if (RunCode(buf, "draw_h ", 6, range))
			{
				drawH(
					GetU16(m_cgCode.CodeResults[0], buf),
					GetU16(m_cgCode.CodeResults[1], buf),
					GetU16(m_cgCode.CodeResults[2], buf),
					GetU16(m_cgCode.CodeResults[3], buf),
					(byte)GetU16(m_cgCode.CodeResults[4], buf),
					GetU16(m_cgCode.CodeResults[5], buf)
					);
				return true;
			}
			#endregion

			#region sysda
			if (Utility.IndexOf(buf, "sysda", range) != 0xffff
			 && (range.End - range.Begin + 1) > 7
				)
			{
				length = buf[range.Begin + 5];
				if (length < 4 && buf[range.Begin + 6] == 0x3d)
				{
					pos.Begin = range.Begin + 7;
					pos.End = range.End;
					GuiApp.System[length] = GetU32(pos, buf);
					return true;
				}
			}
			#endregion

			if (!IsEditor)
			{

				#region delay
				if (RunCode(buf, "delay=", 1, range))
				{
					if ((GuiApp.Delay == 0) && (guiObjectRtRef() < 0xff))
						GuiApp.Delay = GetU16(m_cgCode.CodeResults[0], buf);
					else
						delay_ms(GetU16(m_cgCode.CodeResults[0], buf));
					return true;
				}
				#endregion

				#region sendxy
				if (RunCode(buf, "sendxy=", 1, range))
				{
					GuiApp.Touch.SendXY = (byte)GetU16(m_cgCode.CodeResults[0], buf);
					return true;
				}
				#endregion

				#region topen
				if (RunCode(buf, "topen ", 2, range) && GuiApp.HexStrIndex != 0xffff)
				{
					index = GetU16(m_cgCode.CodeResults[0], buf);
					if (index >= m_timers.Length)
					{
						sendReturnErr(6);
						return false;
					}
					m_timers[index].State = 0;
					m_timers[index].MaxValue = GetU16(m_cgCode.CodeResults[1], buf);
					m_timers[index].CodeBegin = GuiApp.HexStrIndex;
					for (; ; )
					{
						InfoString stringInfo = readInfoString(GuiApp.HexStrIndex);
						++GuiApp.HexStrIndex;
						if (stringInfo.Size > 0)
						{
							SPI_Flash_Read(ref m_hexStrBuf, GuiApp.App.StringDataStart + stringInfo.Start, stringInfo.Size);
							range.Begin = 0;
							range.End = (ushort)(stringInfo.Size - 1);
							if (Utility.IndexOf(m_hexStrBuf, "tend", range) != 0xffff)
								break;
						}
					}
					m_timers[index].Value = 0;
					m_timers[index].State = 0;
					return true;
				}
				#endregion

				#region tpau
				if (RunCode(buf, "tpau ", 3, range))
				{
					index = GetU16(m_cgCode.CodeResults[0], buf);
					if (index >= m_timers.Length)
					{
						sendReturnErr(6);
						return false;
					}
					if (m_timers[index].MaxValue == 0xffff)
					{
						sendReturnErr(7);
						return false;
					}
					m_timers[index].MaxValue = GetU16(m_cgCode.CodeResults[1], buf);
					m_timers[index].State = (byte)GetU16(m_cgCode.CodeResults[2], buf);
					return true;
				}
				#endregion

				#region thc
				if (RunCode(buf, "thc=", 1, range))
				{
					m_lcdDevInfo.DrawColor = GetU16(m_cgCode.CodeResults[0], buf);
					return true;
				}
				#endregion

				#region thdra
				if (RunCode(buf, "thdra=", 1, range))
				{
					m_lcdDevInfo.Draw = (byte)GetU16(m_cgCode.CodeResults[0], buf);
					return true;
				}
				#endregion

				#region sendme
				if (Utility.IndexOf(buf, "sendme", range) != 0xffff)
				{
					sendByte(0x66);
					sendByte((byte)GuiApp.Page);
					sendEnd();
					return false;
				}
				#endregion

				#region com_stop
				if (Utility.IndexOf(buf, "com_stop", range) != 0xffff)
				{
					m_ComQueue.CodePause = m_ComQueue.DiulieYunxing;
					m_ComQueue.CodePause++;
					if (m_ComQueue.CodePause == m_ComQueueLength)
						m_ComQueue.CodePause = 0;
					return true;
				}
				#endregion

				#region com_star
				if (Utility.IndexOf(m_comBuffer, "com_star", range) != 0xffff)
					return true;
				#endregion

				#region touch_j
				if (Utility.IndexOf(buf, "touch_j", range) != 0xffff)
					return true;
				#endregion

				#region code_c
				if (Utility.IndexOf(buf, "code_c", range) != 0xffff)
				{
					GuiApp.Usart.State = 6;
					return true;
				}
				#endregion

				#region tsw
				if (RunCode(buf, "tsw ", 2, range))
				{
					if (buf[m_cgCode.CodeResults[0].Begin] >= '0' && buf[m_cgCode.CodeResults[0].Begin] <= '9')
					{
						length = (byte)GetU16(m_cgCode.CodeResults[0], buf);
						if (length == 0xff)
						{
							setTouchState((byte)GetU16(m_cgCode.CodeResults[1], buf));
							return true;
						}
					}
					else
						length = (byte)getPageName(m_cgCode.CodeResults[0], buf, 1, ref GuiApp.PageInfo);

					if (length >= GuiApp.PageInfo.ObjCount)
					{
						sendReturnErr(2);
						return false;
					}
					GuiApp.PageObjects[length].TouchState = (byte)GetU16(m_cgCode.CodeResults[1], buf);
					return true;
				}
				#endregion

				#region print
				if (Utility.IndexOf(buf, "print ", range) != 0xffff)
				{
					pos.Begin = range.Begin + 6;
					pos.End = range.End;
					num4 = getStringAttribute(buf, pos, ref infoRunAttrs[0]);
					if (infoRunAttrs[0].AttrInfo.DataStart == dataFrom_Null)
					{
						sendReturnErr(0x1a);
						return false;
					}
					send_va(ref infoRunAttrs[0], false);
					return false;
				}
				#endregion

				#region printh
				if (Utility.IndexOf(buf, "printh ", range) != 0xffff)
				{
					for (index = 7; index <= range.End; index = (ushort)(index + 3))
						sendByte(getIntFromStr16(buf, index));
					return false;
				}
				#endregion

				#region
				if (GuiApp.HexStrIndex != 0xffff)
				{
					if ((buf[range.Begin] == '}' && range.Begin == range.End)
					 || (buf[range.Begin] == '{' && range.Begin == range.End)
						)
						return false;

					if (Utility.IndexOf(buf, "if(", range) != 0xffff && buf[range.End] == ')')
					{
						pos.Begin = (ushort)(range.Begin + 3);
						pos.End = (ushort)(range.End - 1);
						num4 = getStringAttribute(buf, pos, ref infoRunAttrs[0]);
						if (infoRunAttrs[0].AttrInfo.DataStart == dataFrom_Null)
						{
							sendReturnErr(0x1A);
							return false;
						}
						if (num4 < 51)
						{
							++num4;
							if (buf[num4] == '>' || buf[num4] == '<' || buf[num4] == '=' || buf[num4] == '!')
							{
								length = buf[num4];
								++num4;
								if (buf[num4] == '=')
								{
									length += 100;
									++num4;
								}
								pos.Begin = num4;
								num4 = getStringAttribute(buf, pos, ref infoRunAttrs[1]);
								if (infoRunAttrs[1].AttrInfo.DataStart == dataFrom_Null)
								{
									sendReturnErr(0x1A);
									return false;
								}

								if (makeAttr(buf, ref infoRunAttrs[0], ref infoRunAttrs[1], length))
								{
									++GuiApp.HexStrIndex;
									return false;
								}

								++GuiApp.HexStrIndex;
								if (GuiApp.HexStrIndex >= GuiApp.App.StringCount)
									return false;

								for (; ; )
								{
									InfoString stringInfo = readInfoString(GuiApp.HexStrIndex);
									++GuiApp.HexStrIndex;
									SPI_Flash_Read(ref m_hexStrBuf, GuiApp.App.StringDataStart + stringInfo.Start, stringInfo.Size);
									if (stringInfo.Size == 1)
									{
										if (m_hexStrBuf[0] == '}')
										{
											if (length == 0)
												return false;
											--length;
										}
										else if (m_hexStrBuf[0] == '}')
											++length;
									}
									else if (stringInfo.Size == 3
											&& m_hexStrBuf[0] == 'e'
											&& m_hexStrBuf[1] == 'n'
											&& m_hexStrBuf[2] == 'd')
									{
										GuiApp.HexStrIndex = 0xffff;
										return false;
									}
								}
							}
							sendReturnErr(0x1A);
							return false;
						}
					}
				}
				#endregion

				#region cov
				if (RunCode(buf, "cov ", 3, range))
				{
					if (m_cgCode.CodeResults[0].End - m_cgCode.CodeResults[0].Begin > 0x1b
					 || m_cgCode.CodeResults[1].End - m_cgCode.CodeResults[1].Begin > 0x1b)
					{
						sendReturnErr(0x1A);
						return false;
					}

					pos = m_cgCode.CodeResults[0];
					num3 = getStringAttribute(buf, pos, ref infoRunAttrs[0]);
					if ((infoRunAttrs[0].AttrInfo.DataStart == dataFrom_Null) || (num3 != pos.End))
					{
						sendReturnErr(0x1a);
						return false;
					}
					pos = m_cgCode.CodeResults[1];
					num3 = getStringAttribute(buf, pos, ref infoRunAttrs[1]);
					if ((infoRunAttrs[1].AttrInfo.DataStart == dataFrom_Null) || (num3 != pos.End))
					{
						sendReturnErr(0x1a);
						return false;
					}

					num = attributeConvert(ref infoRunAttrs[0], ref infoRunAttrs[1], (byte)GetU16(m_cgCode.CodeResults[2], buf));
					if (num == 1 && infoRunAttrs[1].AttrInfo.IsReturn < GuiApp.PageInfo.ObjCount)
						GuiApp.PageObjects[infoRunAttrs[1].AttrInfo.IsReturn].RefreshFlag = 1;
					return (num == 1);
				}
				#endregion

				#region oref
				if (RunCode(buf, "oref ", 2, range))
				{
					length = (byte)GetU32(m_cgCode.CodeResults[0], buf);
					num7 = (byte)GetU32(m_cgCode.CodeResults[1], buf);
					if (length < GuiApp.PageInfo.ObjCount && num7 < 4)
					{
						GuiApp.System[num7] = GuiApp.PageObjects[length].RefreshFlag;
						return false;
					}
				}
				#endregion

				#region cle_f
				if (RunCode(buf, "cle_f ", 2, range))
				{
					length = (byte)GetU32(m_cgCode.CodeResults[0], buf);
					if (length < GuiApp.PageInfo.ObjCount)
					{
						if ((buf[m_cgCode.CodeResults[1].Begin] - 0x30) > 0)
							GuiApp.PageObjects[length].RefreshFlag = 1;
						else
							GuiApp.PageObjects[length].RefreshFlag = 0;
					}
					else if (length == 0xff)
						setRefreshFlag((byte)(buf[m_cgCode.CodeResults[1].Begin] - '0'));
					return false;
				}
				#endregion

				if ((range.End - range.Begin) > 2)
				{

					pos.Begin = range.Begin;
					pos.End = range.End;
					ushort num5 = Utility.IndexOfAny(buf, "=", pos);
					if (num5 != 0xffff)
					{
						num4 = (ushort)(num5 - range.Begin);
						if (num4 > 0 && num4 < 51)
						{
							pos.Begin = num5 + 1;
							pos.End = range.End;
							num4 = 0;
							for (int idx = range.Begin; idx < num5; idx++)
							{
								buffer[num4] = buf[idx];
								num4++;
							}
							buffer[num4] = 0;

							getAttribute(buffer, ref infoRunAttrs[2]);

							if (infoRunAttrs[2].AttrInfo.DataStart == dataFrom_Null)
							{
								sendReturnErr(0x1a);
								return false;
							}

							num3 = getStringAttribute(buf, pos, ref infoRunAttrs[0]);
							if (infoRunAttrs[0].AttrInfo.DataStart == dataFrom_Null)
							{
								sendReturnErr(0x1a);
								return false;
							}
							if (num3 >= pos.End)
							{
								if (setAttr(ref infoRunAttrs[0], ref infoRunAttrs[2], 0) == 0)
									return false;
							}
							else
							{
								++num3;
								if (buf[num3] == '+' || buf[num3] == '-' || buf[num3] == '*' || buf[num3] == '/')
								{
									length = buf[num3];
									pos.Begin = (ushort)(num3 + 1);
									num3 = getStringAttribute(buf, pos, ref infoRunAttrs[1]);
									if (infoRunAttrs[1].AttrInfo.DataStart == dataFrom_Null)
									{
										sendReturnErr(0x1A);
										return false;
									}
									if (AttributeAdd(buf, ref infoRunAttrs[0], ref infoRunAttrs[1], ref infoRunAttrs[2], length) == 0)
										return false;

									while (num3 < pos.End)
									{
										++num3;
										if (buf[num3] == '+' || buf[num3] == '-' || buf[num3] == '*' || buf[num3] == '/')
										{
											length = buf[num3];
											pos.Begin = (ushort)(num3 + 1);
											num3 = getStringAttribute(buf, pos, ref infoRunAttrs[1]);
											if (infoRunAttrs[1].AttrInfo.DataStart == dataFrom_Null)
											{
												sendReturnErr(0x1A);
												return false;
											}
											if (AttributeAdd(buf, ref infoRunAttrs[2], ref infoRunAttrs[1], ref infoRunAttrs[2], length) == 0)
												return false;
										}
										else
										{
											sendReturnErr(0x1A);
											return false;
										}
									}
								}
								else
								{
									sendReturnErr(0x1A);
									return false;
								}
							}
							if (infoRunAttrs[2].AttrInfo.IsReturn < GuiApp.PageInfo.ObjCount)
								GuiApp.PageObjects[infoRunAttrs[2].AttrInfo.IsReturn].RefreshFlag = 1;
							return true;
						}
					}
				}
			}
			sendReturnErr(0);
			return false;
		}
		#endregion

		#region sendByte(byte data)
		private void sendByte(byte data)
		{
			SendByte((int)data, null);
		}

		private void sendEnd()
		{
			sendByte(0xff);
			sendByte(0xff);
			sendByte(0xff);
		}
		#endregion

		#region delay_ms(int ms)
		private void delay_ms(int ms)
		{
			while (ms > 0)
			{
				--ms;
				if (m_runState == 0)
					break;
				Application.DoEvents();
				Thread.Sleep(1);
			}
		}
		#endregion

		#region drawCircle(ushort x0, ushort y0, ushort r, ushort color)
		private void drawCircle(ushort x0, ushort y0, ushort r, ushort color)
		{
			int num = 0;
			int num2 = r;
			int num3 = 3 - (r << 1);
			while (num <= num2)
			{
				lcdDrawPoint((ushort)(x0 + num), (ushort)(y0 - num2), color);
				lcdDrawPoint((ushort)(x0 + num2), (ushort)(y0 - num), color);
				lcdDrawPoint((ushort)(x0 + num2), (ushort)(y0 + num), color);
				lcdDrawPoint((ushort)(x0 + num), (ushort)(y0 + num2), color);
				lcdDrawPoint((ushort)(x0 - num), (ushort)(y0 + num2), color);
				lcdDrawPoint((ushort)(x0 - num2), (ushort)(y0 + num), color);
				lcdDrawPoint((ushort)(x0 - num), (ushort)(y0 - num2), color);
				lcdDrawPoint((ushort)(x0 - num2), (ushort)(y0 - num), color);
				num++;
				if (num3 < 0)
					num3 += (4 * num) + 6;
				else
				{
					num3 += 10 + (4 * (num - num2));
					num2--;
				}
			}
		}
		#endregion

		#region
		private void drawCircles(ushort x0, ushort y0, ushort r, ushort color)
		{
			int num2 = 0;
			int num3 = r;
			int num4 = 3 - (r << 1);
			while (num2 <= num3)
			{
				uint qyt = (uint)((num2 * 2) + 1);
				LCD_AreaSet((ushort)(x0 - num3), (ushort)(y0 - num2), (ushort)(x0 - num3), (ushort)(y0 + num2));
				LCD_WR_POINT(qyt, color);
				LCD_AreaSet((ushort)(x0 + num3), (ushort)(y0 - num2), (ushort)(x0 + num3), (ushort)(y0 + num2));
				LCD_WR_POINT(qyt, color);
				qyt = (uint)((num3 * 2) + 1);
				LCD_AreaSet((ushort)(x0 - num2), (ushort)(y0 - num3), (ushort)(x0 - num2), (ushort)(y0 + num3));
				LCD_WR_POINT(qyt, color);
				LCD_AreaSet((ushort)(x0 + num2), (ushort)(y0 - num3), (ushort)(x0 + num2), (ushort)(y0 + num3));
				LCD_WR_POINT(qyt, color);
				num2++;
				if (num4 < 0)
					num4 += (4 * num2) + 6;
				else
				{
					num4 += 10 + (4 * (num2 - num3));
					num3--;
				}
			}
		}
		#endregion
		#region
		private void drawH(ushort x0, ushort y0, ushort r, ushort degrees, byte cu, ushort color)
		{
			double num = r;
			double d = (3.141592653 * degrees) / 180.0;
			double num3 = num * Math.Cos(d);
			double num4 = num * Math.Sin(d);
			lcdDrawLine((ushort)(x0 - num3), (ushort)(y0 - num4), x0, y0, color, cu);
		}
		#endregion
		#region
		private unsafe ushort findSegmentation(byte[] buf, Range bufPos)
		{
			int star = bufPos.Begin;
			byte num2 = 0;
			while (star <= bufPos.End)
			{
				if (buf[star] == ',' && num2 == 0)
					return (ushort)star;

				if (buf[star] == '"')
				{
					if (star == bufPos.Begin)
						num2 = 1;
					else if (buf[star - 1] != 0x5c)
					{
						if (num2 == 0)
							num2 = 1;
						else
							num2 = 0;
					}
				}
				++star;
			}
			return 0xffff;
		}
		#endregion
		#region
		private uint findFontStart(byte h, byte l)
		{
			byte[] buffer = new byte[2];
			ushort num2 = (ushort)((h * 0x100) + l);
			uint num3 = 0;
			uint num4 = 0;
			uint num5 = (ushort)(m_fontInfo.Length - 2);
			uint add = 0;
			ushort num7 = (ushort)((m_fontInfo.Height / 8) * m_fontInfo.Width);
			num7 = (ushort)(num7 + 2);
			uint num = ((m_fontInfo.DataOffset + GuiApp.App.FontImageStart) + m_fontInfo.NameEnd) + 1;
			while (num5 >= num4)
			{
				num3 = (num5 + num4) / 2;
				add = num + (num3 * num7);
				SPI_Flash_Read(ref buffer, add, 2);
				if (buffer[0] == h && buffer[1] == l)
					return (add + 2);

				if (num5 == num4)
					break;

				if ((buffer[0] * 0x100 + buffer[1]) > num2)
					num5 = num3 - 1;
				else
					num4 = num3 + 1;
			}
			return (num + num7 * (m_fontInfo.Length - 1) + 2);
		}
		#endregion

		#region getAttribute
		private unsafe void getAttribute(byte[] name, ref AttributeRun attr)
		{
			uint index = 0;
			uint num2 = 0;
			byte num4 = 0;

			InfoPage pageInfo = new InfoPage();

			ushort stringInfoStart = (ushort)(GuiApp.PageInfo.InstStart + 4);
			byte[] val = new byte[8];

			Range laction = new Range(0, 14);

			attr.AttrInfo.DataStart = dataFrom_Null;
			attr.AttrInfo.IsReturn = 0xff;
			attr.Value = 1234567890;

			if (compareString(name, "sysda", 5))
			{
				if (name[5] != 0 && name[6] == 0)
				{
					num2 = (uint)(name[5] - '0');
					if (num2 < m_systemLength)
					{
						attr.Value = GuiApp.System[num2];
						attr.AttrInfo.DataStart = (byte)(190 + num2);	// 0xBE
						attr.AttrInfo.MaxValue = uint.MaxValue;
						attr.AttrInfo.MinValue = 0;
					}
				}
			}
			else if (compareString(name, "bkcmd", 0))
			{
				attr.Value = GuiApp.SendReturn;
				attr.AttrInfo.DataStart = dataFrom_Sys_Bkcmd;
				attr.AttrInfo.MaxValue = 3;
				attr.AttrInfo.MinValue = 0;
			}
			else if (compareString(name, "dim", 0))
			{
				attr.Value = 50;
				attr.AttrInfo.DataStart = dataFrom_Sys_Bl;
				attr.AttrInfo.MaxValue = 100;
				attr.AttrInfo.MinValue = 0;
			}
			else if (compareString(name, "dims", 0))
			{
				attr.Value = 50;
				attr.AttrInfo.DataStart = dataFrom_Sys_IntBl;
				attr.AttrInfo.MaxValue = 100;
				attr.AttrInfo.MinValue = 0;
			}
			else if (compareString(name, "baud", 0))
			{
				attr.Value = 0x2580;
				attr.AttrInfo.DataStart = dataFrom_Sys_Baud;
				attr.AttrInfo.MaxValue = uint.MaxValue;
				attr.AttrInfo.MinValue = 0;
			}
			else if (compareString(name, "bauds", 0))
			{
				attr.Value = 0x2580;
				attr.AttrInfo.DataStart = dataFrom_Sys_Bauds;
				attr.AttrInfo.MaxValue = uint.MaxValue;
				attr.AttrInfo.MinValue = 0;
			}
			else if (compareString(name, "spax", 0))
			{
				attr.Value = GuiApp.BrushInfo.SpacingX;
				attr.AttrInfo.DataStart = dataFrom_Sys_SpaX;
				attr.AttrInfo.MaxValue = 0xff;
				attr.AttrInfo.MinValue = 0;
			}
			else if (compareString(name, "spay", 0))
			{
				attr.Value = GuiApp.BrushInfo.SpacingY;
				attr.AttrInfo.DataStart = dataFrom_Sys_SpaY;
				attr.AttrInfo.MaxValue = 0xff;
				attr.AttrInfo.MinValue = 0;
			}
			else if (compareString(name, "ussp", 0))
			{
				attr.Value = m_sysTimer.UsSp;
				attr.AttrInfo.DataStart = dataFrom_Sys_Ussp;
				attr.AttrInfo.MaxValue = 65535000u;
				attr.AttrInfo.MinValue = 0;
			}
			else if (compareString(name, "thsp", 0))
			{
				attr.Value = m_sysTimer.ThSp;
				attr.AttrInfo.DataStart = dataFrom_Sys_ThSp;
				attr.AttrInfo.MaxValue = 65535000u;
				attr.AttrInfo.MinValue = 0;
			}
			else if (compareString(name, "thup", 0))
			{
				attr.Value = m_sysTimer.ThSleepUp;
				attr.AttrInfo.DataStart = dataFrom_Sys_ThUp;
				attr.AttrInfo.MaxValue = 1;
				attr.AttrInfo.MinValue = 0;
			}
			else if (compareString(name, "RED", 0))
				attr.Value = 0xF800;
			else if (compareString(name, "BLUE", 0))
				attr.Value = 0x001F;
			else if (compareString(name, "GRAY", 0))
				attr.Value = 0x8430;
			else if (compareString(name, "BLACK", 0))
				attr.Value = 0x0000;
			else if (compareString(name, "WHITE", 0))
				attr.Value = 0xFFFF;
			else if (compareString(name, "GREEN", 0))
				attr.Value = 0x07E0;
			else if (compareString(name, "BROWN", 0))
				attr.Value = 0xBC40;
			else if (compareString(name, "YELLOW", 0))
				attr.Value = 0xFFE0;

			if (attr.Value != 1234567890)
			{
				if (attr.AttrInfo.DataStart == dataFrom_Null)
					attr.AttrInfo.DataStart = dataFrom_Sys_X;

				attr.AttrInfo.AttrType = HmiAttributeType.Other;
				attr.AttrInfo.Length = 4;
				attr.AttrInfo.DataLength = 4;
			}
			else
			{
				for (; name[num2] != 0; ++num2)
				{
					if (name[num2] == '.')
						num4++;
				}

				laction.Begin = 0;
				laction.End = laction.Begin;

				if (name[laction.Begin] == 0 || name[laction.Begin] == '.')
					return;

				ushort objIndex;
				while (name[laction.End] != '.')
				{
					++laction.End;
					if (laction.End == '(' || name[laction.End] == 0)
						return;
				}
				--laction.End;

				if (num4 == 2)
				{
					objIndex = getPageName(laction, name, 0, ref GuiApp.PageInfo);
					if (objIndex == 0xffff)
						return;

					if (objIndex == GuiApp.Page)
					{
						num4 = 1;
						pageInfo = GuiApp.PageInfo;
					}
					else
						pageInfo = readInfoPage(objIndex);

					laction.Begin = laction.End + 2;
					laction.End = laction.Begin;
					if (name[laction.Begin] == 0 || name[laction.Begin] == '.')
						return;

					while (name[laction.End] != '.')
					{
						++laction.End;
						if (laction.End == '(' || name[laction.End] == 0)
							return;
					}
					--laction.End;
				}
				else
				{
					if (num4 != 1)
						return;
					pageInfo = GuiApp.PageInfo;
				}

				objIndex = getPageName(laction, name, 1, ref pageInfo);
				if (objIndex == 0xffff)
					return;

				objIndex = (ushort)(objIndex + pageInfo.ObjStart);
				laction.Begin = laction.End + 2;
				laction.End = laction.Begin;
				if (laction.End >= 40 && name[laction.End] == 0)
					return;

				while (name[laction.End] != 0)
				{
					++laction.End;
					if (laction.End == 40)
					{
						++laction.End;
						break;
					}
				}
				--laction.End;

				InfoObject infoObject = ReadObject(objIndex);

				if (num4 == 2
				 && infoObject.IsCustomData != 1
				 || infoObject.StringInfoEnd - infoObject.StringInfoStart < 3
					)
					return;

				Range pos2 = new Range(0, 2);
				for (
					stringInfoStart = infoObject.StringInfoStart;
					stringInfoStart <= infoObject.StringInfoEnd;
					++stringInfoStart
					)
				{
					InfoString strInfo = readInfoString(stringInfoStart);
					if (strInfo.Size >= (HmiOptions.InfoAttributeSize + 8))
					{
						SPI_Flash_Read(ref val, GuiApp.App.StringDataStart + strInfo.Start, 8);
						index = Utility.IndexOf(name, val, laction, false);
						if (index == laction.End)
						{
							attr.AttrInfo = Utility.ToStruct<InfoAttribute>(
								SPI_Flash_Read(
									GuiApp.App.StringDataStart + strInfo.Start + 8,
									HmiOptions.InfoAttributeSize
									)
								);
							attr.Value = 0;
							if (num4 == 2)
								attr.AttrInfo.IsReturn = 0xff;

							if (attr.AttrInfo.CanModify == 1)
							{
								attr.AttrInfo.DataStart = dataFrom_RAM;
								fixed (byte* px = &GuiApp.CustomData[attr.AttrInfo.Start])
									attr.Pz = px;
							}
							else
								attr.AttrInfo.DataStart = dataFrom_Buf;

							if (attr.AttrInfo.AttrType >= HmiAttributeType.String || attr.AttrInfo.Length >= 5)
								break;

							if (attr.AttrInfo.CanModify == 1)
								attr.Value = Utility.ToUInt32(GuiApp.CustomData, attr.AttrInfo.Start, attr.AttrInfo.Length);
							else
								attr.Value = Utility.ToUInt32(
									SPI_Flash_Read(
										GuiApp.App.StringDataStart + strInfo.Start + 8 + (uint)HmiOptions.InfoAttributeSize,
										attr.AttrInfo.Length
									), 0, attr.AttrInfo.Length);
							break;
						}
					}
					else
					{
						SPI_Flash_Read(ref val, GuiApp.App.StringDataStart + strInfo.Start, 3);
						if (Utility.IndexOf(val, "end", pos2) != 0xffff)
							break;
					}
				}
			}
		}
		#endregion

		#region getHexStr
		private ushort getHexStr()
		{
			for (int i = 3; i > -1; i--)
			{
				if (m_hexStrPos[i] != 0xffff)
				{
					ushort num2 = m_hexStrPos[i];
					m_hexStrPos[i] = 0xffff;
					return num2;
				}
			}
			return 0xffff;
		}
		#endregion

		#region
		private unsafe byte getIntFromStr16(byte* str)
		{
			byte num = 0;
			if ((str[0] >= 0x30) && (str[0] <= 0x39))
			{
				num = (byte)(str[0] - 0x30);
			}
			else if ((str[0] >= 0x61) && (str[0] <= 0x66))
			{
				num = (byte)((str[0] - 0x61) + 10);
			}
			else if ((str[0] >= 0x41) && (str[0] <= 70))
			{
				num = (byte)((str[0] - 0x41) + 10);
			}
			else
			{
				return num;
			}
			num = (byte)(num << 4);
			if ((str[1] >= 0x30) && (str[1] <= 0x39))
			{
				num = (byte)(num + ((byte)(str[1] - 0x30)));
			}
			else
			{
				if ((str[1] >= 0x61) && (str[1] <= 0x66))
				{
					return (byte)(num + ((byte)((str[1] - 0x61) + 10)));
				}
				if ((str[1] >= 0x41) && (str[1] <= 70))
				{
					num = (byte)(num + ((byte)((str[1] - 0x41) + 10)));
				}
				else
				{
					return 0;
				}
			}
			return num;
		}

		private byte getIntFromStr16(byte[] str, int start)
		{
			byte num = 0;
			byte ch = str[start];
			if (ch >= '0' && ch <= '9')
				num = (byte)(ch - '0');
			else if (ch >= 0x61 && ch <= 0x66)
				num = (byte)(ch - 0x61 + 10);
			else if (ch >= 0x41 && ch <= 70)
				num = (byte)(ch - 0x41 + 10);
			else
				return num;

			num = (byte)(num << 4);
			ch = str[start];
			if (ch >= '0' && ch <= '9')
				num += (byte)(ch - '0');
			else if (ch >= 0x61 && ch <= 0x66)
				num += (byte)(ch - 0x61 + 10);
			else if (ch >= 0x41 && ch <= 70)
				num += (byte)(ch - 0x41 + 10);
			return num;
		}
		#endregion

		#region
		private byte getIntStrLen(uint num)
		{
			if (num >= 0x3b9aca00)
			{
				return 10;
			}
			if (num >= 0x5f5e100)
			{
				return 9;
			}
			if (num >= 0x989680)
			{
				return 8;
			}
			if (num >= 0xf4240)
			{
				return 7;
			}
			if (num >= 0x186a0)
			{
				return 6;
			}
			if (num >= 0x2710)
			{
				return 5;
			}
			if (num >= 1000)
			{
				return 4;
			}
			if (num >= 100)
			{
				return 3;
			}
			if (num >= 10)
			{
				return 2;
			}
			return 1;
		}
		#endregion

		#region getPageName
		private ushort getPageName(Range range, byte[] bt1, byte state, ref InfoPage page)
		{
			byte[] buffer = new byte[14];
			uint add = 0;
			ushort pageCount = 0;
			uint infoPageSize = 0;
			int nameLen = (int)(range.End - range.Begin + 2);

			if (nameLen > 14)
				nameLen = 14;

			if (state == 0)
			{
				add = GuiApp.App.PageStart;
				pageCount = GuiApp.App.PageCount;
				infoPageSize = (uint)HmiOptions.InfoPageSize;
			}
			else if (state == 1)
			{
				if (page.ObjStart == 0xffff)
					return 0xffff;

				infoPageSize = (uint)HmiOptions.InfoObjectSize;
				add = GuiApp.App.ObjectStart + (uint)page.ObjStart * infoPageSize;
				pageCount = (ushort)(page.ObjEnd - page.ObjStart + 1);
			}

			for (ushort i = 0; i < pageCount; i++)
			{
				SPI_Flash_Read(ref buffer, add, nameLen);
				if (Utility.IndexOf(bt1, buffer, range, false) == range.End)
					return i;

				add += infoPageSize;
			}
			return 0xffff;
		}
		#endregion

		#region getStrAtt(byte* buf, InfoRange thisPos, ref InfoRunAttribute att)
		private unsafe ushort getStringAttribute(byte[] bytes, Range range, ref AttributeRun attr)
		{
			byte[] numArray = new byte[30];
			int begin = range.Begin;
			int end = 0;
			ushort index = 0;
			attr.AttrInfo.DataStart = dataFrom_Null;
			if (range.End < range.Begin)
				return 0xFFFF;

			if (bytes[begin] != '"')
			{
				if (bytes[begin] < '0' || bytes[begin] > '9')
				{
					if (bytes[begin] != '+'
					 && bytes[begin] != '-'
					 && bytes[begin] != '*'
					 && bytes[begin] != '/'
					 && bytes[begin] != '>'
					 && bytes[begin] != '<'
					 && bytes[begin] != '='
					 && bytes[begin] != '!'
						)
					{
						while (begin <= range.End)
						{
							index = 0;
							begin = range.Begin;
							while (begin <= range.End)
							{
								if (bytes[begin] == '+'
								 || bytes[begin] == '-'
								 || bytes[begin] == '*'
								 || bytes[begin] == '/'
								 || bytes[begin] == '>'
								 || bytes[begin] == '='
								 || bytes[begin] == '!')
								{
									--begin;
									break;
								}
								if (index == 29)
									return 0xffff;

								numArray[index] = bytes[begin];
								index++;
								begin++;
							}
							numArray[index] = 0;
							getAttribute(numArray, ref attr);

							if (attr.AttrInfo.DataStart == dataFrom_Null)
								return 0xffff;

							if (begin > range.End)
								begin = range.End;

							return (ushort)begin;
						}
					}
					return 0xffff;
				}

				while (begin <= range.End)
				{
					if (bytes[begin] < '0' || bytes[begin] > '9')
					{
						begin--;
						break;
					}
					if (begin == range.End)
						break;

					begin++;
				}
			}
			else
			{	// Start from "
				bytes[begin++] = 0;
				index = 0xffff;
				end = begin;
				while (begin <= range.End)
				{
					if (bytes[begin] != '\\')
					{
						bytes[end] = bytes[begin];
						if (bytes[begin] == '"')
						{
							bytes[end] = 0;
							index = (ushort)begin;
							break;
						}
						begin++;
						end++;
						continue;
					}

					if (begin != range.End)
					{
						begin++;
						if (bytes[begin] == '\\' || bytes[begin] == '"')
						{
							bytes[end] = bytes[begin];
							begin++;
							end++;
							continue;
						}
						if (bytes[begin] == 'r')
						{
							bytes[end] = (byte)'\r';
							end++;
							bytes[end] = (byte)'\n';
							begin++;
							end++;
							continue;
						}
					}
					return index;
				}
				if (index != 0xffff)
				{
					attr.AttrInfo.Start = (ushort)(range.Begin + 1);
					attr.AttrInfo.AttrType = HmiAttributeType.String;
					attr.AttrInfo.DataLength =
					attr.AttrInfo.Length = (ushort)(end - range.Begin);
					attr.AttrInfo.DataStart = dataFrom_Buf;
					fixed (byte* pb = &bytes[attr.AttrInfo.Start])
						attr.Pz = pb;
				}
				return index;
			}

			attr.AttrInfo.DataLength = 4;
			attr.AttrInfo.Length = 4;
			attr.Value = StrToInt(bytes, range.Begin, (byte)(begin - range.Begin + 1));
			attr.AttrInfo.AttrType = HmiAttributeType.Other;
			attr.AttrInfo.DataStart = dataFrom_We;
			return (ushort)begin;
		}
		#endregion

		#region getStringLength
		/// <summary>
		/// Get length of string (to \0)
		/// </summary>
		/// <param name="src"></param>
		/// <returns></returns>
		private unsafe ushort getStringLength(byte* src)
		{
			ushort length = 0;
			while (*src != 0)
			{
				src++;
				length++;
			}
			return length;
		}
		#endregion

		#region GuiInit
		public unsafe void GuiInit(string binPath, HmiApplication hmiApp, bool isEditor)
		{
			m_app = hmiApp;
			GuiApp.Usart.State = 0;
			IsEditor = isEditor;
			m_binPath = binPath;

			m_ComQueue.Queue = Range.List(m_ComQueueLength);
			m_cgCode.CodeResults = Range.List(11);
			GuiApp.System = new uint[4];
			GuiApp.CustomData = new byte[HmiOptions.MaxCustomDataSize];

			GuiApp.Page = 0;
			GuiApp.HexStrIndex = 0xffff;
			GuiApp.Delay = 0;

			clearHexStr();

			GuiApp.BrushInfo.sta = 0;
			GuiApp.BrushInfo.PointColor = 0;
			GuiApp.BrushInfo.BackColor = 0;
			GuiApp.BrushInfo.SpacingX = 0;
			GuiApp.BrushInfo.SpacingY = 0;
			GuiApp.BrushInfo.XCenter = 0;
			GuiApp.BrushInfo.YCenter = 0;

			clearComCode();

			m_reader = new StreamReader(m_binPath);
			readAppInfo();

			lcdSetup(GuiApp.App.IsPotrait);

			clearTimer();
			setTouchState(0);

			InfoString stringInfo = readInfoString(0);
			if (stringInfo.Size > 4)
			{
				SPI_Flash_Read(ref GuiApp.CustomData,
						GuiApp.App.StringDataStart + stringInfo.Start + 4,
						stringInfo.Size - 4
					);
				GuiApp.OveMerrys = (ushort)(stringInfo.Size - 4);
			}

			m_guiCurve = new GuiCurve(this);
			m_guiSlider = new GuiSlider(this);

			GuiObjControl[0] = new GuiObjControl(
				new GuiObjControl.InitHandler(m_guiCurve.GuiCurveInit),
				new GuiObjControl.RefreshHandler(m_guiCurve.GuiCurveRef),
				new GuiObjControl.LoadHandler(m_guiCurve.CurveRefBack)
			);
			GuiObjControl[1] = new GuiObjControl(
				new GuiObjControl.InitHandler(m_guiSlider.GuiSliderObjInit),
				new GuiObjControl.RefreshHandler(m_guiSlider.GuiSliderRef),
				new GuiObjControl.LoadHandler(m_guiSlider.GuiSliderLoad)
			);

			m_runState = 1;
			if (!IsEditor && (m_main_thread == null || m_main_thread.IsAlive))
			{
				if (IsEditor)
					m_runState = 0;
				else
				{
					GuiApp.SendReturn = 2;
					m_main_thread = new Thread(new ThreadStart(runMain));
					m_timer_ms = new Thread(new ThreadStart(timer_5ms));
					m_main_thread.Start();
					m_timer_ms.Start();
				}
			}
		}
		#endregion

		#region guiObjectInit
		private unsafe void guiObjectInit(byte ObjId)
		{
			InfoObject objxinxi = ReadObject(GuiApp.PageInfo.ObjStart + ObjId);
			if (objxinxi.ObjType < 50)
				GuiObjControl[objxinxi.ObjType].OnInit(ref objxinxi, ObjId);
		}
		#endregion

		#region guiObjectRtRef
		private unsafe byte guiObjectRtRef()
		{
			for (byte i = 0; i < GuiApp.PageInfo.ObjCount; i++)
				if (GuiApp.PageObjects[i].RefreshFlag == 1)
				{
					refreshObj(i);
					LcdFirst = true;
					return i;
				}
			return 0xff;
		}
		#endregion

		#region guiPageInit
		private void guiPageInit()
		{
			m_guiCurve.GuiCurvePageInit();
		}
		#endregion

		#region intToStr
		private unsafe byte intToStr(uint num, byte* buf, byte length, byte isend)
		{
			if (length == 0)
				length = getIntStrLen(num);

			if (length > 10)
				length = 10;

			for (ushort i = 0; i < length; i++)
			{
				buf[0] = (byte)(((num / num_pow(10, (byte)((length - i) - 1))) % 10) + 0x30);
				buf++;
			}

			if (isend == 1)
				buf[0] = 0;
			return length;
		}
		#endregion

		#region LCD_AreaSet
		public void LCD_AreaSet(ushort Xpos, ushort Ypos, ushort XEnd, ushort YEnd)
		{
			if (m_isPortrait)
			{
				int num = XEnd - Xpos + 1;
				int num2 = m_lcdDevInfo.Width - Xpos - num;
				int num3 = m_lcdDevInfo.Width - Xpos - 1;
				if (num2 < 0)
					num2 = 0;

				m_screen.Xpos = num2;
				m_screen.Ypos = Ypos;
				m_screen.EndX = num3;
				m_screen.EndY = YEnd;
				m_screen.DX = m_screen.Xpos;
				m_screen.DY = m_screen.Ypos;
			}
			else
			{
				m_screen.Xpos = Xpos;
				m_screen.Ypos = Ypos;
				m_screen.EndX = XEnd;
				m_screen.EndY = YEnd;
				m_screen.DX = m_screen.Xpos;
				m_screen.DY = m_screen.Ypos;
			}
		}
		#endregion

		#region lcdClear
		private void lcdClear(ushort color)
		{
			Graphics.FromImage(ThisBmp[ThisBmpIndex]).Clear(Utility.Get24color(color));
		}
		#endregion

		#region lcdDrawLine
		private void lcdDrawLine(ushort x1, ushort y1, ushort x2, ushort y2, ushort color, byte size)
		{
			int max_dXdY;
			int ddX = 0;
			int ddY = 0;
			int dX = x2 - x1;
			int dY = y2 - y1;
			int incX;
			int incY;

			if (dX > 0)
				incX = 1;
			else if (dX == 0)
				incX = 0;
			else
			{
				incX = -1;
				dX = -dX;
			}

			if (dY > 0)
				incY = 1;
			else if (dY == 0)
				incY = 0;
			else
			{
				incY = -1;
				dY = -dY;
			}

			if (dX > dY)
				max_dXdY = dX;
			else
				max_dXdY = dY;

			int x = x1;
			int y = y1;

			for (ushort i = 0; i <= (max_dXdY + 1); i = (ushort)(i + 1))
			{
				if (size == 1)
					lcdDrawPoint((ushort)x, (ushort)y, color);
				else
					LCD_Fill((ushort)x, (ushort)y, size, size, color);

				ddX += dX;
				ddY += dY;
				if (ddX > max_dXdY)
				{
					ddX -= max_dXdY;
					x += incX;
				}
				if (ddY > max_dXdY)
				{
					ddY -= max_dXdY;
					y += incY;
				}
			}
		}
		#endregion

		#region lcdDrawPoint
		private void lcdDrawPoint(ushort x, ushort y, ushort color)
		{
			if (x < m_lcdDevInfo.Width && y < m_lcdDevInfo.Height)
				ThisBmp[ThisBmpIndex].SetPixel(x, y, Utility.Get24color(color));
		}
		#endregion

		#region lcdDrawRectangle
		private void lcdDrawRectangle(ushort x1, ushort y1, ushort x2, ushort y2, ushort color)
		{
			lcdDrawLine(x1, y1, x2, y1, color, 1);
			lcdDrawLine(x1, y1, x1, y2, color, 1);
			lcdDrawLine(x1, y2, x2, y2, color, 1);
			lcdDrawLine(x2, y1, x2, y2, color, 1);
		}
		#endregion

		#region lcdDrawRectangle3D
		private void lcdDrawRectangle3D(ushort x1, ushort y1, ushort x2, ushort y2, ushort color1, ushort color2)
		{
			lcdDrawLine(x1, y1, x2, y1, color1, 1);
			lcdDrawLine(x1, y1, x1, y2, color1, 1);
			lcdDrawLine(x1, y2, x2, y2, color2, 1);
			lcdDrawLine(x2, y1, x2, y2, color2, 1);
		}
		#endregion

		#region LCD_Fill
		public byte LCD_Fill(ushort sx, ushort sy, ushort w, ushort h, ushort color)
		{
			Graphics.FromImage(ThisBmp[ThisBmpIndex]).FillRectangle(
				new SolidBrush(Utility.Get24color(color)),
				sx, sy, w, h
			);
			return 1;
		}
		#endregion

		#region lcdSetup
		private bool lcdSetup(byte isLandscape)
		{
			ushort w, h;
			if (isLandscape == 0)
			{
				w = GuiApp.App.ScreenWidth;
				h = GuiApp.App.ScreenHeight;
			}
			else if (isLandscape == 1)
			{
				w = GuiApp.App.ScreenHeight;
				h = GuiApp.App.ScreenWidth;
			}
			else
				return false;

			m_lcdDevInfo.Width = w;
			m_lcdDevInfo.Height = h;

			m_screen.EndX = w - 1;
			m_screen.EndY = h - 1;

			SetZoom(0);

			return true;
		}
		#endregion

		#region SetZoom
		public bool SetZoom(int delta)
		{
			if (delta < 0)
			{
				if (m_zoom_m > m_zoom_d)
					--m_zoom_m;
				else
					return false;
			}
			else if (delta > 0)
			{
				if (m_zoom_m < 3 * m_zoom_d)
					++m_zoom_m;
				else
					return false;
			}

			int w = (m_lcdDevInfo.Width * m_zoom_m) / m_zoom_d;
			int h = (m_lcdDevInfo.Height * m_zoom_m) / m_zoom_d;
			if (Width != w || Height != h)
			{
				panelScreen.SuspendLayout();

				Width = (m_lcdDevInfo.Width * m_zoom_m) / m_zoom_d;
				Height = (m_lcdDevInfo.Height * m_zoom_m) / m_zoom_d;
				panelScreen.Left = 0;
				panelScreen.Top = 0;
				panelScreen.Width = Width;
				panelScreen.Height = Height;

				ThisBmp[0] = new Bitmap(Width, Height);
				ThisBmp[1] = new Bitmap(Width, Height);

				m_gc = panelScreen.CreateGraphics();
				// m_gc.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				// m_gc.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				panelScreen.ResumeLayout(false);
				panelScreen.PerformLayout();
			}
			return true;
		}
		#endregion

		#region LCD_WR_POINT
		public void LCD_WR_POINT(uint length, ushort color)
		{
			for (uint i = 0; i < length; i++)
				lcdSendColorData(color);
		}
		#endregion

		#region lcdSendColorData
		private void lcdSendColorData(ushort color)
		{
			try
			{
				if (m_isPortrait)
				{
					if ((m_screen.DY <= m_screen.EndY) && (m_screen.DX <= m_screen.EndX))
					{
						if (color != HmiOptions.ColorTransparent)
							ThisBmp[ThisBmpIndex].SetPixel((m_lcdDevInfo.Width - m_screen.DX) - 1, m_screen.DY, Utility.Get24color(color));
						else if (!HmiOptions.OpenTransparent)
							ThisBmp[ThisBmpIndex].SetPixel((m_lcdDevInfo.Width - m_screen.DX) - 1, m_screen.DY, Utility.Get24color(HmiOptions.ColorTransparentReplace));
						else
							IsTransparent = true;

						m_screen.DX++;
						if (m_screen.DX > m_screen.EndX)
						{
							m_screen.DX = m_screen.Xpos;
							m_screen.DY++;
						}
					}
				}
				else if ((m_screen.DY <= m_screen.EndY) && (m_screen.DX <= m_screen.EndX))
				{
					if (color != HmiOptions.ColorTransparent)
						ThisBmp[ThisBmpIndex].SetPixel(m_screen.DX, m_screen.DY, Utility.Get24color(color));
					else if (!HmiOptions.OpenTransparent)
						ThisBmp[ThisBmpIndex].SetPixel(m_screen.DX, m_screen.DY, Utility.Get24color(HmiOptions.ColorTransparentReplace));
					else
						IsTransparent = true;

					m_screen.DX++;
					if (m_screen.DX > m_screen.EndX)
					{
						m_screen.DX = m_screen.Xpos;
						m_screen.DY++;
					}
				}
			}
			catch { }
		}
		#endregion

		#region loadAllObj
		private void loadAllObj()
		{
			for (int i = 0; i < m_app.HmiPages[GuiApp.Page].HmiObjects.Count; i++)
				if (m_app.HmiPages[GuiApp.Page].HmiObjects[i].Attributes[0].Data[0] != HmiObjType.TIMER
				 && m_app.HmiPages[GuiApp.Page].HmiObjects[i].Attributes[0].Data[0] != HmiObjType.VAR
					)
					LoadObj(m_app.HmiPages[GuiApp.Page].HmiObjects[i]);
		}
		#endregion

		#region LoadObj
		public void LoadObj(HmiObject obj)
		{
			HmiObjectEdit objEdit = new HmiObjectEdit();
			try
			{
				objEdit.HmiObject = obj;
				objEdit.Location = new Point(objEdit.HmiObject.ObjInfo.Panel.X, objEdit.HmiObject.ObjInfo.Panel.Y);
				objEdit.Width = (objEdit.HmiObject.ObjInfo.Panel.EndX - objEdit.HmiObject.ObjInfo.Panel.X + 1);
				objEdit.Height = (objEdit.HmiObject.ObjInfo.Panel.EndY - objEdit.HmiObject.ObjInfo.Panel.Y + 1);
				objEdit.IsMove = true;

				if (objEdit.Width < 3)
					objEdit.Width = 3;

				if (base.Height < 3)
					objEdit.Height = 3;

				objEdit.BackColor = ((obj.Attributes[0].Data[0] == HmiObjType.PAGE)
									? Color.FromArgb(0, 0x48, 0x95, 0xfd)
									: Color.FromArgb(50, 0x48, 0x95, 0xfd)
									);
				objEdit.IsShowName = m_app.IsShowName;
				objEdit.ObjMouseUp += new EventHandler(T_objMouseUp);
				objEdit.ObjChange += new EventHandler(T_ObjChange);
				objEdit.SetApp(m_app);
				objEdit.BackgroundImageLayout = ImageLayout.None;
				objEdit.HmiRunScreen = this;

				panelScreen.Controls.Add(objEdit);

				objEdit.MakeBackground();

				objEdit.BringToFront();
				objEdit.Visible = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error occurred during loading components ".Translate() + ex.Message);
			}
		}
		#endregion

		#region loadRef
		private unsafe bool loadRef(byte index)
		{
			ushort num = (ushort)(GuiApp.PageInfo.ObjStart + index);
			if (num <= GuiApp.PageInfo.ObjEnd)
			{
				InfoObject infoObject = ReadObject(num);
				if (infoObject.ObjType < HmiObjType.OBJECT_TYPE_END)
					GuiObjControl[infoObject.ObjType].OnLoad(ref infoObject, index);
			}
			else
			{
				sendReturnErr(2);
				return false;
			}
			return true;
		}
		#endregion

		#region MakeAttr
		private unsafe bool makeAttr(byte[] buf, ref AttributeRun attr1, ref AttributeRun attr2, byte operation)
		{
			if (attr2.AttrInfo.AttrType < HmiAttributeType.String
			 && attr1.AttrInfo.AttrType < HmiAttributeType.String
				)
			{
				if (operation == 0xA1)
					return (attr1.Value == attr2.Value);
				if (operation == 0x3C)
					return (attr1.Value < attr2.Value);
				if (operation == 0x3E)
					return (attr1.Value > attr2.Value);
				if (operation == 0xA0)
					return (attr1.Value <= attr2.Value);
				if (operation == 0xA2)
					return (attr1.Value >= attr2.Value);
				if (operation == 0x85)
					return (attr1.Value != attr2.Value);
			}
			else if (attr2.AttrInfo.AttrType == HmiAttributeType.String
				  && attr1.AttrInfo.AttrType == HmiAttributeType.String
				  && (operation == 0xA1 || operation == 0x85)
					)
			{
				if (compareString(attr1.Pz, attr2.Pz, 0))
					return (operation == 0xA1);
				return (operation != 0xA1);
			}
			return false;
		}
		#endregion

		private unsafe bool compareString(byte[] v1, string str, uint length)
		{
			fixed(byte* pv1 = &v1[0])
			fixed (byte* numRef = Utility.MergeBytes(Utility.ToBytes(str), Utility.BYTE_ZERO))
				return compareString(pv1, numRef, length);
		}

		private unsafe bool compareString(byte* src1, byte* src2, uint length)
		{
			if (length != 0)
			{
				while (length != 0)
				{
					if (*src1 != *src2)
						return false;
					src1++;
					src2++;
					length--;
				}
				return true;
			}

			while (*src1 == *src2)
			{
				if (*src1 == 0)
					return true;
				src1++;
				src2++;
			}
			return false;
		}

		private unsafe void memcpy(byte* dst, byte* src, uint length)
		{
			while (length != 0)
			{
				*dst = *src;
				dst++;
				src++;
				--length;
			}
		}

		private uint num_pow(byte m, byte n)
		{
			uint num = 1U;
			while ((int)n-- > 0)
				num *= (uint)m;
			return num;
		}

		#region setRefreshFlag
		private unsafe void setRefreshFlag(byte refreshFlag)
		{
			if (refreshFlag > 1)
				refreshFlag = 1;
			for (byte i = 0; i < GuiApp.PageInfo.ObjCount; i++)
				GuiApp.PageObjects[i].RefreshFlag = refreshFlag;
		}
		#endregion
		private void panelScreen_MouseDown(object sender, MouseEventArgs e)
		{
			if (IsEditor)
			{
				if (HmiObjectEdit != null)
					HmiObjectEdit.SetSelected(false);
				ObjMouseUp(null, null);
			}
			else
			{
				m_hmiTime.MoveTime = 0;
				TPDevInf.X = (ushort)e.X;
				TPDevInf.Y = (ushort)e.Y;
				TPDevInf.X0 = TPDevInf.X;
				TPDevInf.Y0 = TPDevInf.Y;
				m_mouse_pos.X = Control.MousePosition.X;
				m_mouse_pos.Y = Control.MousePosition.Y;
				TPDevInf.TouchState = 1;
				m_TPDownEnter = 1;
				TPDevInf.TouchTime = 1;
			}
		}

		private void panelScreen_MouseUp(object sender, MouseEventArgs e)
		{
			if (!IsEditor)
			{
				TPDevInf.TouchTime = 0;
				TPDevInf.TouchState = 0;
				m_TPUpEnter = 1;
			}
		}

		private void panelScreen_Paint(object sender, PaintEventArgs e)
		{
			RefreshPaint();
		}


		private bool picq(ushort x, ushort y, ushort w, ushort h, ushort x2, ushort y2, ref InfoPicture mpicture)
		{
			if (x >= m_lcdDevInfo.Width || y > m_lcdDevInfo.Height)
				return false;

			if ((x2 + w) > mpicture.W)
				w = (ushort)(mpicture.W - x2);

			if ((y2 + h) > mpicture.H)
				h = (ushort)(mpicture.H - y2);

			if ((x + w) > m_lcdDevInfo.Width)
				w = (ushort)(m_lcdDevInfo.Width - x);

			if ((y + h) > m_lcdDevInfo.Height)
				h = (ushort)(m_lcdDevInfo.Height - y);

			uint address = mpicture.DataStart + GuiApp.App.PictureImageStart;
			if (mpicture.IsPotrait == 1)
				address += (uint)(((((y2 + 1) * mpicture.W) - x2) - w) * 2);
			else
				address += (uint)((x2 + (y2 * mpicture.W)) * 2);

			ushort endY = (ushort)((y + h) - 1);
			ushort endX = (ushort)((x + w) - 1);
			for (ushort i = y; i <= endY; i = (ushort)(i + 1))
			{
				LCD_AreaSet(x, i, endX, i);
				sendDate(address, w);
				address += (uint)(mpicture.W * 2);
			}
			return true;
		}

		public unsafe ushort GetU16(Range range, byte[] buf)
		{
			return (ushort)GetU32(range, buf);
		}

		public unsafe uint GetU32(Range range, byte[] bytes)
		{
			AttributeRun[] runattinfArray = new AttributeRun[2];
			ushort index = getStringAttribute(bytes, range, ref runattinfArray[1]);

			runattinfArray[1].AttrInfo.DataStart = dataFrom_We;

			while (index < range.End)
			{
				index++;
				if (bytes[index] == '+' || bytes[index] == '-' || bytes[index] == '*' || bytes[index] == '/')
				{
					byte operation = bytes[index];
					range.Begin = (ushort)(index + 1);
					index = getStringAttribute(bytes, range, ref runattinfArray[0]);
					if (runattinfArray[0].AttrInfo.DataStart == dataFrom_Null
					 && runattinfArray[0].AttrInfo.AttrType > 9
						)
					{
						sendReturnErr(0x1A);
						return 0;
					}
					runattinfArray[1].AttrInfo.DataStart = dataFrom_We;
					if (AttributeAdd(bytes, ref runattinfArray[1], ref runattinfArray[0], ref runattinfArray[1], operation) == 0)
						return 0;
				}
				else
				{
					sendReturnErr(0x1a);
					return 0;
				}
			}

			return runattinfArray[1].Value;
		}

		#region readPage

		public T readInfo<T>(uint position, int index) where T : new()
		{
			byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
			m_reader.BaseStream.Position = position + buffer.Length * index;
			m_reader.BaseStream.Read(buffer, 0, buffer.Length);
			T result = Utility.ToStruct<T>(buffer);
			return result;
		}

		private void readAppInfo()
		{
			GuiApp.App = readInfo<InfoApp>(0, 0);

			m_isPortrait = false;
			if (GuiApp.App.IsPotrait == 1
				/*
				// Portrait only for 480x272 or 800x480
				&& ((GuiApp.App.ScreenWidth == 480 && GuiApp.App.ScreenHeight == 272)
					|| (GuiApp.App.ScreenWidth == 800 && GuiApp.App.ScreenHeight == 480)
					)
				*/
				)
				m_isPortrait = true;
		}

		public InfoObject ReadObject(int objectIndex)
		{
			return readInfo<InfoObject>(GuiApp.App.ObjectStart, objectIndex);
		}

		/// <summary>
		/// Read Page from file
		/// </summary>
		/// <param name="pageIndex">page index</param>
		/// <returns>InfoPage</returns>
		private InfoPage readInfoPage(int pageIndex)
		{
			return readInfo<InfoPage>(GuiApp.App.PageStart, pageIndex);
		}
		#endregion

		public InfoPicture ReadInfoPicture(int pictureIndex)
		{
			return readInfo<InfoPicture>(GuiApp.App.PictureStart, pictureIndex);
		}

		private InfoString readInfoString(int stringIndex)
		{
			return readInfo<InfoString>(GuiApp.App.StringStart, stringIndex);
		}

		private InfoFont readInfoFont(int fontIndex)
		{
			return readInfo<InfoFont>(GuiApp.App.FontStart, fontIndex);
		}

		#region refreshObj
		/// <summary>
		/// 
		/// </summary>
		/// <param name="realiveIndex">index inside page</param>
		/// <returns></returns>
		private unsafe bool refreshObj(byte realiveIndex)
		{
			if (GuiApp.PageObjects[realiveIndex].Visible == 1)
			{
				ushort objectIndex = (ushort)(GuiApp.PageInfo.ObjStart + realiveIndex);
				if (objectIndex > GuiApp.PageInfo.ObjEnd)
				{
					sendReturnErr(2);
					return false;
				}

				InfoObject infoObject = ReadObject(objectIndex);

				if (infoObject.ObjType < HmiObjType.OBJECT_TYPE_END)
					GuiObjControl[infoObject.ObjType].OnRefresh(ref infoObject, realiveIndex);
				else if (infoObject.Panel.Ref != 0xffff)
					setHexIndex((ushort)(infoObject.Panel.Ref + infoObject.StringInfoStart));
			}

			if (realiveIndex == 0)
				setRefreshFlag(1);

			GuiApp.PageObjects[realiveIndex].RefreshFlag = 0;
			return true;
		}
		#endregion

		public unsafe bool RefreshPage(ushort index)
		{
			if (IsEditor)
				return true;

			if (index >= GuiApp.App.PageCount)
			{
				sendReturnErr(3);
				return false;
			}

			// byte[] bytes = new byte[4];

			GuiApp.Page = index;
			GuiApp.DownObjId = 0xff;
			GuiApp.MoveObjId = 0xff;
			GuiApp.PageDataPos = 0;

			clearTimer();
			setTouchState(0);
			clearHexStr();
			guiPageInit();

			fixed (byte* px = &GuiApp.CustomData[GuiApp.OveMerrys])
				GuiApp.PageObjects = (InfoPageObject*)px;
			GuiApp.PageInfo = readInfoPage(index);

			if (GuiApp.PageInfo.InstStart == 0xffff || GuiApp.PageInfo.InstEnd == 0xffff)
				return true;

			Range laction = new Range(0, 2);
			ushort idx;
			for (
				idx = (ushort)(GuiApp.PageInfo.InstStart + 1);
				idx <= GuiApp.PageInfo.InstEnd;
				++idx
				)
			{
				InfoString infoString = readInfoString(idx);
				SPI_Flash_Read(ref m_hexStrBuf,
						GuiApp.App.StringDataStart + infoString.Start,
						infoString.Size
					);

				if (Utility.IndexOf(m_hexStrBuf, "end", laction) != 0xffff)
					break;

				if (infoString.Size > 4)
				{
					uint num3 = BitConverter.ToUInt32(m_hexStrBuf, 0);
					if (num3 == 0xffff)
					{
						if (infoString.Size == 8)
							GuiApp.PageDataPos = BitConverter.ToUInt16(m_hexStrBuf, 4);
					}
					else if ((num3 + infoString.Size - 4) <= GuiApp.CustomData.Length)
					{
						for (ushort i = 4; i < infoString.Size; ++i)
							GuiApp.CustomData[num3++] = m_hexStrBuf[i];
					}
				}
			}
			GuiApp.HexStrIndex = (ushort)(idx + 1);
			return true;
		}

		public byte RefreshPageEdit(HmiPage page)
		{
			clearTimer();
			setTouchState(0);
			panelScreen.Controls.Clear();
			clearHexStr();
			panelScreen.Controls.Clear();
			panelScreen.BackgroundImage = null;

			if (m_app.HmiPages.Count != 0 && page != null)
			{
				GuiApp.Page = (ushort)page.PageId;
				loadAllObj();
			}
			return 1;
		}

		public void RefreshPaint()
		{
			if (!IsEditor && ThisBmp[0] != null)
				LcdFirst = true;
		}

		private unsafe bool RunCode(byte[] buf, string pattern, int paramCount, Range pos)
		{
			int begin = 0;
			int index = 0;
			Range laction = new Range();
			laction.End = pos.End;
			begin = Utility.IndexOf(buf, pattern, pos);
			if (begin == 0xffff)
				return false;

			if (paramCount != 0)
			{
				++begin;
				for (index = 0; index < paramCount; index++)
				{
					if (begin > pos.End)
						return false;

					laction.Begin = begin;
					m_cgCode.CodeResults[index].Begin = begin;
					begin = findSegmentation(buf, laction);
					if (begin == 0xffff)
					{
						if (index != (paramCount - 1))
							return false;
						m_cgCode.CodeResults[index].End = pos.End;
						return true;
					}

					if (begin == m_cgCode.CodeResults[index].Begin)
						return false;

					m_cgCode.CodeResults[index].End = (ushort)(begin - 1);
					++begin;
				}
			}
			return true;
		}

		private void runMain()
		{
			Thread.Sleep(100);

			m_lcdDevInfo.Draw = 0;
			m_lcdDevInfo.DrawColor = 0xF800;

			m_sysTimer.ThSp = 0;
			m_sysTimer.ThSleepUp = 0;
			m_sysTimer.UsSp = 0;

			RefreshPage(0);

			while (m_runState == 1)
			{
				touchScan();
				if (label1.Visible)
				{
					setLabelText(label1, "Run" + m_ComQueue.DiulieYunxing.ToString());
					setLabelText(label2, "Added" + m_ComQueue.Current.ToString());
					setLabelText(label5, "Usart pos" + m_ComQueue.RecvPos.ToString());
				}

				switch (GuiApp.Usart.State)
				{
					case 0:
						if (GuiApp.HexStrIndex != 0xffff)
							scanHexCode();
						else if (m_TPDownEnter == 1)
							scanHotSpotDown();
						else if (m_TPUpEnter == 1)
							scanHotSpotUp();
						else
						{
							ScanComCode();
							for (int i = 0; i < m_timers.Length; i++)
							{
								if (m_timers[i].State == 1
								 && m_timers[i].Value >= m_timers[i].MaxValue)
								{
									GuiApp.HexStrIndex = m_timers[i].CodeBegin;
									GuiApp.TimerIndex = (byte)i;
								}
							}
						}
						break;

					case 6:
						clearComCode();
						GuiApp.Usart.State = 0;
						break;
				}

				if (LcdFirst)
					panelRefresh();
			}
			m_timer_ms.Abort();
			Thread.Sleep(10);
		}

		private void setLabelText(Label label, string text)
		{
			if (label.InvokeRequired)
			{
				label.Invoke(new Action<Label, string>(setLabelText), label, text);
			}
			else
			{
				label.Text = text;
			}
		}

		public void panelRefresh()
		{
			if (!IsEditor)
				m_gc.DrawImage(ThisBmp[0], 0, 0, panelScreen.Width, panelScreen.Height);
			LcdFirst = false;
		}

		public void RunStop()
		{
			try
			{
				m_runState = 0;
				if (IsEditor)
				{
					if (m_reader != null)
					{
						panelScreen.Controls.Clear();
						m_reader.Close();
						m_reader.Dispose();
						m_reader = null;
					}
				}
				else
				{
					while (m_main_thread.IsAlive)
						Application.DoEvents();
					if (m_reader != null)
					{
						m_reader.Close();
						m_reader.Dispose();
						m_reader = null;
					}
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}
		}

		private unsafe void ScanComCode()
		{
			if (m_ComQueue.Queue[m_ComQueue.DiulieYunxing].End != 0xffff)
			{
				Range range = m_ComQueue.Queue[m_ComQueue.DiulieYunxing];
				if (m_ComQueue.CodePause == 0xff)
				{
					if (CodeExecute(m_comBuffer, range, 0))
					{
						LcdFirst = true;
						sendfanhuisuc();
					}
					m_ComQueue.Queue[m_ComQueue.DiulieYunxing].End = 0xffff;
					m_ComQueue.Queue[m_ComQueue.DiulieYunxing].Begin = 0xffff;
					m_ComQueue.DiulieYunxing++;
					if (m_ComQueue.DiulieYunxing == m_ComQueueLength)
						m_ComQueue.DiulieYunxing = 0;
				}
				else
				{
					m_ComQueue.DiulieYunxing++;
					if (m_ComQueue.DiulieYunxing == m_ComQueueLength)
						m_ComQueue.DiulieYunxing = 0;

					if (Utility.IndexOf(m_comBuffer, "com_star", range) != 0xffff)
					{
						m_ComQueue.DiulieYunxing = m_ComQueue.CodePause;
						m_ComQueue.CodePause = 0xff;
						sendfanhuisuc();
					}
				}
			}
			else if (m_ComQueue.CodePause == 0xff)
			{
				m_ComQueue.State = 0;
				if (m_ComQueue.CodePause == 0xff)
				{
					m_ComQueue.State = 0;
					guiObjectRtRef();
				}
			}
		}

		#region scanHexCode()
		private unsafe void scanHexCode()
		{
			if (label1.Visible)
				SendRunCode("hexcmd " + GuiApp.HexStrIndex.ToString());

			InfoString infoString = readInfoString(GuiApp.HexStrIndex);
			GuiApp.HexStrIndex++;
			SPI_Flash_Read(ref m_hexStrBuf, GuiApp.App.StringDataStart + infoString.Start, infoString.Size);
			if (infoString.Size == 0)
				return;

			if (compareString(m_hexStrBuf, "end", 3)
			 || compareString(m_hexStrBuf, "tend", 4)
				)
			{
				GuiApp.HexStrIndex = 0xffff;
				if (GuiApp.TimerIndex < 5)
				{
					m_timers[GuiApp.TimerIndex].Value = 0;
					GuiApp.TimerIndex = 0xff;
				}

				bool run = true;
				while (run)
				{
					run = false;
					if (guiObjectRtRef() == 0xff)
					{
						if (GuiApp.Delay > 0)
						{
							delay_ms(GuiApp.Delay);
							GuiApp.Delay = 0;
						}
					}
					else if (GuiApp.HexStrIndex == 0xffff)
						run = true;
				}
				if (GuiApp.HexStrIndex == 0xffff)
					GuiApp.HexStrIndex = getHexStr();
			}
			else
			{
				Range range = new Range(0, infoString.Size - 1);
				if (CodeExecute(m_hexStrBuf, range, 0))
					LcdFirst = true;
			}
		}
		#endregion

		#region scanHotSpotDown / scanHotSpotUp
		private void scanHotSpotDown()
		{
			sendTouchState(1);
			m_TPDownEnter = 0;
		}

		private void scanHotSpotUp()
		{
			sendTouchState(0);
			m_TPUpEnter = 0;
			GuiApp.DownObjId = 0xff;
			GuiApp.MoveObjId = 0xff;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="state">1 for touch down, 0 for up</param>
		/// <returns></returns>
		private unsafe byte sendTouchState(byte state)
		{
			InfoObject infoObject;
			infoObject.Panel.SendKey = 0;

			byte index = (byte)(GuiApp.PageInfo.ObjEnd - GuiApp.PageInfo.ObjStart);
			if (GuiApp.Touch.SendXY == 1)
			{
				sendByte(0x67);
				sendByte((byte)(TPDevInf.X0 >> 8));
				sendByte((byte)(TPDevInf.X0 >> 0));
				sendByte((byte)(TPDevInf.Y0 >> 8));
				sendByte((byte)(TPDevInf.Y0 >> 0));
				sendByte(state);
				sendEnd();
			}

			if (state == 1)
			{	// Touch Down
				for (ushort num = GuiApp.PageInfo.ObjEnd; num >= GuiApp.PageInfo.ObjStart; --num)
				{
					if (GuiApp.PageObjects[index].Visible == 1)
					{
						infoObject = ReadObject(num);
						if (TPDevInf.X0 > infoObject.Panel.X
						 && TPDevInf.X0 < infoObject.Panel.EndX
						 && TPDevInf.Y0 > infoObject.Panel.Y
						 && TPDevInf.Y0 < infoObject.Panel.EndY
							)
						{
							if (GuiApp.PageObjects[index].TouchState == 1)
							{
								GuiApp.DownObjId = index;
								if (infoObject.ObjType == HmiObjType.OBJECT_TYPE_SLIDER)
									m_guiSlider.GuiSliderPressDown(ref infoObject, index);

								if (infoObject.Panel.Down != 0xffff)
									GuiApp.HexStrIndex = (ushort)(infoObject.Panel.Down + infoObject.StringInfoStart);
							}
							break;
						}
					}
					--index;
					if (index > 0x7F)
						return 0xFF;
				}
			}
			else if (state == 0)
			{	// Touch Up
				if (GuiApp.DownObjId == 0xff || GuiApp.PageObjects[GuiApp.DownObjId].Visible == 0)
					return 0xff;

				index = GuiApp.DownObjId;
				infoObject = ReadObject((ushort)(GuiApp.PageInfo.ObjStart + GuiApp.DownObjId));
				if (infoObject.ObjType == HmiObjType.OBJECT_TYPE_SLIDER)
					m_guiSlider.GuiSliderPressUp(ref infoObject, GuiApp.DownObjId);

				if (infoObject.Panel.Up != 0xFF)
					GuiApp.HexStrIndex = (ushort)(infoObject.StringInfoStart + infoObject.Panel.Up);

				if (infoObject.Panel.Slide != 0xFF)
					setHexIndex((ushort)(infoObject.StringInfoStart + infoObject.Panel.Slide));
			}

			if ((infoObject.Panel.SendKey & (byte)(1 << state)) != 0)
			{
				sendByte(0x65);
				sendByte((byte)GuiApp.Page);
				sendByte(index);
				sendByte(state);
				sendEnd();
			}
			return index;
		}
		#endregion

		private unsafe void send_va(ref AttributeRun att1, bool state)
		{
			fixed (AttributeRun* runattinfRef = &att1)
				send_va(runattinfRef, state);
		}

		private unsafe void send_va(AttributeRun* attr, bool asCmd)
		{
			byte* pz = attr->Pz;
			byte ch;

			if (attr->AttrInfo.AttrType == HmiAttributeType.String)
			{
				if (asCmd)
					sendByte(0x70);
				while ((ch = *pz++) != 0)
					sendByte(ch);
			}
			else
			{
				if (asCmd)
					sendByte(0x71);
				sendByte((byte)(attr->Value >> 0));
				sendByte((byte)(attr->Value >> 8));
				sendByte((byte)(attr->Value >> 16));
				sendByte((byte)(attr->Value >> 24));
			}
			if (asCmd)
				sendEnd();
		}

		#region SendComData
		public unsafe void SendComData(byte data)
		{
			if (m_ComQueue.RecvPos < m_comBuffer.Length)
			{
				if (m_ComQueue.Queue[m_ComQueue.Current].End == 0xffff)
				{
					m_comBuffer[m_ComQueue.RecvPos] = data;
					if (data == 0xFF)
						m_comEnd++;
					else
						m_comEnd = 0;

					if (m_comEnd > 2)
					{	// Package end received
						m_comEnd = 0;
						if (m_ComQueue.Queue[m_ComQueue.Current].Begin == 0xffff)
						{
							m_ComQueue.RecvPos = (ushort)(m_ComQueue.RecvPos + 1);
							m_ComQueue.Queue[m_ComQueue.Current].Begin = m_ComQueue.RecvPos;
							m_ComQueue.Queue[m_ComQueue.Current].End = 0xffff;
						}
						else
						{
							if (m_ComQueue.RecvPos > 3)
								m_ComQueue.Queue[m_ComQueue.Current].End = (ushort)(m_ComQueue.RecvPos - 3);
							else
								m_ComQueue.Queue[m_ComQueue.Current].End = 0;

							m_ComQueue.RecvPos++;
							m_ComQueue.Current++;

							if (m_ComQueue.Current == m_ComQueueLength)
								m_ComQueue.Current = 0;

							if (m_ComQueue.Queue[m_ComQueue.Current].Begin == 0xffff)
							{
								m_ComQueue.Queue[m_ComQueue.Current].Begin = m_ComQueue.RecvPos;
								m_ComQueue.Queue[m_ComQueue.Current].End = 0xffff;
							}
							m_ComQueue.State = 1;
						}
					}
					else if (m_ComQueue.Queue[m_ComQueue.Current].Begin != 0xffff)
					{
						if (m_ComQueue.RecvPos == m_ComQueue.Queue[m_ComQueue.Current].Begin
						 && m_ComQueue.RecvPos > 0
						 && m_ComQueue.State == 0
							)
						{
							m_comBuffer[0] = data;
							m_ComQueue.Queue[m_ComQueue.Current].Begin = 0;
							m_ComQueue.RecvPos = 0;
						}
						m_ComQueue.RecvPos++;
					}
				}
				else
					label4.Text = (int.Parse(label4.Text) + 1).ToString();
			}
			else
			{
				if (m_ComQueue.Queue[m_ComQueue.Current].End == 0xffff && m_ComQueue.Queue[m_ComQueue.Current].Begin != 0xffff)
					m_ComQueue.Queue[m_ComQueue.Current].Begin = 0xffff;
				if (m_ComQueue.State == 0)
					m_ComQueue.RecvPos = 0;
				label3.Text = (int.Parse(label3.Text) + 1).ToString();
			}
		}
		#endregion

		public void SendDataOffset(uint address, ushort offset, ushort WinH, byte WinW)
		{
			while (WinH != 0)
			{
				--WinH;
				sendDate(address, WinW);
				address += offset;
			}
		}

		private void sendDate(uint address, uint length)
		{
			m_reader.BaseStream.Position = address;
			byte[] buffer = new byte[2];
			for (int i = 0; i < length; i++)
			{
				m_reader.BaseStream.Read(buffer, 0, 2);
				lcdSendColorData(buffer.ToU16());
			}
		}

		private void sendReturnErr(byte val)
		{
			if (GuiApp.HexStrIndex != 0xffff || GuiApp.SendReturn == 2 || GuiApp.SendReturn == 3)
			{
				sendByte(val);
				sendEnd();
			}
		}

		private void sendfanhuisuc()
		{
			if ((GuiApp.SendReturn == 1) || (GuiApp.SendReturn == 3))
			{
				sendByte(1);
				sendEnd();
			}
		}

		private void sendFill(ushort color, int length)
		{
			for (int i = 0; i < length; i++)
				lcdSendColorData(color);
		}

		#region sendFont
		private void sendFont(ushort x, ushort y, byte h, byte ch)
		{
			ushort startY = y;
			byte width = 0;
			uint fontStart = m_fontInfo.DataOffset + GuiApp.App.FontImageStart + m_fontInfo.NameEnd + 1;
			if (m_fontInfo.State == 1)
			{
				if (h != 0)
				{
					width = m_fontInfo.Width;
					fontStart += (uint)((((h - 0xA1) * 0x5E + ch - 0xA1) * (m_fontInfo.Width / 8)) * m_fontInfo.Height);
				}
				else
				{
					width = (byte)(m_fontInfo.Width / 2);
					fontStart += (uint)(((0x1ff2 + ch - ' ') * (m_fontInfo.Width / 8)) * m_fontInfo.Height);
				}
			}
			else if (m_fontInfo.State == 0)
			{
				width = m_fontInfo.Width;
				fontStart += (uint)((ch - ' ') * ((m_fontInfo.Height / 8) * m_fontInfo.Width));
			}
			else if (m_fontInfo.State == 2)
			{
				if (h > 0)
					width = m_fontInfo.Width;
				else
					width = (byte)(m_fontInfo.Width / 2);
				fontStart = findFontStart(h, ch);
			}

			ushort fontBytes = (ushort)((m_fontInfo.Height / 8) * width);
			for (uint i = 0; i < fontBytes; i++)
			{
				byte data = SPI_Flash_Read(fontStart + i, 1)[0];
				byte mask = 0x80;
				while (mask != 0)
				{
					if ((data & mask) > 0)
						lcdDrawPoint(x, y, GuiApp.BrushInfo.PointColor);
					mask >>= 1;

					++y;
					if (y >= m_lcdDevInfo.Height)
						break;

					if ((y - startY) == m_fontInfo.Height)
					{
						y = startY;
						x++;
						if (x >= m_lcdDevInfo.Width)
							break;
						break;		//!!!
					}
				}
			}
		}
		#endregion

		#region setAttr
		private unsafe byte setAttr(ref AttributeRun b1, ref AttributeRun b2, byte operation)
		{
			fixed (AttributeRun* runattinfRef = &b1)
			fixed (AttributeRun* runattinfRef2 = &b2)
				return setAttr(runattinfRef, runattinfRef2, operation);
		}

		private unsafe byte setAttr(AttributeRun* b1, AttributeRun* b2, byte operation)
		{
			if (operation == 0
			 && b2->AttrInfo.AttrType < HmiAttributeType.String
			 && b1->AttrInfo.AttrType < HmiAttributeType.String
				)
			{
				if (b2->AttrInfo.DataStart == dataFrom_We)
				{
					b2->Value = b1->Value;
					return 1;
				}
				if (b1->Value > b2->AttrInfo.MaxValue || b1->Value < b2->AttrInfo.MinValue)
				{
					sendReturnErr(0x1b);
					return 0;
				}

				if (b2->AttrInfo.DataStart == dataFrom_Sys_Bl
				 || b2->AttrInfo.DataStart == dataFrom_Sys_IntBl
					)
					return 1;

				if (b2->AttrInfo.DataStart != dataFrom_Sys_Baud)
				{
					if (b2->AttrInfo.DataStart == dataFrom_Sys_Bauds)
						return 0;

					if (b2->AttrInfo.DataStart == dataFrom_Sys_Bkcmd)
					{
						GuiApp.SendReturn = (byte)b1->Value;
						return 1;
					}

					if ((b2->AttrInfo.DataStart > 0xbd) && (b2->AttrInfo.DataStart < 0xc2))
					{
						GuiApp.System[b2->AttrInfo.DataStart - 190] = b1->Value;
						return 1;
					}

					if (b2->AttrInfo.DataStart == dataFrom_RAM)
					{
						ushort length =
									(b2->AttrInfo.Length < b1->AttrInfo.Length)
									? b2->AttrInfo.Length
									: b1->AttrInfo.Length;

						memcpy(b2->Pz, (byte*)&b1->Value, length);
						return 1;
					}
					sendReturnErr(0x1b);
				}
				return 0;
			}
			if (b2->AttrInfo.AttrType == HmiAttributeType.String
			 && b1->AttrInfo.AttrType == HmiAttributeType.String
			 && b2->AttrInfo.DataStart == dataFrom_RAM
				)
			{
				ushort num3;
				ushort len_b1 = getStringLength(b1->Pz);
				ushort len_b2 = getStringLength(b2->Pz);
				byte* pz = b2->Pz;
				if (operation == 0x2b)
				{
					pz += len_b2;
					num3 = (ushort)((b2->AttrInfo.Length - len_b2) - 1);
				}
				else
					num3 = (ushort)(b2->AttrInfo.Length - 1);

				if (num3 > len_b1)
					num3 = len_b1;

				memcpy(pz, b1->Pz, num3);
				pz[num3] = 0;
				return 1;
			}
			sendReturnErr(0x1b);
			return 0;
		}
		#endregion

		#region setHexIndex
		private void setHexIndex(ushort index)
		{
			if (GuiApp.HexStrIndex != 0xffff)
			{
				for (int i = 0; i < 4; i++)
					if (m_hexStrPos[i] == 0xffff)
					{
						m_hexStrPos[i] = GuiApp.HexStrIndex;
						GuiApp.HexStrIndex = index;
						return;
					}

				MessageBox.Show("setHexIndex Error.....");
			}
			else
				GuiApp.HexStrIndex = index;
		}
		#endregion

		public bool ShowPic(ushort x, ushort y, ushort picIndex)
		{
			ushort endX;
			ushort endY;
			uint num = 0;
			if (picIndex >= GuiApp.App.PictureCount)
			{
				if (picIndex == 0xffff)
					return true;
				sendReturnErr(0x04);
				return false;
			}

			InfoPicture infoPicture = ReadInfoPicture(picIndex);

			uint address = infoPicture.DataStart + GuiApp.App.PictureImageStart;
			ushort w = infoPicture.W;
			ushort h = infoPicture.H;

			if ((x + w) > m_lcdDevInfo.Width || (y + h) > m_lcdDevInfo.Height)
			{
				if ((x + w) > m_lcdDevInfo.Width)
				{
					endX = (ushort)(m_lcdDevInfo.Width - 1);
					num = (uint)(x + w - m_lcdDevInfo.Width);
					num *= 2;
				}
				else
					endX = (ushort)(x + w - 1);

				if ((y + h) > m_lcdDevInfo.Height)
					endY = (ushort)(m_lcdDevInfo.Height - 1);
				else
					endY = (ushort)(y + h - 1);

				uint w2 = (uint)(w * 2);
				if (infoPicture.IsPotrait == 1)
					address += num;
				for (ushort i = y; i <= endY; i++)
				{
					LCD_AreaSet(x, i, endX, endY);
					sendDate(address, (uint)(endX - x + 1));
					address += w2;
				}
			}
			else
			{
				endX = (ushort)(x + w - 1);
				endY = (ushort)(y + h - 1);
				LCD_AreaSet(x, y, endX, endY);
				sendDate(address, (uint)(w * h));
			}
			return true;
		}

		private bool showPicQ(ushort x, ushort y, ushort w, ushort h, ushort picIndex)
		{
			return ShowXPic(x, y, w, h, x, y, picIndex);
		}

		public bool ShowXPic(ushort x, ushort y, ushort w, ushort h, ushort x2, ushort y2, ushort picIndex)
		{
			if (picIndex >= GuiApp.App.PictureCount)
			{
				if (picIndex == 0xffff)
					return true;
				sendReturnErr(0x04);
				return false;
			}
			InfoPicture pic = ReadInfoPicture(picIndex);
			return picq(x, y, w, h, x2, y2, ref pic);
		}

		private byte[] SPI_Flash_Read(uint start, int length)
		{
			byte[] bytes = new byte[length];
			try
			{
				m_reader.BaseStream.Position = start;
				m_reader.BaseStream.Read(bytes, 0, length);
			}
			catch (Exception ex)
			{
				MessageBox.Show("SPI_Flash_Read:".Translate() + ex.Message);
			}
			return bytes;
		}
		private void SPI_Flash_Read(ref byte[] dst, uint start, int length)
		{
			SPI_Flash_Read(start, length).CopyTo(dst, 0);
		}

		private unsafe byte StringHZK(ushort x, ushort y, byte* buf, byte mod, ref InfoAction endpoint)
		{
			ushort num = x;
			byte h = 0;
			byte l = 0;
			byte w = 0;
			m_fontInfo = readInfoFont(GuiApp.BrushInfo.FontId);
			if ((GuiApp.BrushInfo.Y + m_fontInfo.Height - 1) <= GuiApp.BrushInfo.EndY)
			{
				while (buf[0] > 0)
				{
					w = (byte)(m_fontInfo.Height / 2);
					while (buf[0] == 13 && buf[1] == 10)
					{
						x = num;
						if (GuiApp.BrushInfo.SpacingY == 0)
							y = (ushort)(y + m_fontInfo.Height);
						else
							y = (ushort)(y + ((ushort)(m_fontInfo.Height + GuiApp.BrushInfo.SpacingY)));

						if (y > ((GuiApp.BrushInfo.EndY - m_fontInfo.Height) + 1))
							return 1;
						endpoint.Wrap = (byte)(endpoint.Wrap + 1);
						buf += 2;
					}
					if (buf[0] <= '~' && buf[0] >= ' ')
					{
						if (x > (GuiApp.BrushInfo.EndX - w + 1))
						{
							x = num;
							if (GuiApp.BrushInfo.SpacingY == 0)
								y = (ushort)(y + m_fontInfo.Height);
							else
								y = (ushort)(y + ((ushort)(m_fontInfo.Height + GuiApp.BrushInfo.SpacingY)));

							if (y > ((GuiApp.BrushInfo.EndY - m_fontInfo.Height) + 1))
								break;

							endpoint.Wrap = (byte)(endpoint.Wrap + 1);
						}
						l = buf[0];
						h = 0;
						endpoint.EndX = (ushort)(x + w);
						endpoint.EndY = (ushort)(y + m_fontInfo.Height);
						if (mod == 1)
						{
							sendFont(x, y, h, l);
						}
						buf++;
						if (buf[0] == 0)
						{
							endpoint.EndX = (ushort)(endpoint.EndX - 1);
							endpoint.EndY = (ushort)(endpoint.EndY - 1);
							return 1;
						}
					}
					else
					{
						w = m_fontInfo.Width;
						if (x > ((GuiApp.BrushInfo.EndX - w) + 1))
						{
							x = num;
							if (GuiApp.BrushInfo.SpacingY == 0xff)
								y = (ushort)(y + m_fontInfo.Height);
							else
								y = (ushort)(y + ((ushort)(m_fontInfo.Height + GuiApp.BrushInfo.SpacingY)));

							if (y > ((GuiApp.BrushInfo.EndY - m_fontInfo.Height) + 1))
								break;

							endpoint.Wrap = (byte)(endpoint.Wrap + 1);
						}
						if (buf[0] == 0)
						{
							endpoint.EndX = (ushort)(endpoint.EndX - 1);
							endpoint.EndY = (ushort)(endpoint.EndY - 1);
							return 1;
						}
						h = buf[0];
						l = buf[1];
						if (l == 0)
							return 1;

						endpoint.EndX = (ushort)(x + w);
						endpoint.EndY = (ushort)(y + m_fontInfo.Height);
						if (mod == 1)
							sendFont(x, y, h, l);

						buf += 2;
					}
					if (GuiApp.BrushInfo.SpacingX == 0)
						x += w;
					else
						x += (ushort)(w + GuiApp.BrushInfo.SpacingX);
				}
			}
			return 1;
		}

		public unsafe uint StringToUInt(Range Pos, byte* bt1)
		{
			uint num = 0;
			uint num2 = 1;
			int end = Pos.End;
			num = 0;
			num2 = 1;
			byte num3 = 0;
			while (num3 < 10)
			{
				if (bt1[end] >= '0' && bt1[end] <= '9')
					num += (uint)((bt1[end] - 0x30) * num2);
				else
					return num;

				num2 *= 10;
				num3 = (byte)(num3 + 1);
				if (end == Pos.Begin)
					return num;
				end = (ushort)(end - 1);
			}
			return num;
		}

		public unsafe uint StrToInt(byte* bt1, int start, int length)
		{
			uint value = 0;
			if (length == 0)
				length = 11;

			while (length > 0 && bt1[start] >= '0' && bt1[start] <= '9')
			{
				value *= 10;
				value += (uint)(bt1[start] - '0');
				start++;
				length--;
			}
			return value;
		}

		public unsafe uint StrToInt(byte* bt1, int length)
		{
			uint value = 0;
			if (length == 0)
				length = 11;
			byte ch;
			while (length != 0)
			{
				ch = *bt1++;
				if (ch < '0' || ch > '9')
					break;
				value *= 10;
				value += (uint)(ch - '0');
				--length;
			}
			return value;
		}
		public unsafe uint StrToInt(byte[] bt1, int start, int length)
		{
			uint value = 0;
			if (length == 0)
				length = 11;
			byte ch;
			while (length != 0)
			{
				ch = bt1[start++];
				if (ch < '0' || ch > '9')
					break;
				value *= 10;
				value += (uint)(ch - '0');
				--length;
			}
			return value;
		}

		public void T_ObjChange(object sender, EventArgs e)
		{
			ObjChange(this, null);
		}

		public void T_objMouseUp(object sender, EventArgs e)
		{
			HmiObjectEdit objedit = (HmiObjectEdit)sender;
			if (HmiObjectEdit != null && HmiObjectEdit != objedit)
				HmiObjectEdit.SetSelected(false);

			HmiObjectEdit = objedit;
			ObjMouseUp(HmiObjectEdit, null);
		}

		private void timer_5ms()
		{
			while (true)
			{
				Thread.Sleep(5);

				m_hmiTime.SystemRuntime += 5;
				m_hmiTime.MoveTime += 5;
				m_hmiTime.Timer20ms += 5;

				if (m_hmiTime.Timer20ms >= 20)
				{
					for (int i = 0; i < m_timers.Length; i++)
						if (m_timers[i].State == 1
						 && m_timers[i].Value < m_timers[i].MaxValue
							)
							m_timers[i].Value = (ushort)(m_timers[i].Value + 20);
					m_hmiTime.Timer20ms = 0;
				}

				if (TPDevInf.TouchTime > 0
				 && TPDevInf.TouchTime < uint.MaxValue
					)
					TPDevInf.TouchTime += 5;
			}
		}

		private void touchScan()
		{
			if (TPDevInf.TouchState == 1)
			{
				TPDevInf.X = (ushort)(Control.MousePosition.X - m_mouse_pos.X + TPDevInf.X0);
				TPDevInf.Y = (ushort)(Control.MousePosition.Y - m_mouse_pos.Y + TPDevInf.Y0);
				if (m_lcdDevInfo.Draw == 1)
				{
					LCD_Fill(TPDevInf.X, TPDevInf.Y, 2, 2, m_lcdDevInfo.DrawColor);
					LcdFirst = true;
				}

				if (GuiApp.MoveObjId < 0xff && m_hmiTime.MoveTime > 20)
				{
					InfoObject infoObject = ReadObject(GuiApp.PageInfo.ObjStart + GuiApp.MoveObjId);
					if (m_guiSlider.GuiSliderPressMove(ref infoObject, GuiApp.MoveObjId) > 0
					 && infoObject.Panel.Slide != 0xff
					 && GuiApp.HexStrIndex == 0xffff
						)
						GuiApp.HexStrIndex = (ushort)(infoObject.Panel.Slide + infoObject.StringInfoStart);

					LcdFirst = true;
					m_hmiTime.MoveTime = 0;
				}
			}
		}

		/// <summary>
		/// Start file
		/// </summary>
		public void StartFile()
		{
			try
			{
				m_reader = new StreamReader(m_binPath);
				readAppInfo();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
		public void PauseScreen()
		{
			try
			{
				if (m_reader != null)
				{
					m_reader.Close();
					m_reader.Dispose();
					m_reader = null;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private unsafe bool XstringHZK(byte* buf)
		{
			InfoAction endpoint = new InfoAction
			{
				Wrap = 0
			};
			ushort x = GuiApp.BrushInfo.X;
			ushort y = GuiApp.BrushInfo.Y;

			if (GuiApp.BrushInfo.EndX >= m_lcdDevInfo.Width)
				GuiApp.BrushInfo.EndX = (ushort)(m_lcdDevInfo.Width - 1);

			if (GuiApp.BrushInfo.EndY >= m_lcdDevInfo.Height)
				GuiApp.BrushInfo.EndY = (ushort)(m_lcdDevInfo.Height - 1);

			if (GuiApp.BrushInfo.sta < 10)
				GuiApp.BrushInfo.sta = (byte)(GuiApp.BrushInfo.sta + 10);

			if ((GuiApp.BrushInfo.sta == 10 || GuiApp.BrushInfo.sta == 12)
			 && GuiApp.BrushInfo.BackColor >= GuiApp.App.PictureCount
				)
			{
				sendReturnErr(4);
				return false;
			}

			if (buf[0] == 0 && GuiApp.BrushInfo.sta < 10)
				GuiApp.BrushInfo.sta = (byte)(GuiApp.BrushInfo.sta + 10);

			if (GuiApp.BrushInfo.sta >= 10)
			{
				if (GuiApp.BrushInfo.sta == 11)
					LCD_Fill(x, y, (ushort)(GuiApp.BrushInfo.EndX - x + 1), (ushort)(GuiApp.BrushInfo.EndY - y + 1), GuiApp.BrushInfo.BackColor);
				else if (GuiApp.BrushInfo.sta == 10)
					ShowXPic(x, y, (ushort)(GuiApp.BrushInfo.EndX - x + 1), (ushort)(GuiApp.BrushInfo.EndY - y + 1), x, y, GuiApp.BrushInfo.BackColor);
				else if (GuiApp.BrushInfo.sta == 12)
					ShowPic(x, y, GuiApp.BrushInfo.BackColor);

				if (buf[0] == 0)
					return true;
				GuiApp.BrushInfo.sta = 3;
			}

			if (GuiApp.BrushInfo.FontId >= GuiApp.App.FontCount)
			{
				sendReturnErr(5);
				return false;
			}

			if ((GuiApp.BrushInfo.XCenter != 0) || (GuiApp.BrushInfo.YCenter != 0))
			{
				ushort num;
				StringHZK(GuiApp.BrushInfo.X, GuiApp.BrushInfo.Y, buf, 0, ref endpoint);
				if (GuiApp.BrushInfo.XCenter > 0 && endpoint.Wrap == 0)
				{
					num = (ushort)(GuiApp.BrushInfo.EndX - endpoint.EndX);
					if (num > 1 && num < m_lcdDevInfo.Width)
					{
						if (GuiApp.BrushInfo.XCenter == 1)
							x = (ushort)(x + (ushort)(num / 2));
						else
							x = (ushort)(x + num);
					}
				}
				if (GuiApp.BrushInfo.YCenter > 0)
				{
					num = (ushort)(GuiApp.BrushInfo.EndY - endpoint.EndY);
					if (num > 1 && num < m_lcdDevInfo.Height)
					{
						if (GuiApp.BrushInfo.YCenter == 1)
							y = (ushort)(y + (ushort)(num / 2));
						else
							y = (ushort)(y + num);
					}
				}
			}
			StringHZK(x, y, buf, 1, ref endpoint);
			return true;
		}

		#region InitializeComponent()
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.panelScreen = new System.Windows.Forms.Panel();
			this.panelScreen.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
			this.label1.Location = new System.Drawing.Point(18, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(43, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "label1";
			this.label1.Visible = false;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
			this.label5.Location = new System.Drawing.Point(18, 184);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(43, 17);
			this.label5.TabIndex = 3;
			this.label5.Text = "label5";
			this.label5.Visible = false;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
			this.label3.Location = new System.Drawing.Point(135, 184);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(15, 17);
			this.label3.TabIndex = 4;
			this.label3.Text = "0";
			this.label3.Visible = false;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
			this.label4.Location = new System.Drawing.Point(183, 184);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(15, 17);
			this.label4.TabIndex = 5;
			this.label4.Text = "0";
			this.label4.Visible = false;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
			this.label2.Location = new System.Drawing.Point(18, 44);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(43, 17);
			this.label2.TabIndex = 8;
			this.label2.Text = "label2";
			this.label2.Visible = false;
			// 
			// panelScreen
			// 
			this.panelScreen.BackColor = System.Drawing.Color.White;
			this.panelScreen.Controls.Add(this.label1);
			this.panelScreen.Controls.Add(this.label2);
			this.panelScreen.Controls.Add(this.label4);
			this.panelScreen.Controls.Add(this.label3);
			this.panelScreen.Controls.Add(this.label5);
			this.panelScreen.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.panelScreen.Location = new System.Drawing.Point(0, 0);
			this.panelScreen.Name = "panelScreen";
			this.panelScreen.Size = new System.Drawing.Size(301, 241);
			this.panelScreen.TabIndex = 9;
			this.panelScreen.Paint += new System.Windows.Forms.PaintEventHandler(this.panelScreen_Paint);
			this.panelScreen.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelScreen_MouseDown);
			this.panelScreen.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelScreen_MouseUp);
			// 
			// HmiRunScreen
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Black;
			this.Controls.Add(this.panelScreen);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Name = "HmiRunScreen";
			this.Size = new System.Drawing.Size(320, 260);
			this.panelScreen.ResumeLayout(false);
			this.panelScreen.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion
	}
}
