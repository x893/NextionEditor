using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace NextionEditor
{
	internal class ComUser
	{
		public SerialPort Port;
		public int RunState = 1;

		private List<int> m_baudrates = new List<int>();
		private List<string> m_coms = new List<string>();

		private string m_flashId = "";
		private string m_oldCom = "";
		private string m_model = "";

		private byte[] m_fwVersion = new byte[2];
		private bool m_isTouch = false;

		private int m_lcdId = 0;
		private int m_flashSize = 0;
		private int m_oldBaud = 0;

		#region ComUser()
		public ComUser()
		{
			m_baudrates.Clear();
			m_baudrates.Add(2400);
			m_baudrates.Add(4800);
			m_baudrates.Add(9600);
			m_baudrates.Add(19200);
			m_baudrates.Add(38400);
			m_baudrates.Add(57600);
			m_baudrates.Add(115200);

			m_oldBaud = Utility.GetInt(Utility.GetXmlString("oldbo_"));
			m_oldCom = Utility.GetXmlString("oldcom_");
		}
		#endregion

		#region clearReadData
		private void clearReadData()
		{
			while (Port.BytesToRead > 0)
				Port.ReadByte();
		}
		#endregion

		#region ComClose
		public void ComClose()
		{
			try
			{
				if (Port != null && Port.IsOpen)
					Port.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
		#endregion

		#region comOpen
		private bool comOpen(string portname, int baurate)
		{
			bool flag = false;
			try
			{
				if (Port != null)
				{
					Port.BaudRate = baurate;
					Port.PortName = portname;
					Port.Open();
					flag = true;
				}
			}
			catch (Exception ex)
			{
				flag = false;
				if (m_coms.Count == 1)
					MessageBox.Show(ex.Message);
			}
			return flag;
		}
		#endregion

		#region getComString()
		private int getComString()
		{
			int num = 0;
			int index = 0;
			byte[] response = new byte[500];
			CodeResults cgcode = new CodeResults();
			cgcode.CodeResult = Range.List(10);

			try
			{
				while (Port.BytesToRead > 0)
				{
					response[index] = (byte)Port.ReadByte();
					index++;
					if (index > response.Length - 1)
						break;
				}
				index -= 4;	// Must be >= 5 bytes
				if (index < 1)
					return 0;

				Range range = new Range(0, index);

				num = Utility.IndexOfEx(response, Utility.PatternBytes("comok "), range, true);
				m_flashSize = 0;
				if (num != 0xffff)
				{
					range.Begin = num - 5;
					if (RunComOk(response, range, 0, 5, cgcode, 7))
					{
						m_isTouch = (Utility.PosByteGetString(cgcode.CodeResult[0], response) != "0");
						m_lcdId = Utility.PosGetU16(cgcode.CodeResult[1], response);
						m_model = Utility.PosByteGetString(cgcode.CodeResult[2], response);
						m_fwVersion[0] = (byte)Utility.PosGetU16(cgcode.CodeResult[3], response);
						m_fwVersion[1] = (byte)Utility.PosGetU16(cgcode.CodeResult[4], response);
						m_flashId = Utility.PosByteGetString(cgcode.CodeResult[5], response);
						m_flashSize = Utility.GetInt(Utility.PosByteGetString(cgcode.CodeResult[6], response));
						if (m_model.Length > 1)
						{
							if ((HmiOptions.Language == 0 && m_model.Substring(0, 1) == "T")
							 || (HmiOptions.Language == 1 && m_model.Substring(0, 1) == "N")
								)
								return 1;

							if (m_model.Length > 6)
							{
								if (HmiOptions.Language == 1 && m_model.Substring(0, 6) == "TJC024")
								{
									Port.SendStringEnd("tjchmi-setapp 1,9325,NX3224T024_011R");
									return 1;
								}
								if (HmiOptions.Language == 1 && m_model.Substring(0, 6) == "TJC043")
								{
									Port.SendStringEnd("tjchmi-setapp 1,0043,NX4827T043_011R");
									return 1;
								}
							}
							MessageBox.Show("Invalid Device.".Translate() + m_model);
						}
						return 0;
					}

					if (RunComOk(response, range, 0, 5, cgcode, 6))
					{
						m_isTouch = Utility.PosByteGetString(cgcode.CodeResult[0], response) != "0";
						m_lcdId = Utility.PosGetU16(cgcode.CodeResult[1], response);
						m_model = Utility.PosByteGetString(cgcode.CodeResult[2], response);
						m_fwVersion[0] = (byte)Utility.PosGetU16(cgcode.CodeResult[3], response);
						m_fwVersion[1] = (byte)Utility.PosGetU16(cgcode.CodeResult[4], response);
						m_flashId = Utility.PosByteGetString(cgcode.CodeResult[5], response);

						if (m_model.Length > 1)
						{
							if (m_model.Substring(0, 1) == "T")
								return 1;
							if (m_model.Substring(0, 1) == "N")
								return 1;

							if (m_model.Length > 6)
							{
								if (m_model.Substring(0, 6) == "TJC024")
								{
									Port.SendStringEnd("tjchmi-setapp 1,9325,NX3224T024_011R");
									return 1;
								}
								if (m_model.Substring(0, 6) == "TJC043")
								{
									Port.SendStringEnd("tjchmi-setapp 1,0043,NX4827T043_011R");
									return 1;
								}
							}
						}
						return 0;
					}
				}
				else if (Utility.IndexOfEx(response, Utility.PatternBytes("downbin"), range, true) != 0xffff)
					return 2;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			return 0;
		}
		#endregion

		#region GetDevicePort
		/// <summary>
		/// Open or Scan COM ports
		/// </summary>
		/// <param name="textBox"></param>
		/// <param name="portname"></param>
		/// <param name="baudrate">= 0 for auto or baudrate</param>
		/// <returns></returns>
		public int GetDevicePort(ref string status, string portname, int baudrate)
		{
			string[] portNames = null;
			List<int> scan_baudrates = new List<int>();
			scan_baudrates.Clear();
			m_coms.Clear();
			RunState = 0;

			if (baudrate == 0 && m_oldBaud > 0)
				scan_baudrates.Add(m_oldBaud);

			// Add previously opened port in auto
			if (!string.IsNullOrEmpty(m_oldCom) && (string.IsNullOrEmpty(portname) || portname.Contains("Auto Search".Translate())))
				m_coms.Add(m_oldCom);

			// Build baudrates list
			if (baudrate == 0)
				foreach (int baud in m_baudrates)
					scan_baudrates.Add(baud);
			else
				scan_baudrates.Add(baudrate);

			if (string.IsNullOrEmpty(portname) || portname.Contains("Auto Search".Translate()))
			{
				try
				{
					portNames = SerialPort.GetPortNames();
					foreach (string port in portNames)
						m_coms.Add(port);
				}
				catch (Exception ex)
				{
					MessageBox.Show("Obtain COM list failed!\r\n error message:".Translate() + ex.Message);
				}
			}
			else
				m_coms.Add(portname);

			// Scan port(s) and baudrate(s)
			foreach (string com in m_coms)
				for (int i = 0; i < scan_baudrates.Count; i++)
				{
					status = string.Concat("Connecting..".Translate(), com, ":", scan_baudrates[i], "\r\n");
					ComClose();

					if (comOpen(com, scan_baudrates[i]))
					{
						clearReadData();
						if (RunState == 2)
						{
							ComClose();
							status = "Connection fail".Translate() + "\r\n";
							RunState = 1;
							return 0;
						}
						Thread.Sleep(80);

						Port.SendStringEnd();
						Port.SendStringEnd("connect");
						for (int j = (int)(1350000L / scan_baudrates[i]) + 30; j > 1; j--)
						{
							Application.DoEvents();
							Thread.Sleep(1);
						}
						switch (getComString())
						{
							case 1:
								if (HmiOptions.FirmwareMajor != m_fwVersion[0] || HmiOptions.FirmwareMinor != m_fwVersion[1])
									MessageBox.Show("Firmware will be upgraded.".Translate());

								status = string.Concat(
											"Connected! Com:".Translate(), com,
											",baudrate:".Translate(), scan_baudrates[i], ",",
											"Model:".Translate(), m_model,
											m_isTouch ? "(With Touch)".Translate() : "(No Touch)".Translate(),
											",firmware Ver:".Translate(), "S" + m_fwVersion[0] + "." + m_fwVersion[1], ",",
											"Device serial number:".Translate(), m_flashId, ",",
											"Flash Size:".Translate(), m_flashSize,
											"(", ((int)((m_flashSize / 1024) / 1024)), " MB)\r\n"
										);

								Utility.PutXmlString(scan_baudrates[i].ToString(), "oldbo_");
								Utility.PutXmlString(com, "oldcom_");
								RunState = 1;
								return 1;

							case 2:
								status = "device firmware recovery".Translate() + "\r\n";
								RunState = 1;
								return 2;
						}
						ComClose();
						status = "Connection fail".Translate() + "\r\n";
					}
					else
					{
						ComClose();
						status = "Connection fail".Translate() + "\r\n";
						break;
					}
				}
			RunState = 1;
			return 0;
		}
		#endregion

		#region RunComOk
		public bool RunComOk(byte[] buf, Range range, ushort valBegin, ushort valEnd, CodeResults result, byte paramCount)
		{
			byte[] val = Utility.PatternBytes("comok ");
			Range laction = new Range();
			ushort pos = 0;
			ushort index = 0;
			laction.End = range.End;
			pos = Utility.IndexOfEx(buf, val, range, false);
			result.CodeResult = Range.List(paramCount);

			if (pos == 0xffff)
				return false;

			if (paramCount != 0)
			{
				byte[] comma = Utility.ToBytes(",");
				++pos;
				for (index = 0; index < paramCount; index++)
				{
					if (pos > range.End)
						return false;

					laction.Begin = pos;
					result.CodeResult[index].Begin = pos;
					pos = Utility.IndexOfEx(buf, comma, laction, true);
					if (pos == 0xffff)
					{
						if (index != (paramCount - 1))
							return false;
						result.CodeResult[index].End = range.End;
						return true;
					}
					if (pos == result.CodeResult[index].Begin)
						return false;

					result.CodeResult[index].End = (ushort)(pos - 1);
					++pos;
				}
			}
			return true;
		}
		#endregion
	}
}
