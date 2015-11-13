using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NextionEditor
{
	public class HmiObject
	{
		public HmiApplication App;
		public HmiPage Page;
		public List<HmiAttribute> Attributes = new List<HmiAttribute>();
		public List<byte[]>[] Codes = new List<byte[]>[4];
		public List<byte[]> TimerRefCodes = new List<byte[]>();
		public byte IsBinding = 0;
		public InfoObject ObjInfo;
		public string ObjName = "";
		public int ObjId;

		public HmiObject(HmiApplication app, HmiPage page)
		{
			App = app;
			Page = page;
			Codes[0] = new List<byte[]>();
			Codes[1] = new List<byte[]>();
			Codes[2] = new List<byte[]>();
			Codes[3] = new List<byte[]>();
		}

		public void BindingPicXY(int picid)
		{
			if (picid < App.Pictures.Count)
			{
				ObjInfo.Panel.EndX = (ushort)((ObjInfo.Panel.X + App.Pictures[picid].W) - 1);
				ObjInfo.Panel.EndY = (ushort)((ObjInfo.Panel.Y + App.Pictures[picid].H) - 1);
			}
		}

		public void CompileCodes(List<byte[]> bts)
		{
			byte[] buffer;
			InfoAttribute infoAttr;

			bts.Add(Utility.PatternBytes("att"));
			foreach (HmiAttribute attr in Attributes)
				if (checkAttribute(attr))
				{
					infoAttr = attr.InfoAttribute;
					if (infoAttr.IsReturn == 1)
						infoAttr.IsReturn = (byte)ObjId;
					else
						infoAttr.IsReturn = 0xff;

					buffer = Utility.MergeBytes(attr.Name, Utility.ToBytes(infoAttr), attr.Data);
					bts.Add(buffer);
				}

			infoAttr.CanModify = 0;
			infoAttr.IsReturn = 0xff;
			infoAttr.Length = 2;
			infoAttr.DataLength = 2;
			infoAttr.AttrType = HmiAttributeType.Other;
			infoAttr.DataStart = 0xff;
			infoAttr.MaxValue = 0;
			infoAttr.MinValue = 0;
			infoAttr.Start = 0;
			infoAttr.IsBinding = 0;

			if (Attributes[0].Data[0] != HmiObjType.TIMER && Attributes[0].Data[0] != HmiObjType.VAR)
			{
				bts.Add(
					Utility.MergeBytes(
						Utility.ToBytes("x", 8),
						Utility.ToBytes(infoAttr),
						Utility.ToBytes(ObjInfo.Panel.X)
					)
				);
				bts.Add(Utility.MergeBytes(Utility.ToBytes("y", 8), Utility.ToBytes(infoAttr), Utility.ToBytes(ObjInfo.Panel.Y)));
				bts.Add(Utility.MergeBytes(Utility.ToBytes("endx", 8), Utility.ToBytes(infoAttr), Utility.ToBytes(ObjInfo.Panel.EndX)));
				bts.Add(Utility.MergeBytes(Utility.ToBytes("endy", 8), Utility.ToBytes(infoAttr), Utility.ToBytes(ObjInfo.Panel.EndY)));
				bts.Add(Utility.MergeBytes(Utility.ToBytes("w", 8), Utility.ToBytes(infoAttr), Utility.ToBytes((int)(ObjInfo.Panel.EndX - ObjInfo.Panel.X + 1))));
				bts.Add(Utility.MergeBytes(Utility.ToBytes("h", 8), Utility.ToBytes(infoAttr), Utility.ToBytes((int)(ObjInfo.Panel.EndY - ObjInfo.Panel.Y + 1))));
			}
			bts.Add(Utility.PatternBytes("end"));

			bts.Add(Utility.PatternBytes("ref"));
			int refCodes = getRefCodes(bts);
			ObjInfo.Panel.Ref = (ushort)(refCodes == 0 ? 0xffff : bts.Count - refCodes);
			bts.Add(Utility.PatternBytes("end"));

			if (Attributes[0].Data[0] != HmiObjType.TIMER && Attributes[0].Data[0] != HmiObjType.VAR)
			{
				bts.Add(Utility.PatternBytes("load"));
				refCodes = GetLoadCodes(bts, 0);
				ObjInfo.Panel.Load = (ushort)(refCodes == 0 ? 0xffff : bts.Count - refCodes);
				bts.Add(Utility.PatternBytes("end"));

				bts.Add(Utility.PatternBytes("down"));
				refCodes = getDownCodes(bts);
				ObjInfo.Panel.Down = (ushort)(refCodes == 0 ? 0xffff : bts.Count - refCodes);
				bts.Add(Utility.PatternBytes("end"));

				bts.Add(Utility.PatternBytes("up"));
				refCodes = GetUpCodes(bts);
				ObjInfo.Panel.Up = (byte)(refCodes == 0 ? 0xff : bts.Count - refCodes);
				bts.Add(Utility.PatternBytes("end"));

				bts.Add(Utility.PatternBytes("slide"));
				refCodes = GetSlideCodes(bts);
				ObjInfo.Panel.Slide = (byte)(refCodes == 0 ? 0xff : bts.Count - refCodes);
				bts.Add(Utility.PatternBytes("end"));
			}
		}

		private void replaceToValues(List<byte[]> codesInf, int variable)
		{
			string codes = "";
			for (int i = 0; i < codesInf.Count; i++)
			{
				int width = ObjInfo.Panel.EndX - ObjInfo.Panel.X + 1;
				int height = ObjInfo.Panel.EndY - ObjInfo.Panel.Y + 1;

				codes = Utility.GetString(codesInf[i])
								.Replace("'&objid&'", ObjId.ToString())
								.Replace("'&pagename&'", Page.Name)
								.Replace("'&objname&'", ObjName)
								.Replace("'&endx&'", ObjInfo.Panel.EndX.ToString())
								.Replace("'&endy&'", ObjInfo.Panel.EndY.ToString())
								.Replace("'&x&'", ObjInfo.Panel.X.ToString())
								.Replace("'&y&'", ObjInfo.Panel.Y.ToString())
								.Replace("'&w&'", width.ToString())
								.Replace("'&h&'", height.ToString()
							);

				byte[] attrVal = GetAttributeValue("sta");
				if (attrVal != null)
					codes = codes.Replace("'&sta&'", attrVal[0].ToString());

				for (int j = 1; j < Attributes.Count; j++)
				{
					HmiAttribute attr = Attributes[j];
					string token = "'&" + Utility.GetString(attr.Name) + "&'";

					if (codes.Contains(token))
					{
						if (variable > 0)
						{
							if (attr.InfoAttribute.AttrType < HmiAttributeType.String)
							{
								if (attr.InfoAttribute.Length == 1)
									codes = codes.Replace(token, attr.Data[0].ToString());
								else if (attr.InfoAttribute.Length == 2)
									codes = codes.Replace(token, attr.Data.ToU16().ToString());
								else if (attr.InfoAttribute.Length == 4)
									codes = codes.Replace(token, attr.Data.ToU32().ToString());
							}
							else if (variable == 2)
								codes = codes
										.Replace(token,
											"\""
											+ Utility.GetString(attr.Data).Replace(@"\", @"\\")
																.Replace("\"", "\\\"")
																.Replace("\r\n", @"\r")
											+ "\"");
							else
								codes = codes.Replace(token, ObjName + "." + Utility.GetString(attr.Name));
						}
						else if (attr.InfoAttribute.AttrType < 10)
							codes = codes.Replace(token, ObjName + "." + Utility.GetString(attr.Name));
						else
							codes = codes.Replace(token, ObjName + "." + Utility.GetString(attr.Name));
					}
				}
				codesInf[i] = codes.ToBytes();
			}
		}

		public void ChangeXY()
		{
			Range bufPos = new Range
			{
				Begin = 0,
				End = 7
			};
			if (Attributes[0].Data[0] == HmiObjType.OBJECT_TYPE_SLIDER)
			{
				for (int i = 0; i < Attributes.Count; i++)
				{
					if (Utility.IndexOf(Attributes[i].Name, "wid".ToBytes(), bufPos, 0) != 0xffff)
					{
						Attributes[i].InfoAttribute.MaxValue = (ushort)((ObjInfo.Panel.EndX - ObjInfo.Panel.X) + 1);
						if (Attributes[i].Data[0] > Attributes[i].InfoAttribute.MaxValue)
							SetAttrValue("wid", Attributes[i].InfoAttribute.MaxValue.ToString());
					}
					if (Utility.IndexOf(Attributes[i].Name, "hig".ToBytes(), bufPos, 0) != 0xffff)
					{
						Attributes[i].InfoAttribute.MaxValue = (ushort)((ObjInfo.Panel.EndY - ObjInfo.Panel.Y) + 1);
						if (Attributes[i].Data[0] > Attributes[i].InfoAttribute.MaxValue)
							SetAttrValue("hig", Attributes[i].InfoAttribute.MaxValue.ToString());
					}
				}
			}
		}

		public bool checkAttribute(HmiAttribute hmiAttr)
		{
			string note = Utility.GetString(hmiAttr.Note);
			if (note.Contains("~"))
			{
				byte[] attrVal;
				string[] attrs = note.Split(Utility.CHAR_TILDA);
				if (attrs.Length != 2)
				{
					MessageBox.Show("Component attribute error:0".Translate());
					return false;
				}

				string[] nameValue = attrs[1].Split(Utility.CHAR_EQUAL);
				if (nameValue.Length != 2)
				{
					nameValue = attrs[1].Split(Utility.CHAR_GREAT);
					if (nameValue.Length != 2)
					{
						MessageBox.Show("Component attribute error:1".Translate());
						return false;
					}
					attrVal = GetAttributeValue(nameValue[0]);
					if (attrVal != null
					 && attrVal.Length > 0
					 && attrVal[0] < byte.Parse(nameValue[1])
						)
						return false;
				}
				else
				{
					attrVal = GetAttributeValue(nameValue[0]);
					if (attrVal != null
					 && attrVal.Length > 0
					 && attrVal[0].ToString() != nameValue[1]
						)
						return false;
				}
			}

			if (hmiAttr.InfoAttribute.IsBinding == 1)
				IsBinding = 1;
			return true;
		}

		public string CheckAttributeValue(HmiAttribute hmiAttr)
		{
			try
			{
				if (hmiAttr.InfoAttribute.AttrType == HmiAttributeType.PicId)
				{
					int num = hmiAttr.Data.ToU16();
					if (num >= App.Pictures.Count)
						return "Invalid Pic ID".Translate();
				}

				if (Utility.GetString(hmiAttr.Name) == "font"
				 && GetAttributeValue("txt") != null
				 && Utility.GetString(GetAttributeValue("txt")) != ""
				 && hmiAttr.Data[0] >= App.Fonts.Count
					)
					return "Invalid Font ID".Translate();
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
			return "";
		}

		public bool DeleteAttribute(string name)
		{
			Range range = new Range
			{
				Begin = 0,
				End = 7
			};
			for (int i = 0; i < Attributes.Count; i++)
			{
				int idx = Utility.IndexOf(Attributes[i].Name, name, range);
				if (idx != 0xffff && (idx == 7 || Attributes[i].Name[idx + 1] == 0))
				{
					Attributes.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		public int GetAttributeCodes(List<byte[]> mycodes, byte index)
		{
			List<List<byte[]>> codesInf = new List<List<byte[]>>(5);
			codesInf.Add(new List<byte[]>(10));
			codesInf.Add(new List<byte[]>(10));
			codesInf.Add(new List<byte[]>(10));
			codesInf.Add(new List<byte[]>(10));
			codesInf.Add(new List<byte[]>(10));

			ObjInfo.ObjType = Attributes[0].Data[0];
			if (index > 4)
				return 0;

			if (Attributes.Count > 0 && Attributes[0].Data.Length == 1)
			{
				byte[] attrVal;

				#region NUMBER
				if (Attributes[0].Data[0] == HmiObjType.NUMBER)
				{
					attrVal = GetAttributeValue("sta");
					if (attrVal != null)
					{
						if (attrVal.Length == 1 && attrVal[0] == 0)
						{
							codesInf[0].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&picc&','&xcen&','&ycen&',0,'&val&','&lenth&'".ToBytes());

							if (Page.HmiObjects[0].GetAttributeValue("sta")[0] == 2
							 && Page.HmiObjects[0].GetAttributeValue("pic").ToU16() == GetAttributeValue("picc").ToU16()
								)
								codesInf[1].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&picc&','&xcen&','&ycen&',3,'&val&','&lenth&'".ToBytes());
							else
								codesInf[1].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&picc&','&xcen&','&ycen&',0,'&val&','&lenth&'".ToBytes());

							codesInf[4].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&picc&','&xcen&','&ycen&',0,'&val&','&lenth&'".ToBytes());
						}
						else if (attrVal.Length == 1 && attrVal[0] == 1)
						{
							codesInf[0].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&bco&','&xcen&','&ycen&',1,'&val&','&lenth&'".ToBytes());
							codesInf[1].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&bco&','&xcen&','&ycen&',1,'&val&','&lenth&'".ToBytes());
							codesInf[4].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&bco&','&xcen&','&ycen&',1,'&val&','&lenth&'".ToBytes());
						}
						else if (attrVal.Length == 1 && attrVal[0] == 2)
						{
							codesInf[0].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&pic&','&xcen&','&ycen&',2,'&val&','&lenth&'".ToBytes());
							codesInf[1].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&pic&','&xcen&','&ycen&',2,'&val&','&lenth&'".ToBytes());
							codesInf[4].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&pic&','&xcen&','&ycen&',2,'&val&','&lenth&'".ToBytes());
						}
					}
				}
				#endregion

				#region BUTTON_T
				else if (Attributes[0].Data[0] == HmiObjType.BUTTON_T)
				{
					attrVal = GetAttributeValue("sta");
					if (attrVal != null)
					{
						if (attrVal.Length == 1 && attrVal[0] == 0)
						{
							codesInf[0].Add("sysda0='&picc0&'".ToBytes());
							codesInf[0].Add("if('&val&'==1)".ToBytes());
							codesInf[0].Add("{".ToBytes());
							codesInf[0].Add("sysda0='&picc1&'".ToBytes());
							codesInf[0].Add("}".ToBytes());
							codesInf[0].Add("xpic '&x&','&y&','&w&','&h&','&x&','&y&',sysda0".ToBytes());
							codesInf[1].Add("sysda0='&picc0&'".ToBytes());
							codesInf[1].Add("if('&val&'==1)".ToBytes());
							codesInf[1].Add("{".ToBytes());
							codesInf[1].Add("sysda0='&picc1&'".ToBytes());
							codesInf[1].Add("}".ToBytes());
							codesInf[1].Add("xpic '&x&','&y&','&w&','&h&','&x&','&y&',sysda0".ToBytes());
							codesInf[2].Add("if('&val&'==1)".ToBytes());
							codesInf[2].Add("{".ToBytes());
							codesInf[2].Add("sysda0=0".ToBytes());
							codesInf[2].Add("}".ToBytes());
							codesInf[2].Add("if('&val&'==0)".ToBytes());
							codesInf[2].Add("{".ToBytes());
							codesInf[2].Add("sysda0=1".ToBytes());
							codesInf[2].Add("}".ToBytes());
							codesInf[2].Add("'&val&'=sysda0".ToBytes());
							codesInf[4].Add("xpic '&x&','&y&','&w&','&h&','&x&','&y&','&picc0&'".ToBytes());
						}
						else if (attrVal.Length == 1 && attrVal[0] == 1)
						{
							codesInf[0].Add("sysda0='&bco0&'".ToBytes());
							codesInf[0].Add("if('&val&'==1)".ToBytes());
							codesInf[0].Add("{".ToBytes());
							codesInf[0].Add("sysda0='&bco1&'".ToBytes());
							codesInf[0].Add("}".ToBytes());
							codesInf[0].Add("fill '&x&','&y&','&w&','&h&',sysda0".ToBytes());
							codesInf[0].Add("draw3d '&x&','&y&','&endx&','&endy&',61341,6339".ToBytes());
							codesInf[1].Add("sysda0='&bco0&'".ToBytes());
							codesInf[1].Add("if('&val&'==1)".ToBytes());
							codesInf[1].Add("{".ToBytes());
							codesInf[1].Add("sysda0='&bco1&'".ToBytes());
							codesInf[1].Add("}".ToBytes());
							codesInf[1].Add("fill '&x&','&y&','&w&','&h&',sysda0".ToBytes());
							codesInf[1].Add("draw3d '&x&','&y&','&endx&','&endy&',61341,6339".ToBytes());
							codesInf[2].Add("if('&val&'==1)".ToBytes());
							codesInf[2].Add("{".ToBytes());
							codesInf[2].Add("sysda0=0".ToBytes());
							codesInf[2].Add("}".ToBytes());
							codesInf[2].Add("if('&val&'==0)".ToBytes());
							codesInf[2].Add("{".ToBytes());
							codesInf[2].Add("sysda0=1".ToBytes());
							codesInf[2].Add("}".ToBytes());
							codesInf[2].Add("'&val&'=sysda0".ToBytes());
							codesInf[4].Add("fill '&x&','&y&','&w&','&h&','&bco0&'".ToBytes());
						}
						else if (attrVal.Length == 1 && attrVal[0] == 2)
						{
							codesInf[0].Add("sysda0='&pic0&'".ToBytes());
							codesInf[0].Add("if('&val&'==1)".ToBytes());
							codesInf[0].Add("{".ToBytes());
							codesInf[0].Add("sysda0='&pic1&'".ToBytes());
							codesInf[0].Add("}".ToBytes());
							codesInf[0].Add("pic '&x&','&y&',sysda0".ToBytes());
							codesInf[1].Add("sysda0='&pic0&'".ToBytes());
							codesInf[1].Add("if('&val&'==1)".ToBytes());
							codesInf[1].Add("{".ToBytes());
							codesInf[1].Add("sysda0='&pic1&'".ToBytes());
							codesInf[1].Add("}".ToBytes());
							codesInf[1].Add("pic '&x&','&y&',sysda0".ToBytes());
							codesInf[2].Add("if('&val&'==1)".ToBytes());
							codesInf[2].Add("{".ToBytes());
							codesInf[2].Add("sysda0=0".ToBytes());
							codesInf[2].Add("}".ToBytes());
							codesInf[2].Add("if('&val&'==0)".ToBytes());
							codesInf[2].Add("{".ToBytes());
							codesInf[2].Add("sysda0=1".ToBytes());
							codesInf[2].Add("}".ToBytes());
							codesInf[2].Add("'&val&'=sysda0".ToBytes());
							codesInf[4].Add("pic '&x&','&y&','&pic0&'".ToBytes());
						}
					}
				}
				#endregion

				#region TIMER
				else if (Attributes[0].Data[0] == HmiObjType.TIMER)
				{
					Utility.AddList(codesInf[0], TimerRefCodes);
					Utility.AddList(codesInf[1], TimerRefCodes);
				}
				#endregion

				#region OBJECT_TYPE_SLIDER
				else if (Attributes[0].Data[0] == HmiObjType.OBJECT_TYPE_SLIDER)
				{
					codesInf[1].Add("load '&objid&'".ToBytes());
				}
				#endregion

				#region OBJECT_TYPE_CURVE
				else if (Attributes[0].Data[0] == HmiObjType.OBJECT_TYPE_CURVE)
				{
					codesInf[1].Add("load '&objid&'".ToBytes());
				}
				#endregion

				#region TEXT
				else if (Attributes[0].Data[0] == HmiObjType.TEXT)
				{
					attrVal = GetAttributeValue("sta");
					if (attrVal != null)
					{
						if (attrVal.Length == 1 && attrVal[0] == 0)
						{
							codesInf[0].Add(Utility.ToBytes("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&picc&','&xcen&','&ycen&',0,'&txt&'"));

							if (Page.HmiObjects[0].GetAttributeValue("sta")[0] == 2
							 && Page.HmiObjects[0].GetAttributeValue("pic").ToU16() == GetAttributeValue("picc").ToU16()
								)
								codesInf[1].Add(Utility.ToBytes("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&picc&','&xcen&','&ycen&',3,'&txt&'"));
							else
								codesInf[1].Add(Utility.ToBytes("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&picc&','&xcen&','&ycen&',0,'&txt&'"));

							codesInf[4].Add(Utility.ToBytes("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&picc&','&xcen&','&ycen&',0,'&txt&'"));
						}
						else if (attrVal.Length == 1 && attrVal[0] == 1)
						{
							codesInf[0].Add(Utility.ToBytes("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&bco&','&xcen&','&ycen&',1,'&txt&'"));
							codesInf[1].Add(Utility.ToBytes("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&bco&','&xcen&','&ycen&',1,'&txt&'"));
							codesInf[4].Add(Utility.ToBytes("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&bco&','&xcen&','&ycen&',1,'&txt&'"));
						}
						else if (attrVal.Length == 1 && attrVal[0] == 2)
						{
							codesInf[0].Add(Utility.ToBytes("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&pic&','&xcen&','&ycen&',2,'&txt&'"));
							codesInf[1].Add(Utility.ToBytes("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&pic&','&xcen&','&ycen&',2,'&txt&'"));
							codesInf[4].Add(Utility.ToBytes("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&pic&','&xcen&','&ycen&',2,'&txt&'"));
						}
					}
				}
				#endregion

				#region BUTTON
				else if (Attributes[0].Data[0] == HmiObjType.BUTTON)
				{
					attrVal = GetAttributeValue("sta");
					if (attrVal != null)
					{
						if (attrVal.Length == 1 && attrVal[0] == 0)
						{
							codesInf[0].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&picc&','&xcen&','&ycen&',10,'&txt&'".ToBytes());
							if (Page.HmiObjects[0].GetAttributeValue("sta")[0] == 2
							 && Page.HmiObjects[0].GetAttributeValue("pic").ToU16() == GetAttributeValue("picc").ToU16()
								)
								codesInf[1].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&picc&','&xcen&','&ycen&',3,'&txt&'".ToBytes());
							else
								codesInf[1].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&picc&','&xcen&','&ycen&',10,'&txt&'".ToBytes());

							codesInf[2].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco2&','&picc2&','&xcen&','&ycen&',10,'&txt&'".ToBytes());
							codesInf[3].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&picc&','&xcen&','&ycen&',10,'&txt&'".ToBytes());
							codesInf[4].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&picc&','&xcen&','&ycen&',10,'&txt&'".ToBytes());
						}
						else if (attrVal.Length == 1 && attrVal[0] == 1)
						{
							codesInf[0].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&bco&','&xcen&','&ycen&',11,'&txt&'".ToBytes());
							codesInf[0].Add("draw3d '&x&','&y&','&endx&','&endy&',61341,6339".ToBytes());
							codesInf[1].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&bco&','&xcen&','&ycen&',11,'&txt&'".ToBytes());
							codesInf[1].Add("draw3d '&x&','&y&','&endx&','&endy&',61341,6339".ToBytes());
							codesInf[2].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco2&','&bco2&','&xcen&','&ycen&',11,'&txt&'".ToBytes());
							codesInf[2].Add("draw3d '&x&','&y&','&endx&','&endy&',61341,6339".ToBytes());
							codesInf[3].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&bco&','&xcen&','&ycen&',11,'&txt&'".ToBytes());
							codesInf[3].Add("draw3d '&x&','&y&','&endx&','&endy&',61341,6339".ToBytes());
							codesInf[4].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&bco&','&xcen&','&ycen&',11,'&txt&'".ToBytes());
							codesInf[4].Add("draw3d '&x&','&y&','&endx&','&endy&',61341,6339".ToBytes());
						}
						else if (attrVal.Length == 1 && attrVal[0] == 2)
						{
							codesInf[0].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&pic&','&xcen&','&ycen&',12,'&txt&'".ToBytes());
							codesInf[1].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&pic&','&xcen&','&ycen&',12,'&txt&'".ToBytes());
							codesInf[2].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco2&','&pic2&','&xcen&','&ycen&',12,'&txt&'".ToBytes());
							codesInf[3].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&pic&','&xcen&','&ycen&',12,'&txt&'".ToBytes());
							codesInf[4].Add("xstr '&x&','&y&','&w&','&h&','&font&','&pco&','&pic&','&xcen&','&ycen&',12,'&txt&'".ToBytes());
						}
					}
				}
				#endregion

				#region PROG
				else if (Attributes[0].Data[0] == HmiObjType.PROG)
				{
					attrVal = GetAttributeValue("sta");
					byte[] buffer2 = GetAttributeValue("dez");
					if (attrVal != null)
					{
						if (attrVal.Length == 1 && attrVal[0] == 0)
						{
							if (buffer2.Length == 1 && buffer2[0] == 0)
							{
								codesInf[0].Add("sysda0='&val&'*'&w&'/100".ToBytes());
								codesInf[0].Add("fill '&x&','&y&',sysda0,'&h&','&pco&'".ToBytes());
								codesInf[0].Add("fill '&x&'+sysda0,'&y&','&w&'-sysda0,'&h&','&bco&'".ToBytes());
								codesInf[1].Add("fill '&x&','&y&','&w&','&h&','&bco&'".ToBytes());
								codesInf[1].Add("fill '&x&','&y&','&val&'*'&w&'/100,'&h&','&pco&'".ToBytes());
								codesInf[4].Add("fill '&x&','&y&','&w&','&h&','&bco&'".ToBytes());
								codesInf[4].Add("fill '&x&','&y&','&val&'*'&w&'/100,'&h&','&pco&'".ToBytes());
							}
							if (buffer2.Length == 1 && buffer2[0] == 1)
							{
								codesInf[0].Add("sysda0='&val&'*'&h&'/100".ToBytes());
								codesInf[0].Add("fill '&x&','&endy&'+1-sysda0,'&w&',sysda0,'&pco&'".ToBytes());
								codesInf[0].Add("fill '&x&','&y&','&w&','&h&'-sysda0,'&bco&'".ToBytes());
								codesInf[1].Add("sysda0='&val&'*'&h&'/100".ToBytes());
								codesInf[1].Add("fill '&x&','&y&','&w&','&h&','&bco&'".ToBytes());
								codesInf[1].Add("fill '&x&','&endy&'+1-sysda0,'&w&',sysda0,'&pco&'".ToBytes());
								codesInf[4].Add("sysda0='&val&'*'&h&'/100".ToBytes());
								codesInf[4].Add("fill '&x&','&y&','&w&','&h&','&bco&'".ToBytes());
								codesInf[4].Add("fill '&x&','&endy&'+1-sysda0,'&w&',sysda0,'&pco&'".ToBytes());
							}
						}
						else if (attrVal.Length == 1 && attrVal[0] == 1)
						{
							if (buffer2.Length == 1 && buffer2[0] == 0)
							{
								codesInf[0].Add("sysda0='&val&'*'&w&'/100".ToBytes());
								codesInf[0].Add("xpic '&x&','&y&',sysda0,'&h&',0,0,'&ppic&'".ToBytes());
								codesInf[0].Add("xpic '&x&'+sysda0,'&y&','&w&'-sysda0,'&h&',sysda0,0,'&bpic&'".ToBytes());
								codesInf[1].Add("pic '&x&','&y&','&bpic&'".ToBytes());
								codesInf[1].Add("xpic '&x&','&y&','&val&'*'&w&'/100,'&h&',0,0,'&ppic&'".ToBytes());
								codesInf[4].Add("pic '&x&','&y&','&bpic&'".ToBytes());
								codesInf[4].Add("xpic '&x&','&y&','&val&'*'&w&'/100,'&h&',0,0,'&ppic&'".ToBytes());
							}
							if (buffer2.Length == 1 && buffer2[0] == 1)
							{
								codesInf[0].Add("sysda0='&val&'*'&h&'/100".ToBytes());
								codesInf[0].Add("xpic '&x&','&endy&'+1-sysda0,'&w&',sysda0,0,'&h&'-sysda0,'&ppic&'".ToBytes());
								codesInf[0].Add("xpic '&x&','&y&','&w&','&h&'-sysda0,0,0,'&bpic&'".ToBytes());
								codesInf[1].Add("sysda0='&val&'*'&h&'/100".ToBytes());
								codesInf[1].Add("pic '&x&','&y&','&bpic&'".ToBytes());
								codesInf[1].Add("xpic '&x&','&endy&'+1-sysda0,'&w&',sysda0,0,'&h&'-sysda0,'&ppic&'".ToBytes());
								codesInf[4].Add("sysda0='&val&'*'&h&'/100".ToBytes());
								codesInf[4].Add("pic '&x&','&y&','&bpic&'".ToBytes());
								codesInf[4].Add("xpic '&x&','&endy&'+1-sysda0,'&w&',sysda0,0,'&h&'-sysda0,'&ppic&'".ToBytes());
							}
						}
					}
				}
				#endregion

				#region PICTURE
				else if (Attributes[0].Data[0] == HmiObjType.PICTURE)
				{
					codesInf[0].Add("pic '&x&','&y&','&pic&'".ToBytes());
					codesInf[1].Add("pic '&x&','&y&','&pic&'".ToBytes());
					codesInf[4].Add("pic '&x&','&y&','&pic&'".ToBytes());
				}
				#endregion

				#region PICTUREC
				else if (Attributes[0].Data[0] == HmiObjType.PICTUREC)
				{
					codesInf[0].Add("xpic '&x&','&y&','&w&','&h&','&x&','&y&','&picc&'".ToBytes());
					codesInf[1].Add("xpic '&x&','&y&','&w&','&h&','&x&','&y&','&picc&'".ToBytes());
					codesInf[4].Add("xpic '&x&','&y&','&w&','&h&','&x&','&y&','&picc&'".ToBytes());
				}
				#endregion

				#region TOUCH
				else if (Attributes[0].Data[0] == HmiObjType.TOUCH)
				{
				}
				#endregion

				#region POINTER
				else if (Attributes[0].Data[0] == HmiObjType.POINTER)
				{
					attrVal = GetAttributeValue("sta");
					if (attrVal != null)
					{
						if ((attrVal.Length == 1) && (attrVal[0] == 0))
						{
							codesInf[0].Add("xpic '&x&','&y&','&w&','&h&','&x&','&y&','&picc&'".ToBytes());
							if (Page.HmiObjects[0].GetAttributeValue("sta")[0] != 2
							 || Page.HmiObjects[0].GetAttributeValue("pic").ToU16() != GetAttributeValue("picc").ToU16()
								)
							{
								codesInf[1].Add("xpic '&x&','&y&','&w&','&h&','&x&','&y&','&picc&'".ToBytes());
							}
							codesInf[4].Add("xpic '&x&','&y&','&w&','&h&','&x&','&y&','&picc&'".ToBytes());
						}
						else if ((attrVal.Length == 1) && (attrVal[0] == 1))
						{
							codesInf[0].Add("fill '&x&','&y&','&w&','&h&','&bco&'".ToBytes());
							codesInf[1].Add("fill '&x&','&y&','&w&','&h&','&bco&'".ToBytes());
							codesInf[4].Add("fill '&x&','&y&','&w&','&h&','&bco&'".ToBytes());
						}
						else if ((attrVal.Length == 1) && (attrVal[0] == 2))
						{
							codesInf[0].Add("pic '&x&','&y&','&pic&'".ToBytes());
							codesInf[1].Add("pic '&x&','&y&','&pic&'".ToBytes());
							codesInf[4].Add("pic '&x&','&y&','&pic&'".ToBytes());
						}
						codesInf[0].Add("draw_h '&w&'/2+'&x&','&h&'/2+'&y&','&h&'/2-'&wid&','&val&','&wid&','&pco&'".ToBytes());
						codesInf[1].Add("draw_h '&w&'/2+'&x&','&h&'/2+'&y&','&h&'/2-'&wid&','&val&','&wid&','&pco&'".ToBytes());
						codesInf[4].Add("draw_h '&w&'/2+'&x&','&h&'/2+'&y&','&h&'/2-'&wid&','&val&','&wid&','&pco&'".ToBytes());
					}
				}
				#endregion

				#region PAGE
				else if (Attributes[0].Data[0] == HmiObjType.PAGE)
				{
					attrVal = GetAttributeValue("sta");
					if (attrVal != null)
					{
						if (attrVal.Length == 1 && attrVal[0] == 1)
						{	// Page with color fill
							codesInf[0].Add(Utility.ToBytes("fill '&x&','&y&','&w&','&h&','&bco&'"));
							codesInf[1].Add(Utility.ToBytes("fill '&x&','&y&','&w&','&h&','&bco&'"));
							codesInf[4].Add(Utility.ToBytes("fill '&x&','&y&','&w&','&h&','&bco&'"));
						}
						else if (attrVal.Length == 1 && attrVal[0] == 2)
						{	// Page with picture background
							codesInf[0].Add(Utility.ToBytes("pic '&x&','&y&','&pic&'"));
							codesInf[1].Add(Utility.ToBytes("pic '&x&','&y&','&pic&'"));
							codesInf[4].Add(Utility.ToBytes("pic '&x&','&y&','&pic&'"));
						}
					}
				}
				#endregion
			}

			foreach (byte[] codes in codesInf[index])
				mycodes.Add(codes);
			return codesInf[index].Count;
		}

		public byte[] GetAttributeValue(string name)
		{
			Range range = new Range
			{
				Begin = 0,
				End = 7
			};
			HmiAttribute hmiAttr = new HmiAttribute();
			for (int i = 0; i < Attributes.Count; i++)
			{
				int idx = Utility.IndexOf(Attributes[i].Name, name.ToBytes(), range, 0);
				if (idx != 0xffff && (idx == 7 || Attributes[i].Name[idx + 1] == 0))
					return Attributes[i].Data;
			}
			return null;
		}

		public int GetCompileRefCodes(List<byte[]> cmds)
		{
			int count = 0;
			List<byte[]> codes = new List<byte[]>();
			GetAttributeCodes(codes, 4);

			codes.DeleteComments();
			count = codes.Count;

			replaceToValues(codes, 2);
			Utility.AddList(cmds, codes);

			return count;
		}

		public List<byte[]> GetCodes()
		{
			List<byte[]> codes = new List<byte[]>();
			GetCodes(codes);
			return codes;
		}

		public void GetCodes(List<byte[]> codes)
		{
			codes.Add(Utility.PatternBytes("att"));
			foreach (HmiAttribute attr in Attributes)
				codes.Add(Utility.MergeBytes(attr.Name, Utility.ToBytes(attr.InfoAttribute), attr.Data, attr.Note));
			codes.Add(Utility.PatternBytes("end"));

			codes.Add(Utility.PatternBytes("load"));
			Utility.AddList(codes, Codes[0]);
			codes.Add(Utility.PatternBytes("end"));

			codes.Add(Utility.PatternBytes("down"));
			Utility.AddList(codes, Codes[1]);
			codes.Add(Utility.PatternBytes("end"));

			codes.Add(Utility.PatternBytes("up"));
			Utility.AddList(codes, Codes[2]);
			codes.Add(Utility.PatternBytes("end"));

			codes.Add(Utility.PatternBytes("slide"));
			Utility.AddList(codes, Codes[3]);
			codes.Add(Utility.PatternBytes("end"));
		}

		public HmiObject CopyObject(HmiApplication hmiApp, HmiPage hmiPage)
		{
			HmiObject hmiObj = new HmiObject(hmiApp, hmiPage)
			{
				ObjInfo = ObjInfo
			};
			hmiObj.Codes[0] = Codes[0].CopyListBytes();
			hmiObj.Codes[1] = Codes[1].CopyListBytes();
			hmiObj.Codes[2] = Codes[2].CopyListBytes();
			hmiObj.Codes[3] = Codes[3].CopyListBytes();

			foreach (HmiAttribute attr in Attributes)
				hmiObj.Attributes.Add(attr.Clone());
			return hmiObj;
		}

		private int getDownCodes(List<byte[]> list)
		{
			List<byte[]> codes = new List<byte[]>();

			GetAttributeCodes(codes, 2);

			codes.AppendList(Codes[1]);
			codes.DeleteComments();

			int count = codes.Count;

			replaceToValues(codes, 0);
			list.AppendList(codes);

			return count;
		}

		/// <summary>
		/// Get attributes from name to "end"
		/// </summary>
		/// <param name="attrs"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		private List<byte[]> getAttributes(List<byte[]> attrs, string name)
		{
			bool flag = false;
			List<byte[]> selected = new List<byte[]>();
			foreach (byte[] attr in attrs)
			{
				string attrName = Utility.GetString(attr);	// Extract name
				if (flag)
				{	// Copy to selected up to "end"
					if (attr.Length == 3 && attrName == "end")
						break;
					selected.Add(attr);
				}
				else if (attr.Length == name.Length && attrName == name)
					flag = true;
			}
			return selected;
		}

		public int GetLoadCodes(List<byte[]> list, byte replace)
		{
			List<byte[]> codes = new List<byte[]>();

			GetAttributeCodes(codes, 1);
			codes.DeleteComments();
			int count = codes.Count;

			replaceToValues(codes, replace);
			list.AppendList(codes);
			return count;
		}

		#region GetObjRamBytes
		public ushort GetObjRamBytes(ref byte[] bytes, ushort pos0)
		{
			int length = bytes.Length;
			int pos = pos0;

			#region OBJECT_TYPE_SLIDER
			if (Attributes[0].Data[0] == HmiObjType.OBJECT_TYPE_SLIDER)
			{
				InfoSliderParam param = new InfoSliderParam();
				try
				{
					param.RefFlag = 0;
					param.Mode = GetAttributeValue("mode")[0];
					param.BackType = GetAttributeValue("sta")[0];
					param.CursorType = GetAttributeValue("psta")[0];
					param.CursorWidth = GetAttributeValue("wid")[0];
					param.CursorHeight = GetAttributeValue("hig")[0];

					if (param.BackType == 0)
						param.BackPicId = GetAttributeValue("picc").ToU16();
					else if (param.BackType == 2)
						param.BackPicId = GetAttributeValue("pic").ToU16();
					else if (param.BackType == 1)
						param.BackPicId = GetAttributeValue("bco").ToU16();

					if (param.CursorType == 0)
						param.CutsorPicId = GetAttributeValue("pco").ToU16();
					else if (param.CursorType == 1)
						param.CutsorPicId = GetAttributeValue("pic2").ToU16();

					param.MaxVal = GetAttributeValue("maxval").ToU16();
					param.MinVal = GetAttributeValue("minval").ToU16();
					param.NowVal = GetAttributeValue("val").ToU16();
					param.LastVal = 0xffff;

					Attributes[5].InfoAttribute.Start = (ushort)(ObjInfo.AttributeStart + 6);
					Attributes[6].InfoAttribute.Start = (ushort)(ObjInfo.AttributeStart + 6);
					Attributes[7].InfoAttribute.Start = (ushort)(ObjInfo.AttributeStart + 6);
					Attributes[8].InfoAttribute.Start = (ushort)(ObjInfo.AttributeStart + 8);
					Attributes[9].InfoAttribute.Start = (ushort)(ObjInfo.AttributeStart + 8);
					Attributes[10].InfoAttribute.Start = (ushort)(ObjInfo.AttributeStart + 4);
					Attributes[11].InfoAttribute.Start = (ushort)(ObjInfo.AttributeStart + 5);
					Attributes[12].InfoAttribute.Start = (ushort)(ObjInfo.AttributeStart + 10);
					Attributes[13].InfoAttribute.Start = (ushort)(ObjInfo.AttributeStart + 12);
					Attributes[14].InfoAttribute.Start = (ushort)(ObjInfo.AttributeStart + 14);

					bytes = Utility.MergeBytes(bytes, Utility.ToBytes(param));
					return (ushort)(bytes.Length - length);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
				return 0;
			}
			#endregion

			#region OBJECT_TYPE_CURVE
			if (Attributes[0].Data[0] == HmiObjType.OBJECT_TYPE_CURVE)
			{
				InfoCurveParam param = new InfoCurveParam();
				InfoCurveChannelParam channel_param = new InfoCurveChannelParam();
				try
				{
					param.BackType = GetAttributeValue("sta")[0];
					if (param.BackType == 0)
						param.PicID = GetAttributeValue("picc").ToU16();
					else if (param.BackType == 2)
						param.PicID = GetAttributeValue("pic").ToU16();

					param.GridX = GetAttributeValue("gdw")[0];
					param.GridY = GetAttributeValue("gdh")[0];
					param.RefFlag = 0;
					param.ChannelCount = (byte)(GetAttributeValue("ch")[0] + 1);
					param.Width = (ushort)((ObjInfo.Panel.EndX - ObjInfo.Panel.X) + 1);
					param.Height = (ushort)((ObjInfo.Panel.EndY - ObjInfo.Panel.Y) + 1);
					param.BackColor = GetAttributeValue("bco").ToU16();
					param.Griclr = GetAttributeValue("gdc").ToU16();
					if ((param.Width * 0.3) > 120.0)
						param.BufLen = (ushort)(param.Width + 120);
					else
						param.BufLen = (ushort)(param.Width * 1.3);

					Attributes[4].InfoAttribute.Start = (ushort)(ObjInfo.AttributeStart + 11);
					Attributes[5].InfoAttribute.Start = Attributes[4].InfoAttribute.Start;
					Attributes[6].InfoAttribute.Start = Attributes[4].InfoAttribute.Start;
					Attributes[7].InfoAttribute.Start = (ushort)(ObjInfo.AttributeStart + 13);
					Attributes[8].InfoAttribute.Start = (ushort)(ObjInfo.AttributeStart + 2);
					Attributes[9].InfoAttribute.Start = (ushort)(ObjInfo.AttributeStart + 3);
					bytes = Utility.MergeBytes(bytes, Utility.ToBytes(param));
					InfoCurveParam curve_param2 = new InfoCurveParam();
					InfoCurveChannelParam curve_channel_param2 = new InfoCurveChannelParam();
					pos = (ushort)((pos + Marshal.SizeOf(curve_param2)) + (Marshal.SizeOf(curve_channel_param2) * param.ChannelCount));
					for (int j = 0; j < param.ChannelCount; j++)
					{
						curve_param2 = new InfoCurveParam();
						curve_channel_param2 = new InfoCurveChannelParam();
						Attributes[10 + j].InfoAttribute.Start = (ushort)(((ObjInfo.AttributeStart + Marshal.SizeOf(curve_param2)) + (Marshal.SizeOf(curve_channel_param2) * j)) + 4);
						channel_param = new InfoCurveChannelParam();
						channel_param.Begin = (ushort)(pos + (j * param.BufLen));
						channel_param.End = (ushort)((channel_param.Begin + param.BufLen) - 1);
						switch (j)
						{
							case 0:
								channel_param.Penclr = GetAttributeValue("pco0").ToU16();
								break;

							case 1:
								channel_param.Penclr = GetAttributeValue("pco1").ToU16();
								break;

							case 2:
								channel_param.Penclr = GetAttributeValue("pco2").ToU16();
								break;

							case 3:
								channel_param.Penclr = GetAttributeValue("pco3").ToU16();
								break;
						}
						channel_param.BufFree = (ushort)(param.BufLen - 1);
						channel_param.BufNext = (ushort)channel_param.Begin;
						channel_param.DotLen = 0;
						bytes = Utility.MergeBytes(bytes, Utility.ToBytes(channel_param));
					}
					return (ushort)(bytes.Length + param.BufLen * param.ChannelCount - length);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
				return 0;
			}
			#endregion

			#region Attributes
			for (int i = 0; i < Attributes.Count; i++)
			{
				if (Attributes[i].InfoAttribute.AttrType < HmiAttributeType.Type15
				 && Attributes[i].InfoAttribute.CanModify == 1
				 && checkAttribute(Attributes[i]))
				{
					if (Attributes[i].InfoAttribute.AttrType == HmiAttributeType.String)
					{
						if ((Attributes[i].Data.Length == 0) || (Attributes[i].Data[Attributes[i].Data.Length - 1] != 0))
							Utility.MergeBytes(Attributes[i].Data, Utility.BYTE_ZERO);
						if (Attributes[i].Data.Length > Attributes[i].InfoAttribute.Length)
						{
							MessageBox.Show(string.Concat(
								"Attribute".Translate(),
								Utility.GetString(Attributes[i].Name),
								"The initial value is larger than the allocated space".Translate()
								));
							return 0;
						}
						bytes = Utility.MergeBytes(bytes, Attributes[i].Data, Utility.ToBytes("", Attributes[i].InfoAttribute.Length - Attributes[i].Data.Length));
					}
					else
					{
						if (Attributes[i].Data.Length != Attributes[i].InfoAttribute.Length)
						{
							MessageBox.Show(string.Concat(
								"Attribute".Translate(),
								Utility.GetString(Attributes[i].Name),
								"The initial value is larger than the allocated space".Translate()
								));
							return 0;
						}
						bytes = Utility.MergeBytes(bytes, Attributes[i].Data);
					}
					Attributes[i].InfoAttribute.Start = (ushort)pos;
					pos += Attributes[i].InfoAttribute.Length;
				}
			}
			#endregion

			return (ushort)(bytes.Length - length);
		}
		#endregion

		private int getRefCodes(List<byte[]> cmds)
		{
			List<byte[]> codes = new List<byte[]>();

			GetAttributeCodes(codes, 0);
			codes.DeleteComments();

			int count = codes.Count;

			replaceToValues(codes, 0);
			cmds.AppendList(codes);

			return count;
		}

		public int GetNoteLength(string name, bool useMinus)
		{
			Range range = new Range
			{
				Begin = 0,
				End = 7
			};
			HmiAttribute attr = new HmiAttribute();
			if (useMinus)
			{
				string[] strArray = name.Split(Utility.CHAR_MINUS);
				if (strArray.Length != 2)
					return 0;
				name = strArray[0];
			}

			for (int i = 0; i < Attributes.Count; i++)
			{
				int num2 = Utility.IndexOf(Attributes[i].Name, Utility.ToBytes(name), range, 0);
				if (num2 != 0xffff
				 && (num2 == 7 || Attributes[i].Name[num2 + 1] == 0)
				 && Attributes[i].InfoAttribute.AttrType == HmiAttributeType.String
					)
					return Attributes[i].InfoAttribute.Length;
			}
			return 0;
		}

		public int GetSlideCodes(List<byte[]> cmds)
		{
			List<byte[]> codes = new List<byte[]>();

			codes.AppendList(Codes[3]);
			codes.DeleteComments();

			int count = codes.Count;

			replaceToValues(codes, 0);
			cmds.AppendList(codes);
			return count;
		}

		public int GetUpCodes(List<byte[]> cmds)
		{
			List<byte[]> codes = new List<byte[]>();

			GetAttributeCodes(codes, 3);

			codes.AppendList(Codes[2]);
			codes.DeleteComments();

			int count = codes.Count;

			replaceToValues(codes, 0);
			cmds.AppendList(codes);
			return count;
		}

		public void PutCodes(List<byte[]> strings)
		{
			try
			{
				List<byte[]> attrs = getAttributes(strings, "att");
				int attrInfoSize = HmiOptions.InfoAttributeSize;
				Attributes.Clear();
				IsBinding = 0;
				foreach (byte[] attr in attrs)
				{
					if (attr.Length >= (attrInfoSize + 8) && attr.Length < 0x400)
					{
						HmiAttribute hmiAttr = new HmiAttribute
						{
							Name = Utility.SubBytes(attr, 0, 8)
						};
						byte[] bytes = Utility.SubBytes(attr, 8, attrInfoSize);
						hmiAttr.InfoAttribute = Utility.ToStruct<InfoAttribute>(bytes);
						if (attr.Length >= (attrInfoSize + 8 + hmiAttr.InfoAttribute.DataLength))
						{
							hmiAttr.Data = Utility.SubBytes(attr, attrInfoSize + 8, hmiAttr.InfoAttribute.DataLength);
							hmiAttr.Note = Utility.SubBytes(attr, attrInfoSize + 8 + hmiAttr.InfoAttribute.DataLength);
							if (hmiAttr.InfoAttribute.AttrType == HmiAttributeType.String)
							{
								if (hmiAttr.Data.Length == 0 || hmiAttr.Data[hmiAttr.Data.Length - 1] != 0)
									hmiAttr.Data = Utility.MergeBytes(hmiAttr.Data, Utility.BYTE_ZERO);
								hmiAttr.InfoAttribute.DataLength = (ushort)hmiAttr.Data.Length;
								if (hmiAttr.InfoAttribute.DataLength > hmiAttr.InfoAttribute.Length)
									hmiAttr.InfoAttribute.Length = hmiAttr.InfoAttribute.DataLength;
							}

							if (Attributes.Count == 1)
							{
								string name = Utility.GetString(hmiAttr.Name);
								if (name == "merry" || name == "memory")
									hmiAttr.Name = "vscope".ToBytes();
								else if (name != "vscope")
									Attributes.AddNewAttribute(
													"vscope",
													1,
													HmiAttributeType.Selection,
													IsYesNo.No,
													"0",
													"Variable scope ( Local variable is only visible in current page, while global variable is visible  in all pages):0-local;1-global".Translate(),
													0, 0, 1, 0
									);
							}
							Attributes.Add(hmiAttr);
							checkAttribute(hmiAttr);
						}
					}
				}
				if (Attributes.Count == 1)
					Attributes.AddNewAttribute("vscope", 1, HmiAttributeType.Selection, IsYesNo.No, "0", "Variable scope ( Local variable is only visible in current page, while global variable is visible  in all pages):0-local;1-global".Translate(), 0, 0, 1, 0);
			}
			catch (Exception ex)
			{
				MessageBox.Show("ERROR occoured when setting component properties".Translate() + ex.Message);
			}

			Codes[0] = getAttributes(strings, "load");
			Codes[1] = getAttributes(strings, "down");
			Codes[2] = getAttributes(strings, "up");
			Codes[3] = getAttributes(strings, "slide");
		}

		public void SetScreenXY()
		{
			if (ObjId == 0)
			{
				ObjInfo.Panel.X = 0;
				ObjInfo.Panel.Y = 0;
				ObjInfo.Panel.EndX = (ushort)(App.LcdWidth - 1);
				ObjInfo.Panel.EndY = (ushort)(App.LcdHeight - 1);
				ObjInfo.Panel.loadlei = 1;
				if (ObjId == 0 && Attributes.Count < 1)
					Attributes = ObjAttOperation.CreateAttrByType(HmiObjType.PAGE, ref ObjInfo.Panel);
			}
		}

		public bool SetAttrTextLength(string name, string newval)
		{
			Range bufPos = new Range
			{
				Begin = 0,
				End = 7
			};
			HmiAttribute attr = new HmiAttribute();
			int @int = Utility.GetInt(newval);
			if (@int > 0xff)
			{
				MessageBox.Show("Max allowed string length: 255 bytes.".Translate());
				return false;
			}
			for (int i = 0; i < Attributes.Count; i++)
			{
				int endName = Utility.IndexOf(Attributes[i].Name, name, bufPos);
				if (endName != 0xffff
				 && (endName == 7 || Attributes[i].Name[endName + 1] == 0)
				 && Attributes[i].InfoAttribute.AttrType == HmiAttributeType.String
					)
				{
					if (Attributes[i].Data.Length <= (@int + 1))
					{
						Attributes[i].InfoAttribute.Length = (ushort)(@int + 1);
						return true;
					}
					MessageBox.Show("The value should not be smaller than length of current string in value \"txt\".".Translate());
					return false;
				}
			}
			return false;
		}

		public bool SetAttrValue(string name, string newval)
		{
			Range range = new Range
			{
				Begin = 0,
				End = 7
			};
			HmiAttribute attr = new HmiAttribute();
			for (int i = 0; i < Attributes.Count; i++)
			{
				int num2 = Utility.IndexOf(Attributes[i].Name, Utility.ToBytes(name, 8), range, 0);
				if (num2 != 0xffff && (num2 == 7 || Attributes[i].Name[num2 + 1] == 0))
				{
					attr = Attributes[i];
					if ((attr.InfoAttribute.AttrType == 2) && (newval == ""))
						newval = "65535";

					if (attr.InfoAttribute.AttrType < HmiAttributeType.String)
					{
						int @int = Utility.GetInt(newval);
						if ((@int > attr.InfoAttribute.MaxValue) || (@int < attr.InfoAttribute.MinValue))
							return false;
						
						if (attr.InfoAttribute.Length == 1)
						{
							if (attr.InfoAttribute.AttrType == 5)
							{
								string[] strArray = name.Split(Utility.CHAR_MINUS);
								if (strArray.Length == 2)
									return SetAttrTextLength(strArray[0], newval);
							}
							else
							{
								attr.InfoAttribute.DataLength = 1;
								attr.Data[0] = (byte)@int;
							}
						}
						else if (attr.InfoAttribute.Length == 2)
						{
							attr.InfoAttribute.DataLength = 2;
							attr.Data = Utility.ToBytes(((ushort)@int));
						}
						else if (attr.InfoAttribute.Length == 4)
						{
							attr.InfoAttribute.DataLength = 4;
							attr.Data = Utility.ToBytes(((uint)@int));
						}
					}
					else
					{
						byte[] valueBytes = newval.ToBytes();
						if (valueBytes.Length >= attr.InfoAttribute.Length)
						{
							MessageBox.Show("Too long string, exceed the max length.".Translate());
							return false;
						}
						attr.Data = valueBytes;
						if (attr.Data.Length == 0 || attr.Data[attr.Data.Length - 1] != 0)
							attr.Data = Utility.MergeBytes(attr.Data, Utility.BYTE_ZERO);
						attr.InfoAttribute.DataLength = (ushort)attr.Data.Length;
					}
					Attributes[i] = attr;
					break;
				}
			}
			return true;
		}
	}
}