using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NextionEditor
{
	public class HmiPage
	{
		public HmiApplication App;
		public List<HmiObject> HmiObjects = new List<HmiObject>();
		public List<byte[]> Codes = new List<byte[]>();
		public string Name = "";
		public int PageId = 0xffff;

		public HmiPage(HmiApplication app)
		{
			App = app;
		}

		public bool Compile(RichTextBox textCompile)
		{
			byte index = 0;
			string[] pageTimers = new string[4];
			bool flag = true;

			List<byte[]> newlist = new List<byte[]>();
			List<byte[]> list2 = new List<byte[]>();

			byte[] binTFT = new byte[HmiOptions.InfoPageObjectSize * HmiObjects.Count + 4];
			byte[] ll = new byte[0];

			Codes.Clear();
			InfoPageObject infoPage;
			ushort customDataAddress = (ushort)(App.OverBytes.Length - 4);

			try
			{
				int i;
				Utility.ToBytes(((uint)customDataAddress)).CopyTo(binTFT, 0);
				customDataAddress += (ushort)(binTFT.Length - 4);
				for (int idxObj = 0; idxObj < HmiObjects.Count; idxObj++)
				{
					if (!objDetect(textCompile, HmiObjects[idxObj]))
					{
						flag = false;
						continue;
					}

					infoPage.TouchState = 1;
					if (HmiObjects[idxObj].ObjInfo.Panel.loadlei == 1)
						infoPage.Visible = 1;
					else
						infoPage.Visible = 0;

					infoPage.RefreshFlag = HmiObjects[idxObj].ObjInfo.Panel.loadlei;
					Utility.ToBytes(infoPage).CopyTo(binTFT, idxObj * HmiOptions.InfoPageObjectSize + 4);

					if (HmiObjects[idxObj].ObjInfo.IsCustomData == 0)
					{
						HmiObjects[idxObj].ObjInfo.AttributeStart = customDataAddress;
						ll = Utility.ToBytes((uint)customDataAddress);
						ushort objRamBytes = HmiObjects[idxObj].GetObjRamBytes(ref ll, customDataAddress);
						if (objRamBytes != 0)
						{
							list2.Add(ll);
							HmiObjects[idxObj].ObjInfo.AttributeLength = objRamBytes;
							customDataAddress += HmiObjects[idxObj].ObjInfo.AttributeLength;
						}
					}
				}

				if (customDataAddress > HmiOptions.MaxCustomDataSize)
				{
					textCompile.AddRichTextString(
						string.Concat(
							"Page:".Translate(),
							Name,
							" Error! Memory overflow:".Translate(), customDataAddress
							),
						Color.Red
						);
					App.Errors++;
					flag = false;
				}

				if (!flag)
					return flag;

				textCompile.AddRichTextString(
					string.Concat(
						"Page:".Translate(),
						Name,
						" OK! Occupy memory:".Translate(),
						customDataAddress), Color.Black);

				ll = Utility.MergeBytes(Utility.ToBytes((uint)0xffff), Utility.ToBytes((uint)customDataAddress));
				list2.Add(ll);

				Codes.Add(Utility.PatternBytes("cre"));
				Codes.Add(binTFT);
				Utility.AppendList(Codes, list2);
				Codes.Add(Utility.PatternBytes("end"));

				newlist.Clear();
				for (int idxObj = 0; idxObj < HmiObjects.Count; idxObj++)
					if (HmiObjects[idxObj].ObjInfo.ObjType == HmiObjType.OBJECT_TYPE_CURVE)
						newlist.Add(Utility.ToBytes("init " + idxObj.ToString()));
				Utility.AppendList(Codes, newlist);

				index = 0;
				newlist.Clear();
				for (int idxObj = 0; idxObj < HmiObjects.Count; idxObj++)
				{
					HmiObject obj = HmiObjects[idxObj];
					if (obj.ObjInfo.ObjType == HmiObjType.TIMER)
					{
						if (index > 3)
						{	// Maximum 4 timer on page
							if (index < 0xff)
							{	// One time message
								textCompile.AddRichTextString("Page:".Translate() + Name + "Error! Only 4 Timers are allowed.".Translate(), Color.Red);
								App.Errors++;
								flag = false;
								index = 0xff;
							}
						}
						else
						{
							newlist.Add(Utility.ToBytes("topen " + index.ToString() + "," + obj.ObjName + ".tim"));
							Utility.AppendList(newlist, obj.Codes[0]);
							newlist.Add(Utility.PatternBytes("tend"));

							obj.TimerRefCodes.Clear();
							obj.TimerRefCodes.Add(Utility.ToBytes("tpau " + index.ToString() + "," + obj.ObjName + ".tim," + obj.ObjName + ".en"));
							pageTimers[index] = obj.ObjName;
							++index;
						}
					}
				}
				Utility.DeleteComments(newlist);
				Utility.AppendList(Codes, newlist);

				newlist.Clear();
				for (int idxObj = 0; idxObj < HmiObjects.Count; idxObj++)
				{
					HmiObject obj = HmiObjects[idxObj];
					if (obj.ObjInfo.Panel.loadlei == 1
					 && obj.Codes[0].Count > 0
					 && obj.ObjInfo.ObjType != HmiObjType.TIMER)
					{
						for (i = 0; i < obj.Codes[0].Count; ++i)
							if (Utility.GetComType(obj.Codes[0][i]) == 0)
								newlist.Add(obj.Codes[0][i]);
					}
				}
				Utility.DeleteComments(newlist);
				Utility.AppendList(Codes, newlist);

				newlist.Clear();
				for (int idxObj = 0; idxObj < HmiObjects.Count; idxObj++)
				{
					if (HmiObjects[idxObj].ObjInfo.Panel.loadlei == 1)
					{
						newlist.Add(Utility.ToBytes("oref " + idxObj.ToString() + ",0"));
						newlist.Add(Utility.PatternBytes("if(sysda0==1)"));
						newlist.Add(Utility.PatternBytes("{"));

						HmiObjects[idxObj].GetLoadCodes(newlist, 0);

						newlist.Add(Utility.ToBytes("cle_f " + idxObj.ToString() + ",0"));
						newlist.Add(Utility.PatternBytes("}"));
					}
				}
				Utility.AppendList(Codes, newlist);

				newlist.Clear();
				for (int idxObj = 0; idxObj < HmiObjects.Count; idxObj++)
				{
					HmiObject obj = HmiObjects[idxObj];
					if (obj.ObjInfo.Panel.loadlei == 1
					 && obj.Codes[0].Count > 0
					 && obj.ObjInfo.ObjType != HmiObjType.TIMER
						)
					{
						for (i = 0; i < obj.Codes[0].Count; i++)
							if (Utility.GetComType(obj.Codes[0][i]) == 1)
								newlist.Add(obj.Codes[0][i]);
					}
				}
				Utility.DeleteComments(newlist);
				Utility.AppendList(Codes, newlist);

				Codes.Add(Utility.PatternBytes("end"));
				return flag;
			}
			catch (Exception ex)
			{
				textCompile.AddRichTextString("Exception: " + ex.Message, Color.Red);
			}
			return false;
		}

		public void DeleteAllObj()
		{
			while (HmiObjects.Count > 1)
				HmiObjects.RemoveAt(HmiObjects.Count - 1);

			if (HmiObjects.Count > 0 && HmiObjects[0].Attributes[0].Data[0] != HmiObjType.PAGE)
				HmiObjects.RemoveAt(0);
		}

		public void delobj(HmiObject obj)
		{
			HmiObjects.RemoveAt(obj.ObjId);
			App.RefreshObjId(this);
		}

		private bool objDetect(RichTextBox textCompile, HmiObject obj)
		{
			bool flag = true;

			if (obj.ObjInfo.Panel.X < 0
			 || obj.ObjInfo.Panel.EndX >= App.LcdWidth
			 || obj.ObjInfo.Panel.Y < 0
			 || obj.ObjInfo.Panel.EndY >= App.LcdHeight
				)
			{
				textCompile.AddRichTextString(
					string.Concat("Page:".Translate(), Name, " Error:".Translate(), obj.ObjName, " Position Invalid:".Translate()),
					Color.Red
					);
				App.Errors++;
				flag = false;
			}

			for (int i = 0; i < obj.Attributes.Count; i++)
			{
				if (obj.Attributes[i].InfoAttribute.AttrType < 15
				 && obj.Attributes[i].InfoAttribute.CanModify == 1
				 && obj.checkAttribute(obj.Attributes[i])
					)
				{
					string err = obj.CheckAttributeValue(obj.Attributes[i]);
					if (err != "")
					{
						textCompile.AddRichTextString(
							string.Concat(
								"Page:".Translate(),
								Name,
								" Error:".Translate(),
								obj.ObjName + "." + obj.Attributes[i].Name.ToString(),
								" InputVal Invalid:".Translate(),
								err),
							Color.Red);
						App.Errors++;
						flag = false;
					}
				}
			}
			return flag;
		}
	}
}
