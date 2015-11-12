using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace NextionEditor
{
	public static class Utility
	{
		public static char[] CHAR_TILDA = new char[] { '~' };
		public static char[] CHAR_EQUAL = new char[] { '=' };
		public static char[] CHAR_GREAT = new char[] { '>' };
		public static char[] CHAR_MINUS = new char[] { '-' };
		public static char[] CHAR_COLON = new char[] { ':' };
		public static char[] CHAR_COMMA = new char[] { ',' };
		public static char[] CHAR_SEMICOLON = new char[] { ';' };
		public static char[] CHAR_SPACE = new char[] { ' ' };
		public static char[] CHAR_DOT = new char[] { '.' };
		public static byte[] BYTE_ZERO = new byte[1] { 0 };

		private static object m_locked;
		private static Dictionary<int, byte[]> m_patterns;
		private static Dictionary<int, string> m_lexis_en;
		private static Dictionary<int, string> m_lexis_cn;

		static Utility()
		{
			m_locked = new object();
			m_patterns = new Dictionary<int, byte[]>(100);
			m_lexis_en = new Dictionary<int, string>(500);
			m_lexis_cn = new Dictionary<int, string>(500);
		}

		public const byte FILE_TYPE_CN_TFT = (byte)'T';
		public const byte FILE_TYPE_CN_HMI = (byte)'U';
		public const byte FILE_TYPE_EN_TFT = (byte)'N';
		public const byte FILE_TYPE_EN_HMI = (byte)'O';
		public static byte FileType(bool compile)
		{
			return (byte)(
				compile
				? ((HmiOptions.Language == 0) ? FILE_TYPE_CN_TFT : FILE_TYPE_EN_TFT)
				: ((HmiOptions.Language == 0) ? FILE_TYPE_CN_HMI : FILE_TYPE_EN_HMI)
				);
		}

		public static void AddList(List<byte[]> dst, List<byte[]> src)
		{
			if (src != null)
				foreach (byte[] bytes in src)
					dst.Add(bytes);
		}
		public static void AppendList(this List<byte[]> dst, List<byte[]> src)
		{
			if (src != null)
				foreach (byte[] bytes in src)
					dst.Add(bytes);
		}

		public static void AddNewAttribute(
			this List<HmiAttribute> attrs,
			string name,
			ushort dataAllocation,
			byte attrType,
			byte isReturn,
			string value,
			string note,
			byte isBinding,
			byte isModify,
			uint maxval,
			uint minval
			)
		{
			int idx = Utility.GetInt(value);
			HmiAttribute item = new HmiAttribute();
			item.InfoAttribute.AttrType = attrType;

			if (attrType < HmiAttributeType.String)
			{
				item.InfoAttribute.MaxValue = maxval;
				item.InfoAttribute.MinValue = minval;

				if (idx <= item.InfoAttribute.MaxValue && idx >= item.InfoAttribute.MinValue)
				{
					if (dataAllocation == 1)
					{
						item.InfoAttribute.DataLength = 1;
						item.Data = new byte[] { (byte)idx };
					}
					else if (dataAllocation == 2)
					{
						item.InfoAttribute.DataLength = 2;
						item.Data = Utility.ToBytes((ushort)idx);
					}
					else if (dataAllocation == 4)
					{
						item.InfoAttribute.DataLength = 4;
						item.Data = Utility.ToBytes((uint)idx);
					}
					else
					{
						MessageBox.Show("Space allocation fault".Translate());
						return;
					}
				}
				item.InfoAttribute.Length = item.InfoAttribute.DataLength;
			}
			else
			{
				item.Data = ToBytes(value);
				if (item.Data.Length == 0 || item.Data[item.Data.Length - 1] != 0)
					item.Data = MergeBytes(item.Data, ToBytes("", 1));

				item.InfoAttribute.DataLength = (ushort)item.Data.Length;
				item.InfoAttribute.Length = (ushort)(dataAllocation + 1);
			}
			item.InfoAttribute.DataStart = 0xFF;
			item.Name = ToBytes(name, 8);
			item.InfoAttribute.Start = 0;
			item.InfoAttribute.IsBinding = isBinding;
			item.InfoAttribute.CanModify = isModify;
			item.InfoAttribute.IsReturn = isReturn;
			item.Note = ToBytes(note);
			attrs.Add(item);
		}

		public static void AddRichTextString(this RichTextBox textBox, string text, Color color)
		{
			int length = textBox.Text.Length;
			textBox.AppendText(text + "\r\n");
			textBox.SelectionStart = length;
			textBox.SelectionLength = text.Length;
			textBox.SelectionColor = color;
			textBox.SelectionStart = textBox.Text.Length;
			textBox.SelectionLength = 0;
			textBox.ScrollToCaret();
		}

		public static byte[] SubBytes(this byte[] src, int start)
		{
			byte[] dst = new byte[src.Length - start];
			for (int i = 0; i < dst.Length; i++)
				dst[i] = src[i + start];
			return dst;
		}

		public static byte[] SubBytes(this byte[] src, int start, int length)
		{
			byte[] dst = new byte[length];
			for (int i = 0; i < length; i++)
				dst[i] = src[i + start];
			return dst;
		}


		#region PosByteGetString
		public static string PosByteGetString(InfoRange range, byte[] bytes)
		{
			return HmiOptions.Encoding.GetString(bytes, range.Begin, range.End - range.Begin + 1);
		}
		/*
				public static unsafe string PosByteGetstring(InfoRange Pos, byte* buf)
				{
					byte[] str = new byte[Pos.End - Pos.Begin + 1];
					int index = 0;
					for (int i = Pos.Begin; i <= Pos.End; i++)
					{
						str[index] = buf[i];
						index++;
					}
					return HmiOptions.Encoding.GetString(str);
				}
		*/
		#endregion

		#region ToStrings(List<byte[]> list)
		public static string ToStrings(List<byte[]> list)
		{
			StringBuilder dst = new StringBuilder(100);
			if (list != null)
				foreach (byte[] bytes in list)
				{
					if (dst.Length != 0)
						dst.Append("\r\n");
					dst.Append(GetString(bytes));
				}
			return dst.ToString();
		}
		#endregion

		public static void CopyTo(byte[] src, int start, ref byte[] dst)
		{
			int j = 0;
			for (int i = start; i < src.Length; i++)
				dst[j++] = src[i];
		}
		/*
		public static unsafe void CopyTo(byte[] src, int start, byte* dst)
		{
			if (start < src.Length)
				for (int i = start; i < src.Length; i++)
				{
					dst[0] = src[i];
					dst++;
				}
		}
		*/
		public static ushort ToU16(this byte[] src)
		{
			ushort value = 0;
			if (src != null)
			{
				if (src.Length >= 1)
					value |= (ushort)((ushort)src[0] << 0);
				if (src.Length >= 2)
					value |= (ushort)((ushort)src[1] << 8);
			}
			return value;
		}
		public static uint ToU32(this byte[] src)
		{
			uint value = 0;
			if (src != null)
			{
				if (src.Length >= 1)
					value |= (uint)src[0] << 0;
				if (src.Length >= 2)
					value |= (uint)src[1] << 8;
				if (src.Length >= 3)
					value |= (uint)src[1] << 16;
				if (src.Length >= 4)
					value |= (uint)src[1] << 24;
			}
			return value;
		}

		public static unsafe T ToStruct<T>(byte[] src) where T : new()
		{
			try
			{
				IntPtr pStruct;
				fixed (byte* psrc = src)
					pStruct = (IntPtr)psrc;
				return (T)Marshal.PtrToStructure(pStruct, typeof(T));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			return default(T);
		}

		public static List<byte[]> CopyListBytes(this List<byte[]> list1)
		{
			List<byte[]> list = new List<byte[]>();
			list.AppendList(list1);
			return list;
		}

		public static bool CreateFolder(string path)
		{
			try
			{
				Directory.CreateDirectory(path);
			}
			catch
			{
				MessageBox.Show("Failed to create directory!".Translate() + path);
				return false;
			}
			return true;
		}

		private static void DeleteFile(string path)
		{
			try
			{
				File.Delete(path);
			}
			catch { }
		}

		public static bool DeleteFileWait(string path)
		{
			string text = "";
			int num = 40;
			while (num > -1)
			{
				Thread.Sleep(50);
				try
				{
					if (File.Exists(path))
						File.Delete(path);
					return true;
				}
				catch (Exception ex)
				{
					text = ex.Message;
					num--;
				}
			}
			MessageBox.Show(text);
			return false;
		}

		public static void DeleteFiles(string path, string mask)
		{
			try
			{
				DirectoryInfo folder = new DirectoryInfo(path);
				foreach (FileInfo file in folder.GetFiles(mask))
					DeleteFile(file.FullName);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		#region DeleteComments delzhushi
		public static void DeleteComments(this List<byte[]> bt)
		{
			for (int i = 0; i < bt.Count; ++i)
			{
				int num = 0;
				byte[] data = bt[i];

				if (data.Length == 0)
				{	// Remove zero length commands
					bt.RemoveAt(i);
					--i;
					continue;
				}

				for (int j = 0; j < data.Length - 1; ++j)
				{
					if ((num % 2) == 0 && data[j] == '/' && data[j + 1] == '/')
					{
						if (j == 0)
						{
							bt.RemoveAt(i);
							--i;
							break;
						}

						int k = j - 1;
						while (k >= 0 && data[k] == ' ')
							--k;

						if (k == -1)
						{
							bt.RemoveAt(i);
							--i;
							break;
						}
						bt[i] = SubBytes(data, 0, k + 1);
						break;
					}

					if (data[j] == '\\' && data[j + 1] == '"')
						++j;
					else if (data[j] == '"')
						++num;
				}
			}
		}
		#endregion

		public static void DrawThisLine(Panel panel, Color color, int wd)
		{
			Pen pen = new Pen(color, (float)wd);
			try
			{
				Point[] points = new Point[5];
				points[0].X = 1;
				points[0].Y = 0;
				points[1].X = panel.Width - 2;
				points[1].Y = 0;
				points[2].X = panel.Width - 2;
				points[2].Y = panel.Height - 1;
				points[3].X = 1;
				points[3].Y = panel.Height - 1;
				points[4].X = 1;
				points[4].Y = 0;
				panel.CreateGraphics().Clear(panel.BackColor);
				panel.CreateGraphics().DrawLines(pen, points);
			}
			catch { }
		}

		public static void DrawThisLine(this UserControl this1, Color color, int wd)
		{
			Pen pen = new Pen(color, (float)wd);
			try
			{
				Point[] points = new Point[5];
				points[0].X = 1;
				points[0].Y = 0;
				points[1].X = this1.Width - 2;
				points[1].Y = 0;
				points[2].X = this1.Width - 2;
				points[2].Y = this1.Height - 1;
				points[3].X = 1;
				points[3].Y = this1.Height - 1;
				points[4].X = 1;
				points[4].Y = 0;
				this1.CreateGraphics().Clear(this1.BackColor);
				this1.CreateGraphics().DrawLines(pen, points);
			}
			catch { }
		}

		public static ushort Get16Color(Color c)
		{
			int r = (c.R * 0x1f) / 0xff;
			int g = (c.G * 0x3f) / 0xff;
			int b = (c.B * 0x1f) / 0xff;
			ushort num = (byte)((r << 3) + (g >> 3));
			num = (ushort)(num << 8);
			return (ushort)(num + ((byte)(((g % 8) << 5) + b)));
		}

		public static Color Get24color(ushort c)
		{
			int num = c / 0x100;
			int num2 = c % 0x100;
			int red = ((num / 8) * 0xff) / 0x1f;
			int green = ((((num % 8) * 8) + (num2 / 0x20)) * 0xff) / 0x3f;
			int blue = ((num2 % 0x20) * 0xff) / 0x1f;
			return Color.FromArgb(red, green, blue);
		}

		public static Bitmap GetBitmap(byte[] image, InfoPicture infoPic, bool makeTransparent)
		{
			Bitmap bm = new Bitmap(infoPic.W, infoPic.H);
			Graphics.FromImage(bm).Clear(Color.White);

			ushort color16;
			Color color24;
			Color black = Color.FromArgb(0, 0, 0, 0);
			Color transparent = Utility.Get24color(HmiOptions.ColorTransparentReplace);
			bool vertical = (infoPic.IsPotrait == 1 ? true : false);
			bool useTransparent = (vertical)
									? !HmiOptions.OpenTransparent
									: !(HmiOptions.OpenTransparent || makeTransparent);
			int endX = infoPic.W - 1;
			int index = 0;
			try
			{
				for (int iY = 0; iY < infoPic.H; iY++)
					for (int iX = 0; iX < infoPic.W; iX++)
					{
						color16 = (ushort)((image[index + 1] << 8) + image[index]);

						if (color16 != HmiOptions.ColorTransparent)
							color24 = Utility.Get24color(color16);
						else if (useTransparent)
							color24 = transparent;
						else
							color24 = black;

						if (vertical)
							bm.SetPixel(endX - iX, iY, color24);
						else
							bm.SetPixel(iX, iY, color24);
						index += 2;
					}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			return bm;
		}

		#region ToBytes(this string src)
		public static byte[] ToBytes(this string src)
		{
			if (string.IsNullOrEmpty(src))
				return new byte[0];
			return HmiOptions.Encoding.GetBytes(src);
		}
		#endregion

		#region ToBytes(this string src, int length)
		public static byte[] ToBytes(string src, int length)
		{
			byte[] dst = new byte[length];
			byte[] bytes = HmiOptions.Encoding.GetBytes(src);
			for (int i = 0; i < length; i++)
			{
				if (i < bytes.Length)
					dst[i] = bytes[i];
				else
					dst[i] = 0;
			}
			return dst;
		}
		#endregion

		#region ToBytes(this object structure)
		public unsafe static byte[] ToBytes(object obj)
		{
			byte[] bytes = new byte[0];
			try
			{
				// int size = Marshal.SizeOf(obj);
				bytes = new byte[Marshal.SizeOf(obj)];
				IntPtr ptr;
				fixed (byte* px = bytes)
					ptr = (IntPtr) px;
				// = IntPtr.Zero;
				// ptr = Marshal.AllocHGlobal(size);
				Marshal.StructureToPtr(obj, ptr, false);
				// Marshal.Copy(ptr, bytes, 0, size);
			}
			catch(Exception ex)
			{
				MessageBox.Show("ToBytes exception:" + ex.Message);
				bytes = new byte[0];
			}
			finally
			{
				//if (ptr != IntPtr.Zero)
				//	Marshal.FreeHGlobal(ptr);
			}
			return bytes;
		}
		#endregion

		public static int GetComType(this byte[] bytes)
		{
			string[] strArray = new string[] { "cls ", "pic ", "picq ", "xstr ", "fill ", "line ", "draw ", "cir " };
			InfoRange range = new InfoRange { Begin = 0 };
			foreach (string str in strArray)
			{
				range.End = (ushort)(bytes.Length - 1);
				if (IndexOf(bytes, str.ToBytes(), range, 0) != 0xffff)
					return 1;
			}
			return 0;
		}

		private static int getRowEnd(this RichTextBox textBox, int start)
		{
			if (textBox.Text.Length == 0)
				return -1;

			int startIndex = start;
			if (startIndex >= textBox.Text.Length)
				startIndex = textBox.Text.Length - 1;

			while (startIndex < textBox.Text.Length)
			{
				if (textBox.Text[startIndex] == '\n')
					return (startIndex - 1);
				startIndex++;
			}
			return startIndex;
		}

		private static int getRowStart(this RichTextBox textBox, int start)
		{
			if (textBox.Text.Length == 0)
				return 0;

			int startIndex = start;
			if (startIndex >= textBox.Text.Length)
				startIndex = textBox.Text.Length - 1;

			if (textBox.Text[startIndex] == '\n')
				startIndex--;

			if (startIndex == -1)
				return 0;

			while (textBox.Text[startIndex] != '\n' && startIndex > 0)
				startIndex--;

			if (startIndex > 0)
				startIndex++;

			return startIndex;
		}

		public static string GetErrorText(string response)
		{
			if (string.IsNullOrEmpty(response))
				return "Empty response".Translate();

			string errorText = "";
			try
			{
				string[] tokens = response.Split(Utility.CHAR_SPACE);
				if (tokens.Length < 4 ||
					tokens[tokens.Length - 1] != "0xff" ||
					tokens[tokens.Length - 2] != "0xff" ||
					tokens[tokens.Length - 3] != "0xff"
					)
					return "Bad response".Translate();

				switch (tokens[0])
				{
					case "0x00": errorText = "Invalid command".Translate(); break;
					case "0x01": errorText = "Run successful".Translate(); break;
					case "0x02": errorText = "Invalid Component ID.".Translate(); break;
					case "0x03": errorText = "Invalid Page ID".Translate(); break;
					case "0x04": errorText = "Invalid Pic ID".Translate(); break;
					case "0x05": errorText = "Invalid Font ID".Translate(); break;
					case "0x06": errorText = "Invalid Timer ID".Translate(); break;
					case "0x07": errorText = "Timer is not configured".Translate(); break;
					case "0x08": errorText = "Invalid System Variables ID".Translate(); break;
					case "0x11": errorText = "Invalid baud rate".Translate(); break;
					case "0x12": errorText = "Invalid Component ID or Channel ID.".Translate(); break;
					case "0x1a": errorText = "Invalid Variables".Translate(); break;
					case "0x1b": errorText = "Invalid variable operation".Translate(); break;
					case "0x86": errorText = "Automatically enter sleep mode".Translate(); break;
					case "0x87": errorText = "Automatic wake sleep".Translate(); break;
					case "0x88": errorText = "System starts successfully".Translate(); break;
					case "0x89": errorText = "SD card upgrading".Translate(); break;
					case "0x70": errorText = "Return string".Translate(); break;
					case "0x71":
						if (tokens.Length == 8)
							errorText = "Return numerical value".Translate();
						break;
					case "0x66":
						if (tokens.Length == 5)
							errorText = string.Concat(
								"Current Page".Translate(),
								Convert.ToInt64(tokens[1], 16).ToString()
								);
						break;
					case "0x65":
						if (tokens.Length == 7)
							errorText = string.Concat(
								"Page".Translate(),
								":", Convert.ToInt32(tokens[1], 16).ToString(),
								"Component".Translate(),
								":", Convert.ToInt32(tokens[2], 16).ToString(),
								" ", Convert.ToInt32(tokens[3], 16) == 0 ? "Touch Release".Translate() : "Touch Press".Translate()
							);
						break;
					case "0x67":
						if (tokens.Length == 9)
							errorText = string.Concat(
									"Coordinate X:".Translate(),
									(Convert.ToInt32(tokens[1], 16) << 8) + Convert.ToInt32(tokens[2], 16),
									" Y:",
									(Convert.ToInt32(tokens[3], 16) << 8) + Convert.ToInt32(tokens[4], 16),
									" Status:".Translate(),
									(Convert.ToInt32(tokens[5], 16) == 0) ? "Touch Release".Translate() : "Touch Press".Translate()
								);
						break;
					case "0x68":
						if (tokens.Length == 9)
							errorText = string.Concat(
									"Sleep mode press event - coordinate X;".Translate(),
									(Convert.ToInt32(tokens[1], 16) << 8) + Convert.ToInt32(tokens[2], 16),
									" Y:",
									(Convert.ToInt32(tokens[3], 16) << 8) + Convert.ToInt32(tokens[4], 16),
									" Status:".Translate(),
									(Convert.ToInt32(tokens[5], 0x10) == 0) ? "Touch Release".Translate() : "Touch Press".Translate()
								);
						break;
				}
			}
			catch(Exception ex)
			{
				errorText = "Exception: " + ex.Message;
			}
			return errorText;
		}

		public static byte[] ConcatBytes(params byte[][] bytes)
		{
			List<byte> value = new List<byte>();
			foreach (byte[] element in bytes)
			{
				value.AddRange(element);
			}
			return value.ToArray();
		}
		public static byte[] MergeBytes(byte[] bytes1, byte[] bytes2)
		{
			byte[] array = new byte[bytes1.Length + bytes2.Length];
			bytes1.CopyTo(array, 0);
			bytes2.CopyTo(array, bytes1.Length);
			return array;
		}

		public static byte[] MergeBytes(byte[] bytes1, byte[] bytes2, byte[] bytes3)
		{
			byte[] array = new byte[(bytes1.Length + bytes2.Length) + bytes3.Length];
			bytes1.CopyTo(array, 0);
			bytes2.CopyTo(array, bytes1.Length);
			bytes3.CopyTo(array, (int)(bytes1.Length + bytes2.Length));
			return array;
		}

		public static byte[] MergeBytes(byte[] bytes1, byte[] bytes2, byte[] bytes3, byte[] bytes4)
		{
			byte[] array = new byte[((bytes1.Length + bytes2.Length) + bytes3.Length) + bytes4.Length];
			bytes1.CopyTo(array, 0);
			bytes2.CopyTo(array, bytes1.Length);
			bytes3.CopyTo(array, (int)(bytes1.Length + bytes2.Length));
			bytes4.CopyTo(array, (int)((bytes1.Length + bytes2.Length) + bytes3.Length));
			return array;
		}

		public static int GetInt(string text)
		{
			int value = 0;
			if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
				return value;
			return 0;
		}

		public static void SetAppPaths()
		{
			string path = Path.Combine(Application.StartupPath, "Data");
			if (!Directory.Exists(path) && !CreateFolder(path))
				path = Application.StartupPath;

			HmiOptions.AppDataPath = path;
			HmiOptions.AppDataBinPath = Path.Combine(path, "Compile");
			HmiOptions.RunFilePath = Path.Combine(HmiOptions.AppDataPath, string.Format(CultureInfo.InvariantCulture, "{0:yyyyMMddHHmmss}.ca", DateTime.Now));
		}

		public static List<byte[]> GetListBytes(this string src)
		{
			List<byte[]> list = new List<byte[]>();
			string[] tokens = src.Replace("(", "(")
								 .Replace(")", ")")
								 .Replace("\r\n", "\n")
								 .Split(new char[] { '\n' }
								);
			foreach (string token in tokens)
			{
				if (token.Trim().Length > 0)
					list.Add(token.Trim().ToBytes());
			}
			return list;
		}

		public static void SetInitialPath(OpenFileDialog dialog, string name)
		{
			dialog.InitialDirectory = GetXmlString(name);
		}

		public static void SetInitialPath(SaveFileDialog dialog, string name)
		{
			dialog.InitialDirectory = GetXmlString(name);
		}

		public static void GetCurve(int width, int offset, ref List<int> values)
		{
			int counter = 0;
			int angle_0_30_60 = 0;
			int widthDiv2 = width / 2;
			values.Clear();
			while (counter < 3)
			{
				int value = -1 * Convert.ToInt32((double)(Math.Sin((3.1415926535897931 * angle_0_30_60) / 60.0) * widthDiv2));
				values.Add(value + offset);
				if (value == 0)
					counter++;
				angle_0_30_60++;
			}
		}

		private static string GetUpdateString(string str, int canshuindex, int addindex)
		{
			int num;
			string[] strArray;
			string str2 = "";

			if (canshuindex == 0)
			{
				strArray = str.Split(CHAR_SPACE);
				int num2 = int.Parse(strArray[1]) + addindex;
				return (strArray[0] + " " + num2.ToString());
			}
			strArray = str.Split(CHAR_COMMA);
			for (num = 0; num < canshuindex; num++)
				str2 = str2 + strArray[num] + ",";
			
			str2 = str2 + ((int.Parse(strArray[canshuindex]) + addindex)).ToString() + ",";
			for (num = canshuindex + 1; num < strArray.Length; num++)
				str2 = str2 + strArray[num] + ",";
			
			return str2.Substring(0, str2.Length - 1);
		}

		public static string GetXmlString(string name)
		{
			string path = HmiOptions.AppDataPath + @"\data.xml";
			XmlDocument document = new XmlDocument();
			try
			{
				if (File.Exists(path))
				{
					document.Load(path);
					XmlNode node = document.SelectSingleNode("HMI").SelectSingleNode(name);
					if (node != null)
						return node.InnerText;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("An error occur when reading the configurateion:".Translate() + ex.Message);
				DeleteFileWait(path);
			}
			return "";
		}

		private static int getCommentStart(this RichTextBox textBox, int start, ref bool isfind)
		{
			int startIndex = start;
			isfind = false;
			int num2 = 0;
			if (startIndex >= textBox.Text.Length)
				startIndex = textBox.Text.Length - 1;

			while (startIndex < (textBox.Text.Length - 1))
			{
				if (textBox.Text.Substring(startIndex, 2) == "//" && (num2 % 2) == 0)
				{
					isfind = true;
					return (startIndex - 1);
				}
				if (textBox.Text.Substring(startIndex, 2) == "\\\"")
					startIndex++;
				else if (textBox.Text.Substring(startIndex, 1) == "\"")
					num2++;

				if (textBox.Text[startIndex] == '\n')
					return (startIndex - 1);
				startIndex++;
			}
			return (textBox.Text.Length - 1);
		}

		public static Array InsertArray(this Array origArray, Array inArray, int add)
		{
			Array destinationArray = Array.CreateInstance(origArray.GetType().GetElementType(), (int)(origArray.Length + inArray.Length));
			if (add == -1)
			{
				Array.Copy(inArray, 0, destinationArray, 0, inArray.Length);
				Array.Copy(origArray, 0, destinationArray, inArray.Length, origArray.Length);
				return destinationArray;
			}
			Array.Copy(origArray, 0, destinationArray, 0, add + 1);
			Array.Copy(inArray, 0, destinationArray, add + 1, inArray.Length);
			if (add < (origArray.Length - 1))
				Array.Copy(origArray, add + 1, destinationArray, add + 1 + inArray.Length, origArray.Length - add - 1);

			return destinationArray;
		}

		public static bool IsNameValid(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				MessageBox.Show("Illegal name".Translate());
				return false;
			}

			if (text[0] >= '0' && text[0] <= '9')
			{
				MessageBox.Show("First character of a Name can not be Number ".Translate());
				return false;
			}

			string invalid = "'\",&=+-*/.";
			for (int i = 0; i < invalid.Length; i++)
				if (text.IndexOf(invalid[i]) >= 0)
				{
					MessageBox.Show("Illegal character in the Name".Translate() + invalid.Substring(i, 1));
					return false;
				}
			return true;
		}

		#region Translate
		public static void TranslateInit()
		{
			string filename = Path.Combine(Application.StartupPath, "lexis.lang");
			if (File.Exists(filename))
			{	// lexis.lang format:
				// Chinese line (correct if need)
				// English line (no change)
				// ...
				string lineCN = null, lineEN;
				using (StreamReader reader = new StreamReader(filename, true))
				{
					while ((lineEN = reader.ReadLine()) != null)
					{
						if (lineCN == null)
						{	// Chinese line
							lineCN = lineEN;
						}
						else
						{	// English line
							if (!string.IsNullOrEmpty(lineEN)
							 && !string.IsNullOrEmpty(lineCN))
							{
								int hash = lineEN.GetHashCode();
								if (!m_lexis_en.ContainsKey(hash))
									m_lexis_en.Add(hash, lineCN);

								hash = lineCN.GetHashCode();
								if (!m_lexis_cn.ContainsKey(hash))
									m_lexis_cn.Add(hash, lineEN);
							}
							lineCN = null;
						}
					}
					reader.Close();
				}
			}
		}

		public static string Translate(this string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				int hash = text.GetHashCode();
				if (HmiOptions.Language == 0)
				{
					if (m_lexis_en.ContainsKey(hash))
						return m_lexis_en[hash];
				}
				else
				{
					if (m_lexis_cn.ContainsKey(hash))
						return m_lexis_cn[hash];
				}
			}
			return text;
		}

		public static void Translate(Form form)
		{
			if (form.Tag == null || (int)form.Tag != HmiOptions.Language)
			{
				form.Text = Translate(form.Text);
				foreach (Control control in form.Controls)
					Translate(control);
				form.Tag = HmiOptions.Language;
			}
		}

		public static void Translate(Control control)
		{
			try
			{
				control.Text = Translate(control.Text);

				if (control is MenuStrip)
					foreach (ToolStripMenuItem item in ((MenuStrip)control).Items)
						setToolStripText(item);
				else if (control is ContextMenuStrip)
					foreach (ToolStripMenuItem item in ((ContextMenuStrip)control).Items)
						setToolStripText(item);
				else if (control is ToolStrip)
					foreach (ToolStripItem item2 in ((ToolStrip)control).Items)
						setToolStripText(item2);
				else if (control is DataGridView)
					foreach (DataGridViewColumn column in ((DataGridView)control).Columns)
						column.HeaderText = Translate(column.HeaderText);
				else if (control is StatusStrip)
					foreach (ToolStripLabel label in ((StatusStrip)control).Items)
						label.Text = Translate(label.Text);

				if (control.Controls.Count > 0)
					for (int i = 0; i < control.Controls.Count; i++)
						Translate(control.Controls[i]);
			}
			catch { }
		}

		private static void setToolStripText(ToolStripItem stripItem)
		{
			stripItem.Text = Translate(stripItem.Text);

			int idx;
			if (stripItem is ToolStripMenuItem)
			{
				ToolStripMenuItem item = (ToolStripMenuItem)stripItem;
				if (item.DropDownItems.Count > 0)
					for (idx = 0; idx < item.DropDownItems.Count; idx++)
						setToolStripText(item.DropDownItems[idx]);
			}
			else if (stripItem is ToolStripSplitButton)
			{
				ToolStripSplitButton button = (ToolStripSplitButton)stripItem;
				if (button.DropDownItems.Count > 0)
					for (idx = 0; idx < button.DropDownItems.Count; idx++)
						setToolStripText(button.DropDownItems[idx]);
			}
			else if (stripItem is ToolStripDropDownButton)
			{
				ToolStripDropDownButton button2 = (ToolStripDropDownButton)stripItem;
				if (button2.DropDownItems.Count > 0)
					for (idx = 0; idx < button2.DropDownItems.Count; idx++)
						setToolStripText(button2.DropDownItems[idx]);
			}
		}
		#endregion

		#region OpenFile
		public static bool OpenFile(ref StreamReader sr, string path)
		{
			try
			{
				sr = new StreamReader(path);
				return true;
			}
			catch
			{
				return false;
			}
		}
		#endregion

		#region BytesToString(this byte[] bytes)
		public static string GetString(byte[] bytes)
		{
			int index = 0;
			while (index < bytes.Length)
			{
				if (bytes[index] == 0)
					break;
				index++;
			}
			if (index == 0)
				return string.Empty;
			return HmiOptions.Encoding.GetString(bytes, 0, index);
		}
		#endregion

		#region BytesToString(this byte[] bytes, byte endChar)
		public static string GetString(byte[] bytes, byte endChar)
		{
			int index = 0;
			while (index < bytes.Length)
			{
				if (bytes[index] == 0 || bytes[index] == endChar)
					break;
				index++;
			}
			if (index == 0)
				return string.Empty;
			return HmiOptions.Encoding.GetString(bytes, 0, index);
		}
		#endregion

		#region
		public static void OpenWeb(string url)
		{
			if (openIE(url, 0) != "")
			{
				string text = openIE(url, 1);
				if (text != "")
					MessageBox.Show(text);
			}
		}
		#endregion

		#region
		private static string openIE(string url, int state)
		{
			try
			{
				if (state == 0)
					Process.Start(url);
				else if (state == 1)
					Process.Start("iexplore.exe", url);
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
			return "";
		}
		#endregion

		#region SavePath
		public static void SavePath(OpenFileDialog op, string name)
		{
			if (File.Exists(op.FileName))
				PutXmlString(Path.GetDirectoryName(op.FileName), name);
		}

		public static void SavePath(SaveFileDialog op, string name)
		{
			if (File.Exists(op.FileName))
				PutXmlString(Path.GetDirectoryName(op.FileName), name);
		}
		#endregion

		#region SaveOption
		public static void SaveOption(string value, string name)
		{
			PutXmlString(value, name);
		}
		#endregion

		#region PutXmlString
		public static void PutXmlString(string value, string name)
		{
			string filename = Path.Combine(HmiOptions.AppDataPath, "data.xml");
			XmlDocument xdoc = new XmlDocument();
			try
			{
				if (!File.Exists(filename))
				{
					xdoc.AppendChild(xdoc.CreateXmlDeclaration("1.0", "utf-8", null));
					xdoc.AppendChild(xdoc.CreateElement("", "HMI", ""));
					xdoc.Save(filename);
				}

				xdoc.Load(filename);
				XmlNode node = xdoc.SelectSingleNode("HMI").SelectSingleNode(name);
				if (node == null)
				{
					XmlNode xhmi = xdoc.SelectSingleNode("HMI");
					XmlElement xname = xdoc.CreateElement(name);
					xname.InnerText = value;
					xhmi.AppendChild(xname);
				}
				else
					node.InnerText = value;
				
				xdoc.Save(filename);
			}
			catch (Exception ex)
			{
				MessageBox.Show("An error occur when writing the configurateion:".Translate() + ex.Message);
				DeleteFileWait(filename);
			}
		}
		#endregion

		#region RunCode
		public static byte RunComOk(byte[] buf, InfoRange range, ushort valBegin, ushort valEnd, InfoCodeResults result, byte paramCount)
		{
			byte[] val = PatternBytes("comok ");
			InfoRange laction = new InfoRange();
			ushort pos = 0;
			ushort index = 0;
			laction.End = range.End;
			pos = IndexOf(buf, val, range, 0);
			result.CodeResults = InfoRange.List(paramCount);

			if (pos == 0xffff)
				return 0;
			if (paramCount != 0)
			{
				byte[] comma = Utility.ToBytes(",");
				++pos;
				for (index = 0; index < paramCount; index++)
				{
					if (pos > range.End)
						return 0;

					laction.Begin = pos;
					result.CodeResults[index].Begin = pos;
					pos = IndexOf(buf, comma, laction, 1);
					if (pos == 0xffff)
					{
						if (index != (paramCount - 1))
							return 0;
						result.CodeResults[index].End = range.End;
						return 1;
					}
					if (pos == result.CodeResults[index].Begin)
						return 0;

					result.CodeResults[index].End = (ushort)(pos - 1);
					++pos;
				}
			}
			return 1;
		}
		#endregion

		#region PosGetU16
		public static ushort PosGetU16(InfoRange Pos, byte[] bytes)
		{
			ushort num = 0;
			ushort num2 = 0;
			int end = Pos.End;
			ushort num4 = 1;

			if (bytes[Pos.Begin] <= '@' || bytes[Pos.Begin] >= '[')
			{
				for (; num2 < 6 && (bytes[end] >= '0' && bytes[end] <= '9'); --end)
				{
					num += (ushort)(num4 * (ushort)(bytes[end] - '0'));
					num4 *= 10;
					++num2;
					if (end == Pos.Begin)
						break;
				}
			}
			else
			{
				if (IndexOf(bytes, "RED", Pos) != 0xffff) return 0xf800;
				if (IndexOf(bytes, "BLUE", Pos) != 0xffff) return 0x1f;
				if (IndexOf(bytes, "GRAY", Pos) != 0xffff) return 0x8430;
				if (IndexOf(bytes, "BLACK", Pos) != 0xffff) return 0;
				if (IndexOf(bytes, "WHITE", Pos) != 0xffff) return 0xffff;
				if (IndexOf(bytes, "GREEN", Pos) != 0xffff) return 0x7e0;
				if (IndexOf(bytes, "BROWN", Pos) != 0xffff) return 0xbc40;
				if (IndexOf(bytes, "YELLOW", Pos) != 0xffff) return 0xffe0;
			}
			return num;
		}
		#endregion

		#region
		public static byte[] PackageEnd = new byte[3] { 0xFF, 0xFF, 0xFF };
		#endregion

		#region SendStringEnd(this SerialPort port)
		public static void SendStringEnd(this SerialPort port)
		{
			SendStringEnd(port, string.Empty);
		}
		#endregion

		public static void SendStringEnd(this SerialPort port, string data)
		{
			try
			{
				data = data.Trim();
				if (port.IsOpen)
				{
					if (data.Length > 2)
					{
						port.Write(data);
						// byte[] bytes = data.StringToBytes();
						// port.Write(bytes, 0, bytes.Length);
					}
					port.Write(PackageEnd, 0, 3);
				}
			}
			catch { }
		}

		public static void SetSizeToParent(this Control me)
		{
			try
			{
				me.Top = 0;
				me.Left = 0;
				me.Width = me.Parent.Width;
				me.Height = me.Parent.Height;
			}
			catch { }
		}

		public static void SetLineSelect(this RichTextBox textBox)
		{
			if (textBox.Text.Length > 0)
			{
				int start = 0;
				while (start < (textBox.Text.Length - 1))
					start = textBox.SetLineSelect(start) + 2;
			}
		}

		public static int SetLineSelect(this RichTextBox textBox, int start)
		{
			textBox.Parent.Focus();

			if (textBox.Text.Length == 0)
				return 0;

			bool isfind = false;
			int startIndex = start;
			if (startIndex >= textBox.Text.Length)
				startIndex = textBox.Text.Length - 1;

			int rowStart = textBox.getRowStart(startIndex);
			int rowEnd = textBox.getRowEnd(startIndex);

			int commentStart = textBox.getCommentStart(rowStart, ref isfind);
			if (commentStart >= rowStart)
			{
				textBox.Select(rowStart, (rowEnd - rowStart) + 1);
				textBox.SelectionColor = Color.Black;
			}

			if (isfind)
			{
				rowStart = commentStart + 1;
				commentStart = rowEnd;
				if (commentStart >= rowStart)
				{
					textBox.Select(rowStart, (rowEnd - rowStart) + 1);
					textBox.SelectionColor = Color.Green;
				}
			}
			textBox.Select(start, 0);
			textBox.Focus();
			return rowEnd;
		}

		public static void Showzi(this Bitmap bm, byte[] byte1, int height, Color color)
		{
			int num6 = 0;
			int num3 = (bm.Width - ((byte1.Length * 8) / height)) / 2;
			int x = num3;
			int y = bm.Height - height;
			for (int i = 0; i < byte1.Length; i++)
			{
				num6 = byte1[i];
				for (int j = 0; j < 8; j++)
				{
					if ((1 & (num6 >> (7 - j))) > 0)
						bm.SetPixel(x, y, color);

					x++;
					if (x >= (num3 + ((byte1.Length * 8) / height)))
					{
						x = num3;
						y++;
					}
				}
			}
		}

		public static byte[] PatternBytes(string pattern)
		{
			return PatternBytes(ref pattern);
		}

		public static byte[] PatternBytes(ref string pattern)
		{
			byte[] bytes;
			int hash = (pattern ?? "").GetHashCode();
			if (m_patterns.ContainsKey(hash))
				bytes = m_patterns[hash];
			else
			{
				bytes = ToBytes(pattern);
				lock (m_locked)
				{
					try
					{
						m_patterns.Add(hash, bytes);
					}
					catch { }
				}
			}
			return bytes;
		}

		#region IndexOf
		/// <summary>
		/// 
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="pattern"></param>
		/// <param name="pos"></param>
		/// <param name="isAnyPosition">= 0 pattern must be at beginning, #0 at any position</param>
		/// <returns></returns>
		public static unsafe ushort IndexOf(byte[] buf, byte[] pattern, InfoRange pos, bool isAnyPosition)
		{
			fixed (byte* numRef = buf)
				return IndexOf(numRef, pattern, pos, isAnyPosition);
		}

		public static unsafe ushort IndexOf(byte[] buf, string pattern, InfoRange pos)
		{
			fixed (byte* numRef = buf)
				return IndexOf(numRef, pattern, pos);
		}
		public static unsafe ushort IndexOf(byte* buf, string pattern, InfoRange pos)
		{
			byte[] pattBytes = Utility.PatternBytes(ref pattern);

			int index = 0;
			for (int i = pos.Begin; i <= pos.End; i++)
			{
				if (buf[i] == pattBytes[index])
				{
					index++;
					if (index >= pattBytes.Length || pattBytes[index] < 32)
						return (ushort)i;
				}
				else
					break;
			}
			return 0xFFFF;
		}

		public static unsafe ushort IndexOfAny(byte[] buf, string patt, InfoRange pos)
		{
			byte[] pattern = Utility.PatternBytes(ref patt);

			int index = 0;
			for (int i = pos.Begin; i <= pos.End; i++)
			{
				if (buf[i] == pattern[index])
				{
					index++;
					if (index >= pattern.Length || pattern[index] < 32)
						return (ushort)i;
				}
				else
					index = 0;
			}
			return 0xFFFF;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="pattern"></param>
		/// <param name="pos"></param>
		/// <param name="isAnyPosition">= 0 pattern must be at beginning, #0 at any position</param>
		/// <returns></returns>
		public static unsafe ushort IndexOf(byte* buf, byte[] pattern, InfoRange pos, bool isAnyPosition)
		{
			int index = 0;
			for (int i = pos.Begin; i <= pos.End; i++)
			{
				if (buf[i] == pattern[index])
				{
					index++;
					if (index >= pattern.Length || pattern[index] < 32)
						return (ushort)i;
				}
				else
				{
					if (!isAnyPosition)
						return 0xFFFF;
					index = 0;
				}
			}
			return 0xFFFF;
		}

		public static ushort IndexOf(byte[] src, byte[] pattern, InfoRange range, byte starmod)
		{
			ushort patternIdx = 0;
			for (int srcIdx = range.Begin; srcIdx <= range.End; srcIdx++)
			{
				if (src[srcIdx] == pattern[patternIdx])
				{
					++patternIdx;
					if (patternIdx >= pattern.Length || pattern[patternIdx] == 0)
						return (ushort)srcIdx;
				}
				else
				{
					if (starmod == 0)
						break;
					patternIdx = 0;
				}
			}
			return 0xFFFF;
		}
		#endregion

		internal static uint ToUInt32(byte[] bytes, ushort start, ushort len)
		{
			byte[] u4 = new byte[4] { 0, 0, 0, 0 };
			for(int idx = 0; idx < len; ++idx)
			{
				if (start + idx < bytes.Length)
					u4[idx] = bytes[start + idx];
			}
			return BitConverter.ToUInt32(u4, 0);
		}
	}
}
