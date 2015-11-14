using System;

namespace NextionEditor
{
	public class GuiSlider
	{
		private GuiApplication m_guiApp;
		private HmiSimulator m_runScreen;
		private HmiTPDev m_TPDev;

		public GuiSlider(HmiSimulator runScreen)
		{
			m_runScreen = runScreen;
			m_guiApp = runScreen.GuiApp;
			m_TPDev = runScreen.TPDevInf;
		}

		private unsafe byte changeTouchValue(ref InfoObject obj, InfoSliderParam* param, ushort val)
		{
			byte num;
			byte num2;
			ushort nowVal = param->NowVal;
			if (param->Mode > 0)
			{
				num = (byte)(param->CursorHeight / 2);
				num2 = (byte)(param->CursorHeight - num);
				if (val >= (obj.Panel.EndY - num))
				{
					param->TouchPos = (ushort)(obj.Panel.EndY - num);
					param->NowVal = param->MinVal;
				}
				else if (val <= ((obj.Panel.Y + num2) - 1))
				{
					param->TouchPos = (ushort)((obj.Panel.Y + num2) - 1);
					param->NowVal = param->MaxVal;
				}
				else
				{
					param->TouchPos = val;
					param->NowVal = (ushort)(param->MinVal + ((((long)(obj.Panel.EndY - num2 - val)) * (param->MaxVal - param->MinVal + 1)) / ((long)(obj.Panel.EndY - obj.Panel.Y - param->CursorHeight + 1))
							)
						);
				}
			}
			else
			{
				num = (byte)(param->CursorWidth / 2);
				num2 = (byte)(param->CursorWidth - num);
				if (val <= (obj.Panel.X + num))
				{
					param->TouchPos = (ushort)(obj.Panel.X + num);
					param->NowVal = param->MinVal;
				}
				else if (val >= ((obj.Panel.EndX - num2) + 1))
				{
					param->TouchPos = (ushort)((obj.Panel.EndX - num2) + 1);
					param->NowVal = param->MaxVal;
				}
				else
				{
					param->TouchPos = val;
					param->NowVal = (ushort)(param->MinVal + ((((long)((val - obj.Panel.X) - num)) * ((param->MaxVal - param->MinVal) + 1)) / ((long)(obj.Panel.EndX - obj.Panel.X - param->CursorWidth + 1))));
				}
			}
			if (param->NowVal > 100)
				param->NowVal = param->NowVal;
			if (nowVal != param->NowVal)
			{
				param->LastVal = param->NowVal;
				return 1;
			}
			return 0;
		}

		private unsafe byte clearSliderCursor(ref InfoObject obj, InfoSliderParam* param, ushort* x0, ushort* x1, ushort* y0, ushort* y1)
		{
			ushort x = 0;
			ushort y = 0;
			ushort num3 = 0;
			ushort lastPos = 0;
			if (param->Mode > 0)
			{
				x = x0[0];
				num3 = x1[0];
				if (y1[0] < param->LastPos)
				{
					lastPos = param->LastPos;
					y = (ushort)((lastPos - param->CursorHeight) + 1);
					if (y1[0] >= y)
						y = (ushort)(y1[0] + 1);
				}
				else if (y1[0] > param->LastPos)
				{
					lastPos = param->LastPos;
					y = (ushort)((lastPos - param->CursorHeight) + 1);
					if (lastPos >= y0[0])
						lastPos = (ushort)(y0[0] - 1);
				}
			}
			else
			{
				y = y0[0];
				lastPos = y1[0];
				if (x0[0] < param->LastPos)
				{
					x = param->LastPos;
					num3 = (ushort)((x + param->CursorWidth) - 1);
					if (x1[0] >= x)
						x = (ushort)(x1[0] + 1);
				}
				else if (x0[0] > param->LastPos)
				{
					x = param->LastPos;
					num3 = (ushort)((x + param->CursorWidth) - 1);
					if (num3 >= x0[0])
						num3 = (ushort)(x0[0] - 1);
				}
			}
			switch (param->BackType)
			{
				case 0:
					m_guiApp.BrushInfo.pic = m_runScreen.ReadInfoPicture(param->BackPicId);
					break;
				case 1:
					m_guiApp.BrushInfo.BackColor = param->BackPicId;
					break;
				case 2:
					m_guiApp.BrushInfo.pic = m_runScreen.ReadInfoPicture(param->BackPicId);
					m_guiApp.BrushInfo.X = obj.Panel.X;
					m_guiApp.BrushInfo.Y = obj.Panel.Y;
					break;
			}
			m_guiApp.BrushInfo.sta = param->BackType;
			m_runScreen.ClearBackground(x, y, (ushort)((num3 - x) + 1), (ushort)((lastPos - y) + 1));
			return 1;
		}

		private unsafe byte drawSliderBackGround(ref InfoObject obj, InfoSliderParam* param)
		{
			switch (param->BackType)
			{
				case 0:
					if (param->BackPicId < m_guiApp.App.PictureCount)
					{
						m_guiApp.BrushInfo.pic = m_runScreen.ReadInfoPicture(param->BackPicId);
						break;
					}
					return 0;

				case 1:
					m_guiApp.BrushInfo.BackColor = param->BackPicId;
					break;

				case 2:
					if (param->BackPicId < m_guiApp.App.PictureCount)
					{
						m_guiApp.BrushInfo.X = obj.Panel.X;
						m_guiApp.BrushInfo.Y = obj.Panel.Y;
						m_guiApp.BrushInfo.pic = m_runScreen.ReadInfoPicture(param->BackPicId);
						break;
					}
					return 0;
			}
			m_guiApp.BrushInfo.sta = param->BackType;
			m_runScreen.ClearBackground(
				obj.Panel.X,
				obj.Panel.Y,
				(ushort)(obj.Panel.EndX - obj.Panel.X + 1),
				(ushort)(obj.Panel.EndY - obj.Panel.Y + 1)
			);
			return 1;
		}

		private unsafe byte drawSliderCursor(ref InfoObject obj, InfoSliderParam* param, ushort* x0, ushort* x1, ushort* y0, ushort* y1)
		{
			byte num;
			if (param->Mode > 0)
			{
				num = (byte)(param->CursorHeight / 2);
				x0[0] = (ushort)(((((obj.Panel.EndX - obj.Panel.X) + 1) - param->CursorWidth) / 2) + obj.Panel.X);
				x1[0] = (ushort)((x0[0] + param->CursorWidth) - 1);
				y1[0] = (ushort)(param->TouchPos + num);
				y0[0] = (ushort)((y1[0] - param->CursorHeight) + 1);
			}
			else
			{
				num = (byte)(param->CursorWidth / 2);
				y0[0] = (ushort)(((((obj.Panel.EndY - obj.Panel.Y) + 1) - param->CursorHeight) / 2) + obj.Panel.Y);
				y1[0] = (ushort)((y0[0] + param->CursorHeight) - 1);
				x0[0] = (ushort)(param->TouchPos - num);
				x1[0] = (ushort)((x0[0] + param->CursorWidth) - 1);
			}
			if (param->CursorType > 0)
			{
				if (param->CutsorPicId >= m_guiApp.App.PictureCount)
				{
					return 0;
				}
				m_guiApp.BrushInfo.sta = 2;
				m_guiApp.BrushInfo.pic = m_runScreen.ReadInfoPicture(param->CutsorPicId);
				m_guiApp.BrushInfo.X = x0[0];
				m_guiApp.BrushInfo.Y = y0[0];
			}
			else
			{
				m_guiApp.BrushInfo.sta = 1;
				m_guiApp.BrushInfo.BackColor = param->CutsorPicId;
			}
			m_runScreen.ClearBackground(
				x0[0],
				y0[0],
				param->CursorWidth,
				param->CursorHeight
			);
			return 1;
		}

		private unsafe byte valueToTouchPos(ref InfoObject obj, InfoSliderParam* param, byte ID)
		{
			ushort num;
			byte num2;
			byte num3;
			if (param->Mode > 0)
			{
				num2 = (byte)(param->CursorHeight / 2);
				num3 = (byte)(param->CursorHeight - num2);

				if (param->NowVal >= param->MaxVal)
					param->TouchPos = (ushort)((obj.Panel.Y + num3) - 1);
				else if (param->NowVal <= param->MinVal)
					param->TouchPos = (ushort)(obj.Panel.EndY - num2);
				else
				{
					num = (ushort)(((param->NowVal - param->MinVal) * (((obj.Panel.EndY - obj.Panel.Y) - param->CursorHeight) + 1)) / ((param->MaxVal - param->MinVal) + 1));
					param->TouchPos = (ushort)((obj.Panel.EndY - num3) - num);
				}
			}
			else
			{
				num2 = (byte)(param->CursorWidth / 2);
				num3 = (byte)(param->CursorWidth - num2);

				if (param->NowVal >= param->MaxVal)
					param->TouchPos = (ushort)((obj.Panel.EndX - num3) + 1);
				else if (param->NowVal <= param->MinVal)
					param->TouchPos = (ushort)(obj.Panel.X + num2);
				else
				{
					num = (ushort)((obj.Panel.X + num2) + ((((long)(param->NowVal - param->MinVal)) * (obj.Panel.EndX - obj.Panel.X - param->CursorWidth + 1)) / ((long)(param->MaxVal - param->MinVal + 1))));
					param->TouchPos = num;
				}
			}

			if (!m_runScreen.IsEditor)
				m_guiApp.PageObjects[ID].RefreshFlag = 1;

			param->LastVal = param->NowVal;
			return 1;
		}

		private unsafe byte refreshSliderCursor(ref InfoObject obj, byte ID)
		{
			ushort x0;
			ushort y0;
			ushort x1;
			ushort y1;

			InfoSliderParam* param;
			fixed (byte* px = &m_guiApp.CustomData[obj.AttributeStart])
				param = (InfoSliderParam*)px;

			drawSliderCursor(ref obj, param, &x0, &x1, &y0, &y1);
			clearSliderCursor(ref obj, param, &x0, &x1, &y0, &y1);

			if (param->Mode > 0)
				param->LastPos = y1;
			else
				param->LastPos = x0;

			return 1;
		}

		public unsafe void GuiSliderLoad(ref InfoObject obj, byte ID)
		{
			GuiSliderRef(ref obj, ID);
		}

		public unsafe byte GuiSliderObjInit(ref InfoObject obj, byte ID)
		{
			if (m_runScreen.IsEditor)
				GuiSliderRef(ref obj, ID);
			return 1;
		}

		public unsafe byte GuiSliderPressDown(ref InfoObject obj, byte ID)
		{
			ushort value;
			InfoSliderParam* param;
			fixed (byte* px = &m_guiApp.CustomData[obj.AttributeStart])
				param = (InfoSliderParam*)px;

			if (m_guiApp.MoveObjId != ID)
				m_guiApp.MoveObjId = ID;

			if (param->Mode > 0)
				value = m_TPDev.Y0;
			else
				value = m_TPDev.X0;

			ushort touchPos = param->TouchPos;
			changeTouchValue(ref obj, param, value);

			if (touchPos != param->TouchPos)
				refreshSliderCursor(ref obj, ID);

			return 1;
		}

		public unsafe byte GuiSliderPressMove(ref InfoObject obj, byte ID)
		{
			ushort y;
			byte num = 0;
			InfoSliderParam* pSliRam;
			fixed (byte* px = &m_guiApp.CustomData[obj.AttributeStart])
				pSliRam = (InfoSliderParam*)px;

			ushort touchPos = pSliRam->TouchPos;
			if (pSliRam->Mode > 0)
			{
				y = m_TPDev.Y;
			}
			else
			{
				y = m_TPDev.X;
			}
			num = changeTouchValue(ref obj, pSliRam, y);
			if (touchPos != pSliRam->TouchPos)
				refreshSliderCursor(ref obj, ID);

			return num;
		}

		public unsafe byte GuiSliderPressUp(ref InfoObject obj, byte ID)
		{
			m_guiApp.MoveObjId = 0xff;
			return 1;
		}

		public unsafe byte GuiSliderRef(ref InfoObject obj, byte ID)
		{
			ushort num;
			ushort num2;
			ushort num3;
			ushort num4;
			InfoSliderParam* mymerry;
			if (m_runScreen.IsEditor)
				fixed (byte* px = &m_guiApp.CustomData[0])
					mymerry = (InfoSliderParam*)px;
			else
				fixed (byte* px = &m_guiApp.CustomData[obj.AttributeStart])
					mymerry = (InfoSliderParam*)px;

			if (mymerry->LastVal != mymerry->NowVal)
				valueToTouchPos(ref obj, mymerry, ID);

			drawSliderBackGround(ref obj, mymerry);
			drawSliderCursor(ref obj, mymerry, &num, &num3, &num2, &num4);
			if (mymerry->Mode > 0)
				mymerry->LastPos = num4;
			else
				mymerry->LastPos = num;

			if (!m_runScreen.IsEditor)
				m_guiApp.PageObjects[ID].RefreshFlag = 0;

			return 1;
		}
	}
}