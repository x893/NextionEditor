using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NextionEditor
{
	public class HmiApplication
	{
		public delegate void AppChangEvent(bool change);
		public delegate bool AppFileSave();

		public AppChangEvent ChangeApplication;
		public AppFileSave FileSave;

		public List<InfoPicture> Pictures = new List<InfoPicture>();
		public List<byte[]> PictureImages = new List<byte[]>();
		public List<InfoFont> Fonts = new List<InfoFont>();
		public List<byte[]> FontImages = new List<byte[]>();
		public List<HmiPage> HmiPages = new List<HmiPage>();

		public bool IsPotrait;
		public bool ChangeApp = false;
		public int Errors;
		public int Warnings;
		public ushort LcdWidth = 320;
		public ushort LcdHeight = 240;
		public byte[] OverBytes;
		public bool IsShowName = true;

		public HmiPage AddPage()
		{
			HmiPage page = new HmiPage(this);

			string name = "page";
			int idx = 0;
			for (; idx < 0xffff; idx++)
				if (!FindPageByName(name + idx.ToString()))
					break;
			page.Name = name + idx.ToString();

			HmiPages.Add(page);
			RefreshPageId();

			HmiObject hmiObject = new HmiObject(this, page);
			hmiObject.ObjName = page.Name;

			page.HmiObjects.Add(hmiObject);
			RefreshObjId(page);
			hmiObject.SetScreenXY();
			return HmiPages[HmiPages.Count - 1];
		}

		public HmiPage CopyPage(int index)
		{
			HmiPage page = AddPage();
			page.HmiObjects[0] = HmiPages[index].HmiObjects[0].CopyObject(this, page);
			page.HmiObjects[0].ObjName = page.Name;
			for (int i = 1; i < HmiPages[index].HmiObjects.Count; i++)
			{
				HmiObject copyeobj = HmiPages[index].HmiObjects[i].CopyObject(this, page);
				copyeobj.ObjName = HmiPages[index].HmiObjects[i].ObjName;
				page.HmiObjects.Add(copyeobj);
			}
			RefreshObjId(page);
			return page;
		}

		public void DeleteAllPages()
		{
			HmiPages.Clear();
		}

		public void DeleteAllPictures()
		{
			Pictures.Clear();
			PictureImages.Clear();
		}

		public void DeleteAllFonts()
		{
			Fonts.Clear();
			FontImages.Clear();
		}

		public void DeletePage(int index, bool isRefreshId)
		{
			HmiPages.RemoveAt(index);
			if (isRefreshId)
				RefreshPageId();
		}

		public void DeletePicture(int index)
		{
			Pictures.RemoveAt(index);
			PictureImages.RemoveAt(index);
		}

		public void DeleteFont(int index)
		{
			Fonts.RemoveAt(index);
			FontImages.RemoveAt(index);
		}

		public bool FindObjByName(HmiPage page, HmiObject obj, string name)
		{
			name = Utility.ToBytes(name, 4).ToString();
			foreach (HmiObject mobj in page.HmiObjects)
				if (mobj.ObjName == name && mobj != obj)
					return true;
			return false;
		}

		public bool FindPageByName(string name)
		{
			for (int idx = 0; idx < HmiPages.Count; idx++)
				if (HmiPages[idx].Name == name)
					return true;
			return false;
		}

		public int GetAllDataSize()
		{
			int idx;
			int size = 0;
			for (idx = 0; idx < PictureImages.Count; idx++)
				size += PictureImages[idx].Length;

			for (idx = 0; idx < FontImages.Count; idx++)
				size += FontImages[idx].Length;

			return (size / 1000);
		}

		public int GetObjQuantity()
		{
			int qty = 0;
			foreach (HmiPage mpage in HmiPages)
				qty += mpage.HmiObjects.Count;

			return qty;
		}

		public HmiPage InsertPage(int index)
		{
			HmiPage item = new HmiPage(this);
			string name = "newpage";
			int idx = 0;
			while (idx < 0xffff)
			{
				if (!FindPageByName(name + idx.ToString()))
					break;
				++idx;
			}
			item.Name = name + idx.ToString();
			HmiPages.Insert(index, item);
			RefreshPageId();
			HmiObject mobj = new HmiObject(this, item)
			{
				App = this,
				ObjName = item.Name
			};
			item.HmiObjects.Add(mobj);
			mobj.SetScreenXY();
			RefreshObjId(item);
			return item;
		}

		public void MakeObjName(HmiPage page, HmiObject obj, byte mark)
		{
			try
			{
				string str = "";
				string newname = "";
				str = HmiObjType.GetNamePrefix(mark);
				for (int i = 0; i < 0xff; i++)
				{
					newname = str + i.ToString();
					if (!FindObjByName(page, obj, newname))
					{
						obj.ObjName = newname;
						break;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		public bool Open(string filename)
		{
			try
			{
				StreamReader reader = new StreamReader(filename);
				if (reader.BaseStream.Length < 4L)
				{
					MessageBox.Show("Wrong resource file or resource file has been damaged".Translate());
					reader.Close();
					reader.Dispose();
					return false;
				}

				byte[] buffer = new byte[3];
				reader.BaseStream.Position = 1L;
				reader.BaseStream.Read(buffer, 0, buffer.Length);
				if (buffer[2] == Utility.FILE_TYPE_CN_HMI)
					buffer[2] = Utility.FILE_TYPE_EN_HMI;	// Change Chinesse to English

				if (buffer[2] != Utility.FILE_TYPE_EN_HMI)
				{
					MessageBox.Show("Wrong resource file or resource file has been damaged".Translate());
					reader.Close();
					reader.Dispose();
					return false;
				}

				if ((HmiOptions.VersionMajor != 0 || HmiOptions.VersionMinor != 8)
				&& (buffer[0] > HmiOptions.VersionMajor || (buffer[0] == HmiOptions.VersionMajor && buffer[1] > HmiOptions.VersionMinor))
					)
				{
					MessageBox.Show(string.Concat(
						"The project is produced by higher versions editor(S".Translate(),
						buffer[0], ".", buffer[1],
						"), please upgrade your software.".Translate()
						)
					);
					reader.Close();
					reader.Dispose();
					return false;
				}

				if (buffer[0] != HmiOptions.VersionMajor
				&& (MessageBox.Show(
							string.Concat(
								"The project is made by OLD version editor (".Translate(),
								buffer[0], ".", buffer[1],
								") Would you like to upgrade the latest version ? ".Translate(),
								HmiOptions.VersionMajor, ".", HmiOptions.VersionMinor
							),
							"Source file version upgrade".Translate(),
							MessageBoxButtons.YesNo
						) != DialogResult.Yes)
					)
				{
					reader.Close();
					reader.Dispose();
					return false;
				}

				if (buffer[0] == HmiOptions.VersionMajor)
					return readInfoApp(this, reader);
				if (buffer[0] == 3)
					return true;

				MessageBox.Show("This file is not compatiable".Translate());
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			return false;
		}



		#region ReadInfoApp readdatathis
		// private static List<InfoObject> m_Objects = new List<InfoObject>();
		// private static List<InfoPage> m_Pages = new List<InfoPage>();
		// private static List<byte[]> m_StringDatas = new List<byte[]>();
		// private static List<InfoString> m_Strings = new List<InfoString>();


		private bool readInfoApp(HmiApplication app, StreamReader reader)
		{
			List<InfoObject> m_Objects = new List<InfoObject>();
			List<InfoPage> m_Pages = new List<InfoPage>();
			List<byte[]> m_StringDatas = new List<byte[]>();
			List<InfoString> m_Strings = new List<InfoString>();

			InfoApp infoApp; // = new InfoApp();
			try
			{
				int idx;

				// Load Application Info
				reader.BaseStream.Position = 0L;
				byte[] buffer = new byte[HmiOptions.InfoAppSize];
				reader.BaseStream.Read(buffer, 0, buffer.Length);
				infoApp = Utility.ToStruct<InfoApp>(buffer);

				// Load Pictures and PictureImage
				app.Pictures.Clear();
				app.PictureImages.Clear();
				if (infoApp.PictureCount != 0)
				{
					reader.BaseStream.Position = infoApp.PictureStart;
					buffer = new byte[HmiOptions.InfoPictureSize];
					for (idx = 0; idx < infoApp.PictureCount; idx++)
					{
						reader.BaseStream.Read(buffer, 0, buffer.Length);
						app.Pictures.Add(Utility.ToStruct<InfoPicture>(buffer));
					}

					reader.BaseStream.Position = infoApp.PictureImageStart;
					for (idx = 0; idx < app.Pictures.Count; idx++)
					{
						buffer = new byte[app.Pictures[idx].Size];
						reader.BaseStream.Read(buffer, 0, buffer.Length);
						app.PictureImages.Add(buffer);
					}
				}

				// Load Fonts with FontImage
				app.Fonts.Clear();
				app.FontImages.Clear();
				if (infoApp.FontCount != 0)
				{
					reader.BaseStream.Position = infoApp.FontStart;
					buffer = new byte[HmiOptions.InfoFontSize];
					for (idx = 0; idx < infoApp.FontCount; idx++)
					{
						reader.BaseStream.Read(buffer, 0, buffer.Length);
						app.Fonts.Add(Utility.ToStruct<InfoFont>(buffer));
					}

					reader.BaseStream.Position = infoApp.FontImageStart;
					for (idx = 0; idx < app.Fonts.Count; idx++)
					{
						buffer = new byte[app.Fonts[idx].Size];
						reader.BaseStream.Read(buffer, 0, buffer.Length);
						app.FontImages.Add(buffer);
					}
				}

				if (infoApp.StringCount != 0)
				{
					reader.BaseStream.Position = infoApp.StringStart;
					buffer = new byte[HmiOptions.InfoStringSize];
					for (idx = 0; idx < infoApp.StringCount; idx++)
					{
						reader.BaseStream.Read(buffer, 0, buffer.Length);
						m_Strings.Add(Utility.ToStruct<InfoString>(buffer));
					}

					reader.BaseStream.Position = infoApp.StringDataStart;
					for (idx = 0; idx < m_Strings.Count; idx++)
					{
						buffer = new byte[m_Strings[idx].Size];
						reader.BaseStream.Read(buffer, 0, buffer.Length);
						m_StringDatas.Add(buffer);
					}
				}

				if (infoApp.PageCount != 0)
				{
					reader.BaseStream.Position = infoApp.PageStart;
					buffer = new byte[HmiOptions.InfoPageSize];
					for (idx = 0; idx < infoApp.PageCount; idx++)
					{
						reader.BaseStream.Read(buffer, 0, buffer.Length);
						m_Pages.Add(Utility.ToStruct<InfoPage>(buffer));
					}
				}

				if (infoApp.ObjectCount != 0)
				{
					reader.BaseStream.Position = infoApp.ObjectStart;
					buffer = new byte[HmiOptions.InfoObjectSize];
					for (idx = 0; idx < infoApp.ObjectCount; idx++)
					{
						reader.BaseStream.Read(buffer, 0, buffer.Length);
						m_Objects.Add(Utility.ToStruct<InfoObject>(buffer));
					}
				}

				reader.Close();
				reader.Dispose();

				app.IsPotrait = (infoApp.IsPotrait == 1 ? true : false);
				app.LcdWidth = (app.IsPotrait ? infoApp.ScreenHeight : infoApp.ScreenWidth);
				app.LcdHeight = (app.IsPotrait ? infoApp.ScreenWidth : infoApp.ScreenHeight);

				if (infoApp.VersionMajor == 0 && infoApp.VersionMinor < 30)
				{
					HmiOptions.OpenTransparent = false;
					for (int j = 0; j < app.PictureImages.Count; j++)
					{
						idx = 0;
						while (idx < (app.PictureImages[j].Length - 1))
						{
							if ((app.PictureImages[j][idx] == HmiOptions.ColorTransparent) && (app.PictureImages[j][idx + 1] == 0))
								app.PictureImages[j][idx] = (byte)HmiOptions.ColorTransparentReplace;
							idx += 2;
						}
					}
				}
				else
					HmiOptions.OpenTransparent = false;

				app.HmiPages.Clear();
				List<byte[]> strings = new List<byte[]>();

				for (int i = 0; i < m_Pages.Count; i++)
				{
					HmiPage hmiPage = new HmiPage(app)
					{
						Name = Utility.GetString(Utility.ToBytes(m_Pages[i].Name))
					};

					if (m_Pages[i].ObjStart != 0xffff && m_Pages[i].ObjEnd != 0xffff)
					{
						for (idx = m_Pages[i].ObjStart; idx <= m_Pages[i].ObjEnd; idx++)
						{
							HmiObject hmiObject = new HmiObject(app, hmiPage)
							{
								ObjInfo = m_Objects[idx],
								ObjName = Utility.GetString(Utility.ToBytes(m_Objects[idx].Name))
							};
							strings.Clear();
							for (int k = m_Objects[idx].StringInfoStart; k <= m_Objects[idx].StringInfoEnd; k++)
								strings.Add(m_StringDatas[k]);

							hmiObject.PutCodes(strings);
							hmiPage.HmiObjects.Add(hmiObject);
						}
					}
					app.HmiPages.Add(hmiPage);
				}

				app.RefreshAllId();
				if (infoApp.VersionMajor == 0 && infoApp.VersionMinor == 8)
				{
					foreach (HmiPage mpage2 in app.HmiPages)
						if (mpage2.HmiObjects[0].Attributes.Count == 4
						 && Utility.GetString(mpage2.HmiObjects[0].Attributes[2].Name) == "pco"
							)
							mpage2.HmiObjects[0].Attributes[2].Name = Utility.ToBytes("bco", 8);
				}
				return true;
			}
			catch (Exception ex)
			{
				reader.Close();
				reader.Dispose();
				MessageBox.Show(ex.Message);
				return false;
			}
		}
		#endregion


		public void RefreshAllId()
		{
			RefreshPageId();
			for (int i = 0; i < HmiPages.Count; i++)
				RefreshObjId(HmiPages[i]);
		}

		public void RefreshObjId(HmiPage page)
		{
			for (int i = 0; i < page.HmiObjects.Count; i++)
				page.HmiObjects[i].ObjId = i;
		}

		public void RefreshPageId()
		{
			for (int i = 0; i < HmiPages.Count; i++)
				HmiPages[i].PageId = i;
		}

		public bool RenameObj(HmiPage page, HmiObject obj, string newname)
		{
			int length = newname.ToBytes().Length;
			if ((length == 0) || (length > 14))
			{
				MessageBox.Show("Min Length 1 byte, Max Length 14 byte".Translate());
				return false;
			}
			if (!Utility.IsNameValid(newname))
			{
				return false;
			}
			if (FindObjByName(page, obj, newname))
			{
				MessageBox.Show("Duplicate Name!".Translate());
				return false;
			}
			obj.ObjName = newname;
			return true;
		}

		public bool SaveFile(string binPath, bool compile, RichTextBox compilerOutput)
		{
			if (compilerOutput == null)
				compilerOutput = new RichTextBox();

			compilerOutput.Text = "";
			Errors = 0;
			Warnings = 0;
			if (!Utility.DeleteFileWait(binPath))
				return false;

			return SaveToFile(binPath, compile, compilerOutput);
		}

		#region SaveToFile
		public bool SaveToFile(string binPath, bool compile, RichTextBox textCompile)
		{
			SaveToFileData data = new SaveToFileData();
			StreamWriter writer = new StreamWriter(binPath);
			InfoApp infoApp = new InfoApp();
			try
			{
				if (!prepareToSave(ref infoApp, compile, textCompile, data))
				{
					textCompile.AddRichTextString(
						string.Concat(
							"Compile failure!".Translate(), " ",
							Errors, " Errors,".Translate(), " ",
							Warnings, " Warnings,".Translate()
						), Color.Red);
					writer.Close();
					writer.Dispose();
					return false;
				}

				infoApp.VersionMajor = HmiOptions.VersionMajor;
				infoApp.VersionMinor = HmiOptions.VersionMinor;

				infoApp.FileType = Utility.FileType(compile);

				byte[] buffer = Utility.ToBytes(infoApp);
				writer.BaseStream.Write(buffer, 0, HmiOptions.InfoAppSize);

				int idx;
				if (compile)
				{	// Fill zeros to 4K boundary
					buffer = new byte[0x1000 - HmiOptions.InfoAppSize];
					for (idx = 0; idx < buffer.Length; idx++)
						buffer[idx] = 0;
					writer.BaseStream.Write(buffer, 0, buffer.Length);
				}

				for (idx = 0; idx < Pictures.Count; idx++)
					writer.BaseStream.Write(PictureImages[idx], 0, PictureImages[idx].Length);

				for (idx = 0; idx < Fonts.Count; idx++)
					writer.BaseStream.Write(FontImages[idx], 0, FontImages[idx].Length);

				if (compile)
				{	// Compile : 
					int imgSize = (getPictureImagesSize() + getFontImagesSize()) % 0x1000;
					if (imgSize != 0)
					{
						buffer = new byte[0x1000 - imgSize];
						for (idx = 0; idx < buffer.Length; idx++)
							buffer[idx] = 0;
						writer.BaseStream.Write(buffer, 0, buffer.Length);
					}
				}

				for (idx = 0; idx < data.m_infoStrings.Count; idx++)
					writer.BaseStream.Write(data.m_stringDatas[idx], 0, data.m_stringDatas[idx].Length);

				if (compile)
				{
					StreamReader reader = new StreamReader(Path.Combine(Application.StartupPath, "fwc.bin"));
					buffer = new byte[reader.BaseStream.Length];
					reader.BaseStream.Position = 0L;
					reader.BaseStream.Read(buffer, 0, buffer.Length);
					writer.BaseStream.Write(buffer, 0, buffer.Length);
					reader.Close();
				}

				for (idx = 0; idx < data.m_infoPages.Count; idx++)
				{
					buffer = Utility.ToBytes(data.m_infoPages[idx]);
					writer.BaseStream.Write(buffer, 0, HmiOptions.InfoPageSize);
				}

				for (idx = 0; idx < data.m_infoObjects.Count; idx++)
				{
					buffer = Utility.ToBytes(data.m_infoObjects[idx]);
					writer.BaseStream.Write(buffer, 0, HmiOptions.InfoObjectSize);
				}

				uint dataStart = 0;
				for (idx = 0; idx < infoApp.PictureCount; idx++)
				{
					InfoPicture picture = Pictures[idx];
					picture.DataStart = dataStart;
					buffer = Utility.ToBytes(picture);
					writer.BaseStream.Write(buffer, 0, HmiOptions.InfoPictureSize);
					dataStart += (uint)PictureImages[idx].Length;
				}

				for (idx = 0; idx < infoApp.StringCount; idx++)
				{
					buffer = Utility.ToBytes(data.m_infoStrings[idx]);
					writer.BaseStream.Write(buffer, 0, HmiOptions.InfoStringSize);
				}

				dataStart = 0;
				for (idx = 0; idx < infoApp.FontCount; idx++)
				{
					InfoFont font = Fonts[idx];
					font.DataOffset = dataStart;
					buffer = Utility.ToBytes(font);
					writer.BaseStream.Write(buffer, 0, HmiOptions.InfoFontSize);
					dataStart += (uint)FontImages[idx].Length;
				}
				writer.Close();
				writer.Dispose();
				textCompile.AddRichTextString(
					string.Concat(
						"Compile Success!".Translate(), " ",
						Errors, " Errors,".Translate(), " ",
						Warnings, " Warnings, FileSize:".Translate(), infoApp.FontDataStart
						),
					Color.Black);
				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			return false;
		}
		#endregion

		#region SaveToFileData
		private class SaveToFileData
		{
			public List<InfoPage> m_infoPages = new List<InfoPage>();
			public List<InfoObject> m_infoObjects = new List<InfoObject>();
			public List<InfoString> m_infoStrings = new List<InfoString>();
			public List<byte[]> m_stringDatas = new List<byte[]>();
			public int m_stringDataAddress = 0;
		}
		#endregion

		#region getPictureImagesSize
		/// <summary>
		/// Calculate size of Picture Images
		/// </summary>
		/// <param name="hmiApp"></param>
		/// <returns></returns>
		private int getPictureImagesSize()
		{
			int size = 0;
			for (int i = 0; i < Pictures.Count; i++)
				size += PictureImages[i].Length;
			return size;
		}
		#endregion
		#region getFontImagesSize
		/// <summary>
		/// Calculate size of Font Images
		/// </summary>
		/// <param name="hmiApp"></param>
		/// <returns></returns>
		private int getFontImagesSize()
		{
			int size = 0;
			for (int i = 0; i < Fonts.Count; i++)
				size += FontImages[i].Length;
			return size;
		}
		#endregion
		#region addString
		/// <summary>
		/// Add String Info and String Data
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		private int addString(byte[] bytes, SaveToFileData data)
		{
			InfoString infoString;
			infoString.Size = (ushort)bytes.Length;
			infoString.Start = (uint)data.m_stringDataAddress;

			data.m_infoStrings.Add(infoString);			// Add String Info (descriptor)
			data.m_stringDatas.Add(bytes);				// Add String Data
			data.m_stringDataAddress += bytes.Length;	// Add to String Data Address
			return (data.m_infoStrings.Count - 1);		// Return String Info last index
		}
		#endregion

		#region prepareToSave
		private bool prepareToSave(
			ref InfoApp infoApp,
			bool compile,
			RichTextBox textCompile,
			SaveToFileData data
			)
		{
			infoApp.IsPotrait = (byte)(IsPotrait ? 1 : 0);
			infoApp.ScreenWidth = (IsPotrait ? LcdHeight : LcdWidth);
			infoApp.ScreenHeight = (IsPotrait ? LcdWidth : LcdHeight);

			bool success = true;
			ushort customDataLength = 0;
			OverBytes = Utility.ToBytes((uint)customDataLength);

			if (compile)
			{
				for (int idxPage = 0; idxPage < HmiPages.Count; idxPage++)
				{
					HmiPage hmiPage = HmiPages[idxPage];
					for (int idxObj = 0; idxObj < hmiPage.HmiObjects.Count; ++idxObj)
					{
						HmiObject hmiObj = hmiPage.HmiObjects[idxObj];

						hmiObj.ObjInfo.ObjType = hmiObj.Attributes[0].Data[0];
						hmiObj.ObjInfo.IsCustomData = hmiObj.Attributes[1].Data[0];

						if (hmiObj.ObjInfo.IsCustomData == 1)
						{
							hmiObj.ObjInfo.AttributeStart = customDataLength;
							int objRambytes = hmiObj.GetObjRamBytes(ref OverBytes, customDataLength);
							if (objRambytes != 0)
							{
								hmiObj.ObjInfo.AttributeLength = (ushort)objRambytes;
								customDataLength += hmiObj.ObjInfo.AttributeLength;
							}
						}
					}
				}
			}
			addString(OverBytes, data);

			List<byte[]> bts = new List<byte[]>();
			for (int idxPage = 0; idxPage < HmiPages.Count; idxPage++)
			{
				HmiPage hmiPage = HmiPages[idxPage];

				if (compile && !hmiPage.Compile(textCompile))
					success = false;
				else
				{
					Application.DoEvents();

					InfoPage infoPage = new InfoPage
					{
						ObjCount = (byte)hmiPage.HmiObjects.Count,
						Name = Utility.ToStruct<InfoBytes14>(Utility.ToBytes(hmiPage.Name, 14))
					};
					if (infoPage.ObjCount > 0)
					{
						infoPage.ObjStart = (ushort)data.m_infoObjects.Count;

						for (int idxObj = 0; idxObj < hmiPage.HmiObjects.Count; idxObj++)
						{
							HmiObject hmiObj = hmiPage.HmiObjects[idxObj];
							hmiObj.ObjInfo.Name = Utility.ToStruct<InfoBytes14>(Utility.ToBytes(hmiObj.ObjName, 14));
							bts.Clear();
							if (compile)
								hmiObj.CompileCodes(bts);
							else
								hmiObj.GetCodes(bts);

							if (bts.Count > 0)
							{
								hmiObj.ObjInfo.StringInfoStart = (ushort)data.m_infoStrings.Count;
								for (int k = 0; k < bts.Count; k++)
									addString(bts[k], data);

								hmiObj.ObjInfo.StringInfoEnd = (ushort)((hmiObj.ObjInfo.StringInfoStart + bts.Count) - 1);
							}
							else
							{
								MessageBox.Show("Detect the component code is 0, error will occur when save the source file".Translate());
								hmiObj.ObjInfo.StringInfoStart = 0xffff;
								hmiObj.ObjInfo.StringInfoEnd = 0xffff;
							}
							data.m_infoObjects.Add(hmiObj.ObjInfo);
						}
						infoPage.ObjEnd = (ushort)((infoPage.ObjStart + hmiPage.HmiObjects.Count) - 1);
					}
					else
					{
						infoPage.ObjStart = 0xffff;
						infoPage.ObjEnd = 0xffff;
					}
					if (compile)
					{
						infoPage.InstStart = (ushort)data.m_infoStrings.Count;
						for (int num5 = 0; num5 < hmiPage.Codes.Count; num5++)
							addString(hmiPage.Codes[num5], data);
						infoPage.InstEnd = (ushort)(data.m_infoStrings.Count - 1);
					}
					else
					{
						infoPage.InstStart = 0xffff;
						infoPage.InstEnd = 0xffff;
					}
					data.m_infoPages.Add(infoPage);
				}
			}

			if (success)
			{
				infoApp.PageCount = (ushort)data.m_infoPages.Count;
				infoApp.ObjectCount = (ushort)data.m_infoObjects.Count;
				infoApp.FontCount = (ushort)Fonts.Count;
				infoApp.PictureCount = (ushort)Pictures.Count;
				infoApp.PictureImageStart = (uint)HmiOptions.InfoAppSize;

				if (compile)
					infoApp.PictureImageStart = 0x1000;

				infoApp.FontImageStart = infoApp.PictureImageStart + ((uint)getPictureImagesSize());
				infoApp.StringDataStart = infoApp.FontImageStart + ((uint)getFontImagesSize());
				if (compile)
				{
					uint num6 = infoApp.StringDataStart % 0x1000;
					if (num6 != 0)
						infoApp.StringDataStart += 0x1000 - num6;
				}

				infoApp.FirmwareStart = infoApp.StringDataStart + (uint)data.m_stringDataAddress;
				infoApp.FirmwareSize = 0;

				if (compile)
				{	// Add bootloader if compile
					StreamReader reader = new StreamReader(Path.Combine(Application.StartupPath, "fwc.bin"));
					infoApp.FirmwareSize = (uint)reader.BaseStream.Length;
					reader.Close();
				}

				infoApp.PageStart = infoApp.FirmwareStart + infoApp.FirmwareSize;
				infoApp.ObjectStart = infoApp.PageStart + ((uint)(data.m_infoPages.Count * HmiOptions.InfoPageSize));
				infoApp.PictureStart = infoApp.ObjectStart + ((uint)(infoApp.ObjectCount * HmiOptions.InfoObjectSize));
				infoApp.StringStart = infoApp.PictureStart + ((uint)(HmiOptions.InfoPictureSize * infoApp.PictureCount));
				infoApp.StringCount = (uint)data.m_infoStrings.Count;
				infoApp.FontStart = infoApp.StringStart + ((uint)(HmiOptions.InfoStringSize * data.m_infoStrings.Count));
				infoApp.FontDataStart = infoApp.FontStart + ((uint)(HmiOptions.InfoFontSize * infoApp.FontCount));
			}
			return success;
		}
		#endregion
	}
}
