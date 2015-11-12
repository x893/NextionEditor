using System;
using System.Collections.Generic;

namespace NextionEditor
{
	public static class ObjAttOperation
	{
		public static List<HmiAttribute> CreateAttrByType(byte attrType, ref InfoPanel panel)
		{
			List<HmiAttribute> attrs = new List<HmiAttribute>();

			if (attrType == HmiObjType.NUMBER)
			{
				panel.X = 0;
				panel.Y = 0;
				panel.EndX = 100;
				panel.EndY = 30;
				panel.loadlei = 1;

				attrs.AddNewAttribute("lei", 1, HmiAttributeType.State, IsYesNo.No, attrType.ToString(), "Number".Translate(), 0, 0, 0xff, 0);
				attrs.AddNewAttribute("vscope", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Variable scope ( Local variable is only visible in current page, while global variable is visible  in all pages):0-local;1-global".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("sta", 1, HmiAttributeType.Selection, IsYesNo.No, "1", "Background fill:0-crop image;1-solid color;2-Image".Translate(), 0, 0, 2, 0);
				attrs.AddNewAttribute("bco", 2, HmiAttributeType.Color, IsYesNo.No, "65535", "Background color".Translate() + "~sta=1", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("picc", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "Background crop image(must be full screen image)".Translate() + "~sta=0", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("pic", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "Background image".Translate() + "~sta=2", 1, 1, 0xffff, 0);
				attrs.AddNewAttribute("pco", 2, HmiAttributeType.Color, IsYesNo.No, "0", "Font Color".Translate(), 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("font", 1, HmiAttributeType.Font, IsYesNo.No, "0", "Font".Translate(), 0, 1, 0xff, 0);
				attrs.AddNewAttribute("xcen", 1, HmiAttributeType.Other, IsYesNo.No, "1", "Horizontal alignment: 0-Left,1-Center, 2-Right".Translate(), 0, 1, 2, 0);
				attrs.AddNewAttribute("ycen", 1, HmiAttributeType.Other, IsYesNo.No, "1", "Vertical alignment: 0-Up,1-Center, 2-Down".Translate(), 0, 1, 2, 0);
				attrs.AddNewAttribute("val", 4, HmiAttributeType.Other, IsYesNo.Yes, "0", "Number(min-0,max-4294967295)".Translate(), 0, 1, uint.MaxValue, 0);
				attrs.AddNewAttribute("lenth", 1, HmiAttributeType.Other, IsYesNo.No, "0", "Show lenth(0-auto,max-10)".Translate(), 0, 1, 10, 0);
				return attrs;
			}

			if (attrType == HmiObjType.BUTTON_T)
			{
				panel.X = 0;
				panel.Y = 0;
				panel.EndX = 50;
				panel.EndY = 50;
				panel.loadlei = 1;

				attrs.AddNewAttribute("lei", 1, HmiAttributeType.State, IsYesNo.No, attrType.ToString(), "Dual-state button".Translate(), 0, 0, 0xff, 0);
				attrs.AddNewAttribute("vscope", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Variable scope ( Local variable is only visible in current page, while global variable is visible  in all pages):0-local;1-global".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("sta", 1, HmiAttributeType.Selection, IsYesNo.No, "1", "Background fill:0-crop image;1-solid color;2-Image".Translate(), 0, 0, 2, 0);
				attrs.AddNewAttribute("picc0", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "State 0 crop background (Must be full screen image)".Translate() + "~sta=0", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("picc1", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "State 1 crop background (Must be full screen image)".Translate() + "~sta=0", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("bco0", 2, HmiAttributeType.Color, IsYesNo.No, "48631", "State 0 background color".Translate() + "~sta=1", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("bco1", 2, HmiAttributeType.Color, IsYesNo.No, "1024", "State 1 background color".Translate() + "~sta=1", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("pic0", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "State 0 background image".Translate() + "~sta=2", 1, 1, 0xffff, 0);
				attrs.AddNewAttribute("pic1", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "State 1 background image".Translate() + "~sta=2", 1, 1, 0xffff, 0);
				attrs.AddNewAttribute("val", 1, HmiAttributeType.Other, IsYesNo.Yes, "0", "Current Status (0 or 1)".Translate(), 0, 1, 1, 0);
				return attrs;
			}
			if (attrType == HmiObjType.VAR)
			{
				panel.X = 0;
				panel.Y = 0;
				panel.EndX = 0;
				panel.EndY = 0;
				panel.loadlei = 1;
				attrs.AddNewAttribute("lei", 1, HmiAttributeType.State, IsYesNo.No, attrType.ToString(), "Variable".Translate(), 0, 0, 0xff, 0);
				attrs.AddNewAttribute("vscope", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Variable scope ( Local variable is only visible in current page, while global variable is visible  in all pages):0-local;1-global".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("sta", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Variable types:0-Number;1-String".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("val", 4, HmiAttributeType.Other, IsYesNo.No, "0", "Initial value ( 0 to 4294967295 )".Translate() + "~sta=0", 0, 1, uint.MaxValue, 0);
				attrs.AddNewAttribute("txt", 30, HmiAttributeType.String, IsYesNo.No, "newtxt", "Initial value".Translate() + "~sta=1", 0, 1, 0xff, 0);
				attrs.AddNewAttribute("txt-maxl", 1, HmiAttributeType.StrLength, IsYesNo.No, "30", "MaxLenth".Translate() + "~sta=1", 0, 0, 0xff, 0);
				return attrs;
			}
			if (attrType == HmiObjType.TIMER)
			{
				panel.X = 0;
				panel.Y = 0;
				panel.EndX = 0;
				panel.EndY = 0;
				panel.loadlei = 1;
				attrs.AddNewAttribute("lei", 1, HmiAttributeType.State, IsYesNo.No, attrType.ToString(), "Timer".Translate(), 0, 0, 0xff, 0);
				attrs.AddNewAttribute("vscope", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Variable scope ( Local variable is only visible in current page, while global variable is visible  in all pages):0-local;1-global".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("tim", 2, HmiAttributeType.Other, IsYesNo.Yes, "400", "Set time in ms. ( 50 to 65535)".Translate(), 0, 1, 0xffff, 50);
				attrs.AddNewAttribute("en", 1, HmiAttributeType.Other, IsYesNo.Yes, "1", "Trigger: (0 - Disable, 1 - Enable)".Translate(), 0, 1, 1, 0);
				return attrs;
			}
			if (attrType == HmiObjType.OBJECT_TYPE_CURVE)
			{
				panel.X = 0;
				panel.Y = 0;
				panel.EndX = 200;
				panel.EndY = 200;
				panel.loadlei = 1;
				attrs.AddNewAttribute("lei", 1, HmiAttributeType.State, IsYesNo.No, attrType.ToString(), "Waveform".Translate(), 0, 0, 0xff, 0);
				attrs.AddNewAttribute("vscope", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Variable scope ( Local variable is only visible in current page, while global variable is visible  in all pages):0-local;1-global".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("sta", 1, HmiAttributeType.Selection, IsYesNo.No, "1", "Background fill:0-crop image;1-solid color;2-Image".Translate(), 0, 0, 2, 0);
				attrs.AddNewAttribute("ch", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Numbers of Channels (Min 1, Max 4):0-1;1-2;2-3;3-4".Translate(), 0, 0, 3, 0);
				attrs.AddNewAttribute("bco", 2, HmiAttributeType.Color, IsYesNo.No, "0", "Background color".Translate() + "~sta=1".Translate(), 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("picc", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "Background crop image(must be full screen image)".Translate() + "~sta=0", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("pic", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "Background image".Translate() + "~sta=2", 1, 1, 0xffff, 0);
				attrs.AddNewAttribute("gdc", 2, HmiAttributeType.Color, IsYesNo.No, "1024", "Grid Color".Translate() + "~sta=1", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("gdw", 1, HmiAttributeType.Other, IsYesNo.No, "40", "Grid Width (0 = None)".Translate() + "~sta=1", 0, 1, 0xff, 0);
				attrs.AddNewAttribute("gdh", 1, HmiAttributeType.Other, IsYesNo.No, "40", "Grid Height 0 = None)".Translate() + "~sta=1", 0, 1, 0xff, 0);
				attrs.AddNewAttribute("pco0", 2, HmiAttributeType.Color, IsYesNo.No, "64495", "Channel 0 foreground color".Translate() + "~ch>0", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("pco1", 2, HmiAttributeType.Color, IsYesNo.No, "65519", "Channel 1 foreground color".Translate() + "~ch>1", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("pco2", 2, HmiAttributeType.Color, IsYesNo.No, "63488", "Channel 2 foreground color".Translate() + "~ch>2", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("pco3", 2, HmiAttributeType.Color, IsYesNo.No, "2016", "Channel 3 foreground color".Translate() + "~ch>3", 0, 1, 0xffff, 0);
				return attrs;
			}
			if (attrType == HmiObjType.OBJECT_TYPE_SLIDER)
			{
				panel.X = 0;
				panel.Y = 0;
				panel.EndX = 0xc7;
				panel.EndY = 0x18;
				panel.loadlei = 1;
				attrs.AddNewAttribute("lei", 1, HmiAttributeType.State, IsYesNo.No, attrType.ToString(), "Slider".Translate(), 0, 0, 0xff, 0);
				attrs.AddNewAttribute("vscope", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Variable scope ( Local variable is only visible in current page, while global variable is visible  in all pages):0-local;1-global".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("mode", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Direction:0-horizontal;1-Vertical".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("sta", 1, HmiAttributeType.Selection, IsYesNo.No, "1", "Background fill:0-crop image;1-solid color;2-Image".Translate(), 0, 0, 2, 0);
				attrs.AddNewAttribute("psta", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Cursor fill:0-solid;1-Image".Translate(), 0, 0, 2, 0);
				attrs.AddNewAttribute("bco", 2, HmiAttributeType.Color, IsYesNo.No, "1024", "Background color".Translate() + "~sta=1".Translate(), 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("pic", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "Background image".Translate() + "~sta=2", 1, 1, 0xffff, 0);
				attrs.AddNewAttribute("picc", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "Background crop image(must be full screen image)".Translate() + "~sta=0", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("pco", 2, HmiAttributeType.Color, IsYesNo.No, "63488", "Cursor Color".Translate() + "~psta=0".Translate(), 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("pic2", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "Cursor Image".Translate() + "~psta=1", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("wid", 1, HmiAttributeType.Other, IsYesNo.No, "25", "Cursor Width".Translate(), 0, 1, 200, 0);
				attrs.AddNewAttribute("hig", 1, HmiAttributeType.Other, IsYesNo.No, "25", "Cursor Height".Translate(), 0, 1, 0x19, 0);
				attrs.AddNewAttribute("val", 2, HmiAttributeType.Other, IsYesNo.Yes, "50", "CuVal".Translate(), 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("maxval", 2, HmiAttributeType.Other, IsYesNo.No, "100", "MaxVal".Translate(), 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("minval", 2, HmiAttributeType.Other, IsYesNo.No, "0", "MinVal".Translate(), 0, 1, 0xffff, 0);
				return attrs;
			}
			if (attrType == HmiObjType.TEXT)
			{
				panel.X = 0;
				panel.Y = 0;
				panel.EndX = 100;
				panel.EndY = 30;
				panel.loadlei = 1;
				attrs.AddNewAttribute("lei", 1, HmiAttributeType.State, IsYesNo.No, attrType.ToString(), "Text".Translate(), 0, 0, 0xff, 0);
				attrs.AddNewAttribute("vscope", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Variable scope ( Local variable is only visible in current page, while global variable is visible  in all pages):0-local;1-global".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("sta", 1, HmiAttributeType.Selection, IsYesNo.No, "1", "Background fill:0-crop image;1-solid color;2-Image".Translate(), 0, 0, 2, 0);
				attrs.AddNewAttribute("bco", 2, HmiAttributeType.Color, IsYesNo.No, "65535", "Background color".Translate() + "~sta=1", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("picc", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "Background crop image(must be full screen image)".Translate() + "~sta=0", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("pic", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "Background image".Translate() + "~sta=2", 1, 1, 0xffff, 0);
				attrs.AddNewAttribute("pco", 2, HmiAttributeType.Color, IsYesNo.No, "0", "Font Color".Translate(), 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("font", 1, HmiAttributeType.Font, IsYesNo.No, "0", "Font".Translate(), 0, 1, 0xff, 0);
				attrs.AddNewAttribute("xcen", 1, HmiAttributeType.Other, IsYesNo.No, "1", "Horizontal alignment: 0-Left,1-Center, 2-Right".Translate(), 0, 1, 2, 0);
				attrs.AddNewAttribute("ycen", 1, HmiAttributeType.Other, IsYesNo.No, "1", "Vertical alignment: 0-Up,1-Center, 2-Down".Translate(), 0, 1, 2, 0);
				attrs.AddNewAttribute("txt", 30, HmiAttributeType.String, IsYesNo.Yes, "newtxt", "Content".Translate(), 0, 1, 0xff, 0);
				attrs.AddNewAttribute("txt-maxl", 1, HmiAttributeType.StrLength, 0, "30", "MaxLength".Translate(), 0, 0, 0xff, 0);
				return attrs;
			}
			if (attrType == HmiObjType.BUTTON)
			{
				panel.X = 0;
				panel.Y = 0;
				panel.EndX = 100;
				panel.EndY = 30;
				panel.loadlei = 1;
				attrs.AddNewAttribute("lei", 1, HmiAttributeType.State, IsYesNo.No, attrType.ToString(), "Button".Translate(), 0, 0, 0xff, 0);
				attrs.AddNewAttribute("vscope", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Variable scope ( Local variable is only visible in current page, while global variable is visible  in all pages):0-local;1-global".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("sta", 1, HmiAttributeType.Selection, IsYesNo.No, "1", "Background fill:0-crop image;1-solid color;2-Image".Translate(), 0, 0, 2, 0);
				attrs.AddNewAttribute("picc", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "Defult crop image background(must be full screen image)".Translate() + "~sta=0", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("picc2", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "Press crop image background(must be full screen image)".Translate() + "~sta=0", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("bco", 2, HmiAttributeType.Color, IsYesNo.No, "48631", "Default background color".Translate() + "~sta=1", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("bco2", 2, HmiAttributeType.Color, IsYesNo.No, "1024", "Press Eevent background color".Translate() + "~sta=1", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("pic", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "Default background image".Translate() + "~sta=2", 1, 1, 0xffff, 0);
				attrs.AddNewAttribute("pic2", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "Press Event background Image".Translate() + "~sta=2", 1, 1, 0xffff, 0);
				attrs.AddNewAttribute("pco", 2, HmiAttributeType.Color, IsYesNo.No, "0", "Default Font color".Translate(), 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("pco2", 2, HmiAttributeType.Color, IsYesNo.No, "0", "Press Eevent font color".Translate(), 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("font", 1, HmiAttributeType.Font, IsYesNo.No, "0", "Font".Translate(), 0, 1, 0xff, 0);
				attrs.AddNewAttribute("xcen", 1, HmiAttributeType.Other, IsYesNo.No, "1", "Horizontal alignment: 0-Left,1-Center, 2-Right".Translate(), 0, 1, 2, 0);
				attrs.AddNewAttribute("ycen", 1, HmiAttributeType.Other, IsYesNo.No, "1", "Vertical alignment: 0-Up,1-Center, 2-Down".Translate(), 0, 1, 2, 0);
				attrs.AddNewAttribute("txt", 30, HmiAttributeType.String, IsYesNo.Yes, "newtxt", "Content".Translate(), 0, 1, 0xff, 0);
				attrs.AddNewAttribute("txt-maxl", 1, HmiAttributeType.StrLength, 0, "30", "MaxLength".Translate(), 0, 0, 0xff, 0);
				return attrs;
			}
			if (attrType == HmiObjType.PROG)
			{
				panel.X = 0;
				panel.Y = 0;
				panel.EndX = 100;
				panel.EndY = 30;
				panel.loadlei = 1;
				attrs.AddNewAttribute("lei", 1, HmiAttributeType.State, IsYesNo.No, attrType.ToString(), "Progress bar".Translate(), 0, 0, 0xff, 0);
				attrs.AddNewAttribute("vscope", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Variable scope ( Local variable is only visible in current page, while global variable is visible  in all pages):0-local;1-global".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("sta", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Cursor fill:0-solid;1-Image".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("dez", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Progress bar direction: 0-horizontal;1-vertical".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("bco", 2, HmiAttributeType.Color, IsYesNo.No, "48631", "Background color".Translate() + "~sta=0", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("pco", 2, HmiAttributeType.Color, IsYesNo.No, "1024", "Foreground color".Translate() + "~sta=0", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("bpic", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "Background image".Translate() + "~sta=1", 1, 1, 0xffff, 0);
				attrs.AddNewAttribute("ppic", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "Foreground Image".Translate() + "~sta=1", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("val", 1, HmiAttributeType.Other, IsYesNo.Yes, "50", "Progress value".Translate(), 0, 1, 100, 0);
				return attrs;
			}
			if (attrType == HmiObjType.PICTURE)
			{
				panel.X = 0;
				panel.Y = 0;
				panel.EndX = 100;
				panel.EndY = 30;
				panel.loadlei = 1;
				attrs.AddNewAttribute("lei", 1, HmiAttributeType.State, IsYesNo.No, attrType.ToString(), "Picture".Translate(), 0, 0, 0xff, 0);
				attrs.AddNewAttribute("vscope", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Variable scope ( Local variable is only visible in current page, while global variable is visible  in all pages):0-local;1-global".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("pic", 2, HmiAttributeType.PicId, IsYesNo.Yes, "65535", "Picture".Translate(), 1, 1, 0xffff, 0);
				return attrs;
			}
			if (attrType == HmiObjType.PICTUREC)
			{
				panel.X = 0;
				panel.Y = 0;
				panel.EndX = 100;
				panel.EndY = 30;
				panel.loadlei = 1;
				attrs.AddNewAttribute("lei", 1, HmiAttributeType.State, IsYesNo.No, attrType.ToString(), "Crop Image".Translate(), 0, 0, 0xff, 0);
				attrs.AddNewAttribute("vscope", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Variable scope ( Local variable is only visible in current page, while global variable is visible  in all pages):0-local;1-global".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("picc", 2, HmiAttributeType.PicId, IsYesNo.Yes, "65535", "Crop Image (must crop full screen image)".Translate(), 0, 1, 0xffff, 0);
				return attrs;
			}
			if (attrType == HmiObjType.TOUCH)
			{
				panel.X = 0;
				panel.Y = 0;
				panel.EndX = 100;
				panel.EndY = 30;
				panel.loadlei = 1;
				attrs.AddNewAttribute("lei", 1, HmiAttributeType.State, IsYesNo.No, attrType.ToString(), "Touch Area".Translate(), 0, 0, 0xff, 0);
				attrs.AddNewAttribute("vscope", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Variable scope ( Local variable is only visible in current page, while global variable is visible  in all pages):0-local;1-global".Translate(), 0, 0, 1, 0);
				return attrs;
			}
			if (attrType == HmiObjType.POINTER)
			{
				panel.X = 0;
				panel.Y = 0;
				panel.EndX = 100;
				panel.EndY = 100;
				panel.loadlei = 1;
				attrs.AddNewAttribute("lei", 1, HmiAttributeType.State, IsYesNo.No, attrType.ToString(), "Gauges".Translate(), 0, 0, 0xff, 0);
				attrs.AddNewAttribute("vscope", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Variable scope ( Local variable is only visible in current page, while global variable is visible  in all pages):0-local;1-global".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("sta", 1, HmiAttributeType.Selection, IsYesNo.No, "1", "Background fill content:0-crop image;1-solid color".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("picc", 2, HmiAttributeType.PicId, IsYesNo.No, "65535", "Background crop image (must be full screen image)".Translate() + "~sta=0", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("bco", 2, HmiAttributeType.Color, IsYesNo.No, "65535", "Background color".Translate() + "~sta=1", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("val", 2, HmiAttributeType.Other, IsYesNo.Yes, "0", "Angle,0-360".Translate(), 0, 1, 360, 0);
				attrs.AddNewAttribute("wid", 1, HmiAttributeType.Other, IsYesNo.No, "2", "The thickness of Pointer,maximum:5".Translate(), 0, 1, 5, 0);
				attrs.AddNewAttribute("pco", 2, HmiAttributeType.Color, IsYesNo.No, "1024", "Pointer color".Translate(), 0, 1, 0xffff, 0);
				return attrs;
			}
			if (attrType == HmiObjType.PAGE)
			{
				attrs.AddNewAttribute("lei", 1, HmiAttributeType.State, IsYesNo.No, attrType.ToString(), "Page".Translate(), 0, 0, 0xff, 0);
				attrs.AddNewAttribute("vscope", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Variable scope ( Local variable is only visible in current page, while global variable is visible  in all pages):0-local;1-global".Translate(), 0, 0, 1, 0);
				attrs.AddNewAttribute("sta", 1, HmiAttributeType.Selection, IsYesNo.No, "1", "Background fill content:0-no background;1-solid color;2-Image".Translate(), 0, 0, 2, 0);
				attrs.AddNewAttribute("bco", 2, HmiAttributeType.Color, IsYesNo.Yes, "65535", "Background color".Translate() + "~sta=1", 0, 1, 0xffff, 0);
				attrs.AddNewAttribute("pic", 2, HmiAttributeType.PicId, IsYesNo.Yes, "65535", "Background Image (must be full screen image)".Translate() + "~sta=2", 0, 1, 0xffff, 0);
			}
			return attrs;
		}
	}
}
