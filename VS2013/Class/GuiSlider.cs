using System;

namespace NextionEditor
{
    public class GuiSlider
    {
        private GuiApplication m_guiApp;
        private HmiRunScreen m_runScreen;
        private HmiTPDev m_TPDev;

        public GuiSlider(HmiRunScreen scr)
        {
            m_runScreen = scr;
            m_guiApp = scr.GuiApp;
            m_TPDev = scr.TPDevInf;
        }

        private unsafe byte ChangeTouchValue(ref InfoObject obj, InfoSliderParam* pSliRam, ushort val)
        {
            byte num;
            byte num2;
            ushort nowVal = pSliRam->NowVal;
			if (pSliRam->Mode > 0)
			{
				num = (byte)(pSliRam->CursorHeight / 2);
				num2 = (byte)(pSliRam->CursorHeight - num);
				if (val >= (obj.Panel.EndY - num))
				{
					pSliRam->TouchPos = (ushort)(obj.Panel.EndY - num);
					pSliRam->NowVal = pSliRam->MinVal;
				}
				else if (val <= ((obj.Panel.Y + num2) - 1))
				{
					pSliRam->TouchPos = (ushort)((obj.Panel.Y + num2) - 1);
					pSliRam->NowVal = pSliRam->MaxVal;
				}
				else
				{
					pSliRam->TouchPos = val;
					pSliRam->NowVal = (ushort)(pSliRam->MinVal + ((((long)(obj.Panel.EndY - num2 - val)) * (pSliRam->MaxVal - pSliRam->MinVal + 1)) / ((long)(obj.Panel.EndY - obj.Panel.Y - pSliRam->CursorHeight + 1))
							)
						);
				}
			}
			else
			{
				num = (byte)(pSliRam->CursorWidth / 2);
				num2 = (byte)(pSliRam->CursorWidth - num);
				if (val <= (obj.Panel.X + num))
				{
					pSliRam->TouchPos = (ushort)(obj.Panel.X + num);
					pSliRam->NowVal = pSliRam->MinVal;
				}
				else if (val >= ((obj.Panel.EndX - num2) + 1))
				{
					pSliRam->TouchPos = (ushort)((obj.Panel.EndX - num2) + 1);
					pSliRam->NowVal = pSliRam->MaxVal;
				}
				else
				{
					pSliRam->TouchPos = val;
					pSliRam->NowVal = (ushort)(pSliRam->MinVal + ((((long)((val - obj.Panel.X) - num)) * ((pSliRam->MaxVal - pSliRam->MinVal) + 1)) / ((long)(obj.Panel.EndX - obj.Panel.X - pSliRam->CursorWidth + 1))));
				}
			}
            if (pSliRam->NowVal > 100)
                pSliRam->NowVal = pSliRam->NowVal;
            if (nowVal != pSliRam->NowVal)
            {
                pSliRam->LastVal = pSliRam->NowVal;
                return 1;
            }
            return 0;
        }

        private unsafe byte ClearSliderCursor(ref InfoObject obj, InfoSliderParam* pSliRam, ushort* CurXPos, ushort* CurXEnd, ushort* CurYPos, ushort* CurYEnd)
        {
            ushort x = 0;
            ushort y = 0;
            ushort num3 = 0;
            ushort lastPos = 0;
            if (pSliRam->Mode > 0)
            {
                x = CurXPos[0];
                num3 = CurXEnd[0];
                if (CurYEnd[0] < pSliRam->LastPos)
                {
                    lastPos = pSliRam->LastPos;
                    y = (ushort) ((lastPos - pSliRam->CursorHeight) + 1);
                    if (CurYEnd[0] >= y)
                        y = (ushort) (CurYEnd[0] + 1);
                }
                else if (CurYEnd[0] > pSliRam->LastPos)
                {
                    lastPos = pSliRam->LastPos;
                    y = (ushort) ((lastPos - pSliRam->CursorHeight) + 1);
                    if (lastPos >= CurYPos[0])
                        lastPos = (ushort) (CurYPos[0] - 1);
                }
            }
            else
            {
                y = CurYPos[0];
                lastPos = CurYEnd[0];
                if (CurXPos[0] < pSliRam->LastPos)
                {
                    x = pSliRam->LastPos;
                    num3 = (ushort) ((x + pSliRam->CursorWidth) - 1);
                    if (CurXEnd[0] >= x)
                        x = (ushort) (CurXEnd[0] + 1);
                }
                else if (CurXPos[0] > pSliRam->LastPos)
                {
                    x = pSliRam->LastPos;
                    num3 = (ushort) ((x + pSliRam->CursorWidth) - 1);
                    if (num3 >= CurXPos[0])
                        num3 = (ushort) (CurXPos[0] - 1);
                }
            }
            switch (pSliRam->BackType)
            {
                case 0:
					m_guiApp.BrushInfo.pic = m_runScreen.ReadInfoPicture(pSliRam->BackPicId);
                    break;
                case 1:
                    m_guiApp.BrushInfo.BackColor = pSliRam->BackPicId;
                    break;
                case 2:
					m_guiApp.BrushInfo.pic = m_runScreen.ReadInfoPicture(pSliRam->BackPicId);
                    m_guiApp.BrushInfo.X = obj.Panel.X;
                    m_guiApp.BrushInfo.Y = obj.Panel.Y;
                    break;
            }
            m_guiApp.BrushInfo.sta = pSliRam->BackType;
            m_runScreen.ClearBackground(x, y, (ushort) ((num3 - x) + 1), (ushort) ((lastPos - y) + 1));
            return 1;
        }

		private unsafe byte DrawSliderBackGround(ref InfoObject obj, InfoSliderParam* pSliRam)
		{
			switch (pSliRam->BackType)
			{
				case 0:
					if (pSliRam->BackPicId < m_guiApp.App.PictureCount)
					{
						m_guiApp.BrushInfo.pic = m_runScreen.ReadInfoPicture(pSliRam->BackPicId);
						break;
					}
					return 0;

				case 1:
					m_guiApp.BrushInfo.BackColor = pSliRam->BackPicId;
					break;

				case 2:
					if (pSliRam->BackPicId < m_guiApp.App.PictureCount)
					{
						m_guiApp.BrushInfo.X = obj.Panel.X;
						m_guiApp.BrushInfo.Y = obj.Panel.Y;
						m_guiApp.BrushInfo.pic = m_runScreen.ReadInfoPicture(pSliRam->BackPicId);
						break;
					}
					return 0;
			}
			m_guiApp.BrushInfo.sta = pSliRam->BackType;
			m_runScreen.ClearBackground(
				obj.Panel.X,
				obj.Panel.Y,
				(ushort)(obj.Panel.EndX - obj.Panel.X + 1),
				(ushort)(obj.Panel.EndY - obj.Panel.Y + 1)
			);
			return 1;
		}

        private unsafe byte DrawSliderCursor(ref InfoObject obj, InfoSliderParam* pSliRam, ushort* CurXPos, ushort* CurXEnd, ushort* CurYPos, ushort* CurYEnd)
        {
            byte num;
            if (pSliRam->Mode > 0)
            {
                num = (byte) (pSliRam->CursorHeight / 2);
                CurXPos[0] = (ushort) (((((obj.Panel.EndX - obj.Panel.X) + 1) - pSliRam->CursorWidth) / 2) + obj.Panel.X);
                CurXEnd[0] = (ushort) ((CurXPos[0] + pSliRam->CursorWidth) - 1);
                CurYEnd[0] = (ushort) (pSliRam->TouchPos + num);
                CurYPos[0] = (ushort) ((CurYEnd[0] - pSliRam->CursorHeight) + 1);
            }
            else
            {
                num = (byte) (pSliRam->CursorWidth / 2);
                CurYPos[0] = (ushort) (((((obj.Panel.EndY - obj.Panel.Y) + 1) - pSliRam->CursorHeight) / 2) + obj.Panel.Y);
                CurYEnd[0] = (ushort) ((CurYPos[0] + pSliRam->CursorHeight) - 1);
                CurXPos[0] = (ushort) (pSliRam->TouchPos - num);
                CurXEnd[0] = (ushort) ((CurXPos[0] + pSliRam->CursorWidth) - 1);
            }
            if (pSliRam->CursorType > 0)
            {
                if (pSliRam->CutsorPicId >= m_guiApp.App.PictureCount)
                {
                    return 0;
                }
                m_guiApp.BrushInfo.sta = 2;
				m_guiApp.BrushInfo.pic = m_runScreen.ReadInfoPicture(pSliRam->CutsorPicId);
                m_guiApp.BrushInfo.X = CurXPos[0];
                m_guiApp.BrushInfo.Y = CurYPos[0];
            }
            else
            {
                m_guiApp.BrushInfo.sta = 1;
                m_guiApp.BrushInfo.BackColor = pSliRam->CutsorPicId;
            }
            m_runScreen.ClearBackground(
				CurXPos[0],
				CurYPos[0],
				pSliRam->CursorWidth,
				pSliRam->CursorHeight
			);
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
            ushort num;
            InfoSliderParam* pSliRam;
			fixed (byte* px = &m_guiApp.CustomData[obj.AttributeStart])
				pSliRam = (InfoSliderParam*)px;
            ushort touchPos = pSliRam->TouchPos;
            if (m_guiApp.MoveObjId != ID)
                m_guiApp.MoveObjId = ID;

            if (pSliRam->Mode > 0)
                num = m_TPDev.Y0;
            else
                num = m_TPDev.X0;
            
			ChangeTouchValue(ref obj, pSliRam, num);
            if (touchPos != pSliRam->TouchPos)
                RefSliderCursor(ref obj, ID);
            
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
            num = ChangeTouchValue(ref obj, pSliRam, y);
            if (touchPos != pSliRam->TouchPos)
                RefSliderCursor(ref obj, ID);

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
                ValueToTouchPos(ref obj, mymerry, ID);
            
			DrawSliderBackGround(ref obj, mymerry);
            DrawSliderCursor(ref obj, mymerry, &num, &num3, &num2, &num4);
            if (mymerry->Mode > 0)
                mymerry->LastPos = num4;
            else
                mymerry->LastPos = num;
            
			if (!m_runScreen.IsEditor)
                m_guiApp.PageObjects[ID].RefreshFlag = 0;
            
			return 1;
        }

        private unsafe byte RefSliderCursor(ref InfoObject obj, byte ID)
        {
            ushort num;
            ushort num2;
            ushort num3;
            ushort num4;
            InfoSliderParam* pSliRam;
			fixed (byte* px = &m_guiApp.CustomData[obj.AttributeStart])
				pSliRam = (InfoSliderParam*)px;
            DrawSliderCursor(ref obj, pSliRam, &num, &num3, &num2, &num4);
            ClearSliderCursor(ref obj, pSliRam, &num, &num3, &num2, &num4);
            if (pSliRam->Mode > 0)
                pSliRam->LastPos = num4;
            else
                pSliRam->LastPos = num;
            return 1;
        }

        private unsafe byte ValueToTouchPos(ref InfoObject obj, InfoSliderParam* pSliRam, byte ID)
        {
            ushort num;
            byte num2;
            byte num3;
            if (pSliRam->Mode > 0)
            {
                num2 = (byte) (pSliRam->CursorHeight / 2);
                num3 = (byte) (pSliRam->CursorHeight - num2);
                if (pSliRam->NowVal >= pSliRam->MaxVal)
                {
                    pSliRam->TouchPos = (ushort) ((obj.Panel.Y + num3) - 1);
                }
                else if (pSliRam->NowVal <= pSliRam->MinVal)
                {
                    pSliRam->TouchPos = (ushort) (obj.Panel.EndY - num2);
                }
                else
                {
                    num = (ushort) (((pSliRam->NowVal - pSliRam->MinVal) * (((obj.Panel.EndY - obj.Panel.Y) - pSliRam->CursorHeight) + 1)) / ((pSliRam->MaxVal - pSliRam->MinVal) + 1));
                    pSliRam->TouchPos = (ushort) ((obj.Panel.EndY - num3) - num);
                }
            }
            else
            {
                num2 = (byte) (pSliRam->CursorWidth / 2);
                num3 = (byte) (pSliRam->CursorWidth - num2);
                if (pSliRam->NowVal >= pSliRam->MaxVal)
                {
                    pSliRam->TouchPos = (ushort) ((obj.Panel.EndX - num3) + 1);
                }
                else if (pSliRam->NowVal <= pSliRam->MinVal)
                {
                    pSliRam->TouchPos = (ushort) (obj.Panel.X + num2);
                }
                else
                {
                    num = (ushort) ((obj.Panel.X + num2) + ((((long) (pSliRam->NowVal - pSliRam->MinVal)) * (obj.Panel.EndX - obj.Panel.X - pSliRam->CursorWidth + 1)) / ((long) (pSliRam->MaxVal - pSliRam->MinVal + 1))));
                    pSliRam->TouchPos = num;
                }
            }
            if (!m_runScreen.IsEditor)
            {
                m_guiApp.PageObjects[ID].RefreshFlag = 1;
            }
            pSliRam->LastVal = pSliRam->NowVal;
            return 1;
        }
    }
}

