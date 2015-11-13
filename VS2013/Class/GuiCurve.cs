using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NextionEditor
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct InfoCurveParam
	{
		public byte RefFlag;
		public byte BackType;
		public byte GridX;
		public byte GridY;
		public byte ChannelCount;
		public ushort PicID;
		public ushort Width;
		public ushort Height;
		public ushort BackColor;
		public ushort Griclr;
		public ushort BufLen;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct InfoCurveChannelParam
	{
		public ushort Begin;
		public ushort End;
		public ushort Penclr;
		public ushort BufFree;
		public ushort BufNext;
		public ushort DotLen;
	}

	public class GuiCurve
	{
		private const byte REF_PER_LINE = 5;

		public class InfoCurveIndex
		{
			public byte ObjID;
			public byte Channel;
			public ushort Offset;
		}

		private const byte CURVE_OBJ_CHMAX = 4;
		private const byte CURVE_PAGE_COUNT = 5;
		private InfoCurveIndex[] CurveIndex = new InfoCurveIndex[5];
		private GuiApplication m_guiApp;
		private HmiRunScreen m_runScreen;
		private HmiTPDev m_TPDev;

		public GuiCurve(HmiRunScreen scr)
		{
			m_runScreen = scr;
			m_guiApp = scr.GuiApp;
			m_TPDev = scr.TPDevInf;
			for (int idx = 0; idx < CurveIndex.Length; ++idx)
				CurveIndex[idx] = new InfoCurveIndex();
		}

		public unsafe void CurveRefBack(ref InfoObject obj, byte ID)
		{
			InfoCurveParam* curveParam;
			byte[] buffer = new byte[0];
			if (m_runScreen.IsEditor)
			{
				GuiCurvePageInit();
				fixed (byte* px = &m_guiApp.CustomData[0])
					curveParam = (InfoCurveParam*)px;
			}
			else
			{
				fixed (byte *px = &m_guiApp.CustomData[obj.AttributeStart])
					curveParam = (InfoCurveParam*)px;
			}

			if (m_runScreen.IsEditor || m_guiApp.PageObjects[ID].Visible == 1)
			{
				if (curveParam->BackType == 0)
					m_runScreen.ShowXPic(
						obj.Panel.X,
						obj.Panel.Y,
						(ushort)(obj.Panel.EndX - obj.Panel.X + 1),
						(ushort)(obj.Panel.EndY - obj.Panel.Y + 1),
						obj.Panel.X,
						obj.Panel.Y,
						curveParam->PicID
					);
				else if (curveParam->BackType == 2)
					m_runScreen.ShowPic(
						obj.Panel.X,
						obj.Panel.Y,
						curveParam->PicID
					);
				else
				{
					short num;
					uint length = (uint)(curveParam->Width * curveParam->Height);
					m_runScreen.LCD_AreaSet(
						obj.Panel.X,
						obj.Panel.Y,
						obj.Panel.EndX,
						obj.Panel.EndY
					);
					m_runScreen.LCD_WR_POINT(length, curveParam->BackColor);
					if (curveParam->GridX > 0)
					{
						for (num = (short)obj.Panel.X; num <= obj.Panel.EndX; num = (short)(num + curveParam->GridX))
						{
							m_runScreen.LCD_AreaSet((ushort)num, obj.Panel.Y, (ushort)num, obj.Panel.EndY);
							m_runScreen.LCD_WR_POINT(curveParam->Height, curveParam->Griclr);
						}
					}
					if (curveParam->GridY > 0)
						for (num = (short)obj.Panel.EndY; num >= obj.Panel.Y; num = (short)(num - curveParam->GridY))
						{
							m_runScreen.LCD_AreaSet(obj.Panel.X, (ushort)num, obj.Panel.EndX, (ushort)num);
							m_runScreen.LCD_WR_POINT(curveParam->Width, curveParam->Griclr);
						}
				}
			}
		}

		public unsafe bool GuiCruveCmd(byte[] cmd, Range cmdRange, Range range)
		{
			range.End = range.Begin;
			while (cmd[range.End] != ',')
			{
				range.End++;
				if (range.End > cmdRange.End)
					return false;	// Not found ','
			}
			range.End--;
			byte data = (byte)m_runScreen.GetU16(range, cmd);
			if (data >= 0xff)
				return false;

			int index;
			for (index = 0; index < 5; index++)
				if (CurveIndex[index].ObjID == data)
					break;

			if (index < 5)
			{
				range.Begin = range.End + 2;
				byte channel = (byte)(cmd[range.Begin] - '0');
				if (channel >= 4)
					return false;
				range.Begin = range.Begin + 2;
				range.End = cmdRange.End;
				CurveIndex[index].Channel = channel;
				data = (byte)m_runScreen.StrToInt(cmd, range.Begin, range.End - range.Begin + 1);
				GuiCurveAdd(CurveIndex[index], data);
			}
			else
				return false;
			return true;
		}

		public unsafe void GuiCurveAdd(InfoCurveIndex index, byte data)
		{
			InfoCurveParam* curve_paramPtr;
			fixed(byte* px = &m_guiApp.CustomData[index.Offset])
				curve_paramPtr = (InfoCurveParam*)px;

			if (index.Channel < curve_paramPtr->ChannelCount)
			{
				InfoCurveChannelParam* curve_channel_paramPtr;

				fixed(byte* px = &m_guiApp.CustomData[
										index.Offset
										+ sizeof(InfoCurveParam)
										+ sizeof(InfoCurveChannelParam) * index.Channel
										])
					curve_channel_paramPtr = (InfoCurveChannelParam*)px;

				if (curve_channel_paramPtr->BufFree > 0)
				{
					if (data > curve_paramPtr->Height)
						m_guiApp.CustomData[curve_channel_paramPtr->BufNext] = (byte)(curve_paramPtr->Height - 1);
					else
						m_guiApp.CustomData[curve_channel_paramPtr->BufNext] = data;
					
					if (curve_channel_paramPtr->BufNext != curve_channel_paramPtr->End)
						curve_channel_paramPtr->BufNext = (ushort)(curve_channel_paramPtr->BufNext + 1);
					else
						curve_channel_paramPtr->BufNext = (ushort)curve_channel_paramPtr->Begin;
					
					if (curve_channel_paramPtr->DotLen < curve_paramPtr->Width)
						curve_channel_paramPtr->DotLen = (ushort)(curve_channel_paramPtr->DotLen + 1);
					
					curve_channel_paramPtr->BufFree = (ushort)(curve_channel_paramPtr->BufFree - 1);
					m_runScreen.GuiApp.PageObjects[index.ObjID].RefreshFlag = 1;
				}
			}
		}

		public unsafe void GuiCurveCheckRef()
		{
			for (byte i = 0; i < 5; i = (byte)(i + 1))
				if (CurveIndex[i].ObjID != 0xff)
				{
					InfoCurveParam* curve_paramPtr = (InfoCurveParam*)m_guiApp.CustomData[CurveIndex[i].Offset];
					if (curve_paramPtr->RefFlag > 0)
					{
						InfoObject infoObject = m_runScreen.ReadObject(CurveIndex[i].ObjID);
						GuiCurveRef(ref infoObject, CurveIndex[i].ObjID);
						m_runScreen.LcdFirst = true;
					}
				}
		}

		public byte GuiCurveInit(ref InfoObject obj, byte ID)
		{
			if (m_runScreen.IsEditor)
				GuiCurvePageInit();

			for (byte i = 0; i < 5; i = (byte)(i + 1))
				if (CurveIndex[i].ObjID == 0xff)
				{
					CurveIndex[i].ObjID = ID;
					CurveIndex[i].Offset = obj.AttributeStart;
					if (m_runScreen.IsEditor)
						CurveRefBack(ref obj, ID);
					break;
				}
			return 1;
		}

		public void GuiCurvePageInit()
		{
			for (int i = 0; i < 5; i++)
			{
				CurveIndex[i].ObjID = 0xff;
				CurveIndex[i].Channel = 0xff;
				CurveIndex[i].Offset = 0xff;
			}
		}

		public unsafe byte GuiCurveRef(ref InfoObject obj, byte ID)
		{
			byte qyt = 0;
			byte[] buffer = new byte[4];
			ushort[] numArray = new ushort[4];
			ushort[] numArray2 = new ushort[4];
			ushort[] numArray3 = new ushort[4];
			ushort[] numArray4 = new ushort[4];
			uint address = 0;
			InfoPicture pic = new InfoPicture();
			InfoCurveParam* curve_paramPtr;
			fixed(byte* px = &m_guiApp.CustomData[obj.AttributeStart])
				curve_paramPtr = (InfoCurveParam*)px;
			InfoCurveChannelParam*[] curve_channel_paramPtrArray = new InfoCurveChannelParam*[4];
			short x = 0;
			short num11 = 0;
			if (curve_paramPtr->ChannelCount <= 4)
			{
				byte num3;
				for (num3 = 0; num3 < curve_paramPtr->ChannelCount; num3++)
					curve_channel_paramPtrArray[num3] = (InfoCurveChannelParam*)((((uint)curve_paramPtr) + sizeof(InfoCurveParam)) + (sizeof(InfoCurveChannelParam) * num3));

				byte num2 = 0;
				num3 = 0;
				while (num3 < curve_paramPtr->ChannelCount)
				{
					buffer[num3] = 1;
					numArray[num3] = curve_channel_paramPtrArray[num3]->BufNext;
					numArray3[num3] = curve_channel_paramPtrArray[num3]->DotLen;
					numArray4[num3] = numArray3[num3];
					if (numArray3[num3] > 0)
					{
						num2 = (byte)(num2 + 1);
					}
					num3 = (byte)(num3 + 1);
				}
				if (num2 == 0)
				{
					CurveRefBack(ref obj, ID);
				}
				else
				{
					if (curve_paramPtr->BackType == 0)
					{
						qyt = 5;
						pic = m_runScreen.ReadInfoPicture(curve_paramPtr->PicID);
						address = pic.DataStart + m_guiApp.App.PictureImageStart;
						if (pic.IsPotrait == 1)
							address += (uint)((((obj.Panel.Y + 1) * pic.W) - obj.Panel.X) * 2);
						else
							address += (uint)(((obj.Panel.Y * pic.W) + obj.Panel.X) * 2);
					}
					else if (curve_paramPtr->BackType == 2)
					{
						qyt = 5;
						pic = m_runScreen.ReadInfoPicture(curve_paramPtr->PicID);
						address = pic.DataStart + m_guiApp.App.PictureImageStart;
					}
					else
					{
						x = (short)obj.Panel.X;
						num11 = (short)(x + curve_paramPtr->GridX);
					}
					for (short i = (short)obj.Panel.X; i <= obj.Panel.EndX; i = (short)(i + qyt))
					{
						short endx = (short)((i + 5) - 1);
						if (endx > obj.Panel.EndX)
						{
							endx = (short)obj.Panel.EndX;
						}
						if (curve_paramPtr->BackType == 0)
						{
							if (pic.IsPotrait == 1)
								address -= (uint)(qyt * 2);

							m_runScreen.LCD_AreaSet((ushort)i, obj.Panel.Y, (ushort)endx, obj.Panel.EndY);
							m_runScreen.SendDataOffset(address, (ushort)(pic.W << 1), curve_paramPtr->Height, (byte)((endx - i) + 1));

							if (pic.IsPotrait == 0)
								address += (uint)(qyt * 2);
						}
						else if (curve_paramPtr->BackType == 2)
						{
							if (pic.IsPotrait == 1)
								address -= (uint)(qyt * 2);
							
							m_runScreen.LCD_AreaSet((ushort)i, obj.Panel.Y, (ushort)endx, obj.Panel.EndY);
							m_runScreen.SendDataOffset(address, (ushort)(pic.W << 1), curve_paramPtr->Height, (byte)((endx - i) + 1));
							if (pic.IsPotrait == 0)
								address += (uint)(qyt * 2);
						}
						else
						{
							short endy;
							short y;
							if (curve_paramPtr->GridX > 0)
							{
								ushort num14;
								ushort num15;
								if (i == x)
								{
									m_runScreen.LCD_AreaSet((ushort)i, obj.Panel.Y, (ushort)i, obj.Panel.EndY);
									m_runScreen.LCD_WR_POINT(curve_paramPtr->Height, curve_paramPtr->Griclr);
									num14 = (ushort)(i + 1);
								}
								else
									num14 = (ushort)i;
								
								if ((num14 + 5) >= num11)
								{
									num15 = (ushort)(num11 - 1);
									x = num11;
									num11 = (short)(num11 + curve_paramPtr->GridX);
								}
								else
									num15 = (ushort)((num14 + 5) - 1);
								
								qyt = (byte)((num15 - i) + 1);
								if ((i + qyt) > obj.Panel.EndX)
								{
									num15 = (ushort)endx;
									qyt = (byte)((endx - i) + 1);
								}
								if (curve_paramPtr->GridY > 0)
								{
									endy = (short)obj.Panel.EndY;
									while (endy > obj.Panel.Y)
									{
										y = (short)((endy - curve_paramPtr->GridY) + 1);
										if (y < obj.Panel.Y)
										{
											y = (short)obj.Panel.Y;
										}
										m_runScreen.LCD_AreaSet(num14, (ushort)y, num15, (ushort)endy);
										m_runScreen.LCD_WR_POINT((uint)(((num15 - num14) + 1) * (endy - y)), curve_paramPtr->BackColor);
										m_runScreen.LCD_WR_POINT((uint)((num15 - num14) + 1), curve_paramPtr->Griclr);
										endy = (short)(endy - curve_paramPtr->GridY);
									}
								}
								else
								{
									address = (uint)(curve_paramPtr->Height * ((num15 - num14) + 1));
									m_runScreen.LCD_AreaSet(num14, obj.Panel.Y, num15, obj.Panel.EndY);
									m_runScreen.LCD_WR_POINT(address, curve_paramPtr->BackColor);
								}
							}
							else if (curve_paramPtr->GridY > 0)
							{
								for (endy = (short)obj.Panel.EndY; endy > obj.Panel.Y; endy = (short)(endy - curve_paramPtr->GridY))
								{
									y = (short)((endy - curve_paramPtr->GridY) + 1);
									if (y < obj.Panel.Y)
									{
										y = (short)obj.Panel.Y;
									}
									m_runScreen.LCD_AreaSet((ushort)i, (ushort)y, (ushort)endx, (ushort)endy);
									qyt = (byte)((endx - i) + 1);
									m_runScreen.LCD_WR_POINT((uint)(qyt * (endy - y)), curve_paramPtr->BackColor);
									m_runScreen.LCD_WR_POINT(qyt, curve_paramPtr->Griclr);
								}
							}
							else
							{
								qyt = (byte)((endx - i) + 1);
								address = (uint)(curve_paramPtr->Height * ((endx - i) + 1));
								m_runScreen.LCD_AreaSet((ushort)i, obj.Panel.Y, (ushort)endx, obj.Panel.EndY);
								m_runScreen.LCD_WR_POINT(address, curve_paramPtr->BackColor);
							}
						}
						num3 = 0;
						while (num3 < curve_paramPtr->ChannelCount)
						{
							if (numArray4[num3] > 0)
							{
								for (num2 = 0; num2 < qyt; num2 = (byte)(num2 + 1))
								{
									byte num6;
									if (numArray[num3] == curve_channel_paramPtrArray[num3]->Begin)
										numArray[num3] = (ushort)curve_channel_paramPtrArray[num3]->End;
									else
										numArray[num3] = (ushort)(numArray[num3] - 1);
									
									if (numArray[num3] == curve_channel_paramPtrArray[num3]->Begin)
										numArray2[num3] = (ushort)curve_channel_paramPtrArray[num3]->End;
									else
										numArray2[num3] = (ushort)(numArray[num3] - 1);
									
									byte num5 = m_guiApp.CustomData[numArray[num3]];
									byte num4 = m_guiApp.CustomData[numArray2[num3]];
									if (buffer[num3] > 0)
									{
										num4 = num5;
										buffer[num3] = 0;
									}

									if (numArray4[num3] == 1)
										num4 = num5;
									if (num5 > num4)
									{
										num6 = (byte)((num5 - num4) + 1);
										m_runScreen.LCD_AreaSet((ushort)(i + num2), (ushort)(obj.Panel.EndY - num5), (ushort)(i + num2), (ushort)(obj.Panel.EndY - num4));
										m_runScreen.LCD_WR_POINT(num6, curve_channel_paramPtrArray[num3]->Penclr);
									}
									else
									{
										num6 = (byte)((num4 - num5) + 1);
										m_runScreen.LCD_AreaSet((ushort)(i + num2), (ushort)(obj.Panel.EndY - num4), (ushort)(i + num2), (ushort)(obj.Panel.EndY - num5));
										m_runScreen.LCD_WR_POINT(num6, curve_channel_paramPtrArray[num3]->Penclr);
									}
									numArray4[num3] = (ushort)(numArray4[num3] - 1);
									if (numArray4[num3] == 0)
										break;
								}
							}
							num3 = (byte)(num3 + 1);
						}
					}
					for (num3 = 0; num3 < curve_paramPtr->ChannelCount; num3 = (byte)(num3 + 1))
					{
						if (curve_channel_paramPtrArray[num3]->DotLen == curve_paramPtr->Width)
						{
							curve_channel_paramPtrArray[num3]->BufFree = (ushort)((curve_paramPtr->BufLen - curve_paramPtr->Width) - (curve_channel_paramPtrArray[num3]->DotLen - numArray3[num3]));
						}
					}
				}
				curve_paramPtr->RefFlag = 0;
			}
			return 0;
		}
	}
}
