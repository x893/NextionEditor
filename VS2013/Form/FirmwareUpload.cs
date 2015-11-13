using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace NextionEditor
{
	public class FirmwareUpload : Form
	{
		private int m_sizeTFT;
		private string m_binPath;
		private SerialPort m_port;
		private ComUser m_comUser = new ComUser();
		private bool m_isStarted = false;
		private bool m_isUpdata = false;
		private bool m_isStopped = false;
		private byte[] m_bytes;
		private int m_sendsize;
		private int m_sk = 0;
		private InfoApp m_appInf;
		private int m_uptime = 0;
		
		private Button ButtonExit;
		private Button ButtonGo;
		private ComboBox BaudRate;
		private ComboBox ComPort;
		private IContainer components = null;
		private Label label2;
		private Label label4;
		private Label label7;
		private TextBox textBox2;
		private System.Windows.Forms.Timer timer1;

		public FirmwareUpload(string path)
		{
			m_binPath = path;
			InitializeComponent();
			Utility.Translate(this);
		}

		private void ButtonExit_Click(object sender, EventArgs e)
		{
			m_comUser.ComClose();
			base.Close();
		}

		private void ButtonGo_Click(object sender, EventArgs e)
		{
			ButtonGo.Enabled = false;
			ButtonExit.Enabled = false;
			ComPort.Enabled = false;
			BaudRate.Enabled = false;

			if (!m_isStarted)
			{
				if (BaudRate.Text == "")
				{
					MessageBox.Show("Please select baudrate".Translate());
					ButtonGo.Enabled = true;
				}
				else if (ComPort.Text == "")
				{
					MessageBox.Show("Please select COM Port".Translate());
					ButtonGo.Enabled = true;
				}
				else
				{
					comUpdate();
				}
			}
			else
			{
				timer1.Enabled = false;
				if (MessageBox.Show(
					"Are you sure to stop uploading files? It's not finished yet. ".Translate(),
					"Confirm".Translate(),
					MessageBoxButtons.YesNo
					) == DialogResult.Yes)
				{
					m_isStopped = true;
				}
				else
					ButtonGo.Enabled = true;

				timer1.Enabled = true;
			}
		}

		private void BaudRate_SelectedIndexChanged(object sender, EventArgs e)
		{
			uint num = uint.Parse(BaudRate.Text) / 9;
			label2.Text = string.Concat(
				"File size:".Translate(), m_sizeTFT.ToString(),
				" ",
				"Estimated upload Time:".Translate(), (m_sizeTFT / num).ToString(), "Seconds".Translate()
				);
		}

		private void comUpdate()
		{
			string str = "";
			string fwc_path = Path.Combine(Application.StartupPath, "fwc.bin");
			uint baudRate = uint.Parse(BaudRate.Text);

			m_isStopped = false;
			m_isUpdata = false;

			try
			{
				StreamReader reader;

				string status = string.Empty;
				int fwStatus = m_comUser.GetDevicePort(ref status, ComPort.Text, 0);
				textBox2.Text = status;

				if (fwStatus == 1)
				{
					m_isStarted = true;
					textBox2.AppendText(string.Concat(
						"Forced upload baudrate:".Translate(),
						baudRate,
						"  ",
						"Start uploading".Translate(),
						"\r\n"
						));
				}
				else if (fwStatus == 2)	// need download firmware
				{
					m_isStarted = true;
					textBox2.AppendText("recovery".Translate());
					textBox2.AppendText("\r\n");

					using (reader = new StreamReader(fwc_path))
					{
						m_sizeTFT = (int)reader.BaseStream.Length;
						m_bytes = new byte[m_sizeTFT];
						reader.BaseStream.Read(m_bytes, 0, m_bytes.Length);
						reader.Close();
					}
					Thread.Sleep(100);
					m_port.Write(m_bytes, (int)HmiOptions.FlashInfoFwBegin, HmiOptions.InfoFirmwareSize);
				}
				else
				{
					ButtonGo.Enabled = true;
					ButtonExit.Enabled = true;
					ComPort.Enabled = true;
					BaudRate.Enabled = true;
					return;
				}

				if (fwStatus == 1)
				{
					reader = new StreamReader(m_binPath);
					m_sizeTFT = (int)reader.BaseStream.Length;
					m_bytes = new byte[m_sizeTFT];
					reader.BaseStream.Read(m_bytes, 0, m_bytes.Length);
					reader.Close();
					Thread.Sleep(100);

					str = "tjchmi-wri " + m_sizeTFT.ToString() + "," + baudRate.ToString() + "," + m_appInf.VersionMajor.ToString();
					m_port.Write(str.ToBytes(), 0, str.Length);

					byte[] buffer = new byte[3];
					buffer[0] = 0xff;
					buffer[1] = 0xff;
					buffer[2] = 0xff;
					m_port.Write(buffer, 0, 3);

					Thread.Sleep(100);

					m_port.Close();
					m_port.BaudRate = (int)baudRate;
					m_port.Open();
				}
				m_sendsize = 0;
				m_uptime = 0;
				timer1.Enabled = true;
				ButtonGo.Enabled = true;
				ButtonGo.Text = "Stop".Translate();
				m_sk = 0;
				m_isUpdata = true;

				while (true)
				{
					Application.DoEvents();
					if (m_isStopped)
					{
						endSend();
						return;
					}

					m_sk = 0;
					while (m_sk != 5)
					{
						Application.DoEvents();
						Thread.Sleep(2);
						if (m_isStopped)
						{
							endSend();
							return;
						}

						while (m_port.BytesToRead > 0)
						{
							switch (m_port.ReadByte())
							{
								case 5:
									m_sk = 5;
									break;

								case 6:
									MessageBox.Show("Serial communication error".Translate());
									m_isStopped = true;
									endSend();
									return;
							}
						}
					}

					Thread.Sleep(5);
					if (m_port.IsOpen)
						while (m_port.BytesToRead > 0)
							if (m_port.ReadByte() == 5)
								m_sk = 5;

					m_sk = 0;
					if (m_sendsize >= m_sizeTFT)
					{
						showDown();
						m_port.Close();
						m_isUpdata = false;
						m_isStarted = false;

						textBox2.AppendText(string.Concat(
							"Download finished! Total time:".Translate(),
							m_uptime / 1000,
							" seconds".Translate(),
							"\r\n"
							));

						ButtonExit.Enabled = true;
						timer1.Enabled = false;
						ComPort.Enabled = true;
						BaudRate.Enabled = true;
						ButtonGo.Enabled = true;
						ButtonGo.Text = "GO!".Translate();
						return;
					}

					if (m_sendsize == 0)
						Thread.Sleep(200);

					if ((m_sizeTFT - m_sendsize) >= 0x1000)
					{
						m_port.Write(m_bytes, m_sendsize, 0x1000);
						m_sendsize += 0x1000;
					}
					else
					{
						m_port.Write(m_bytes, m_sendsize, m_sizeTFT - m_sendsize);
						m_sendsize = m_sizeTFT;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void endSend()
		{
			m_port.Close();
			m_isStarted = false;
			m_isUpdata = false;
			ButtonGo.Text = "GO!".Translate();
			ButtonGo.Enabled = true;
			ButtonExit.Enabled = true;
			m_sendsize = 0;
			textBox2.AppendText(string.Concat("Forced interrupt!".Translate(), "\r\n"));
			timer1.Enabled = false;
			ComPort.Enabled = true;
			BaudRate.Enabled = true;
		}

		private void getComPorts()
		{
			ComPort.Items.Clear();
			ComPort.Items.Add("Auto Search".Translate());
			string[] portNames = null;
			try
			{
				portNames = SerialPort.GetPortNames();
				foreach (string portName in portNames)
					ComPort.Items.Add(portName);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Fail to obtain COM list.".Translate() + "\r\n" + "Error information".Translate() + ex.Message);
			}
			ComPort.SelectedIndex = 0;
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
			this.components = new System.ComponentModel.Container();
			this.m_port = new System.IO.Ports.SerialPort(this.components);
			this.label2 = new System.Windows.Forms.Label();
			this.ComPort = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.BaudRate = new System.Windows.Forms.ComboBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.ButtonExit = new System.Windows.Forms.Button();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.ButtonGo = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// m_port
			// 
			this.m_port.WriteBufferSize = 204800;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(14, 90);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(43, 17);
			this.label2.TabIndex = 2;
			this.label2.Text = "label2";
			// 
			// ComPort
			// 
			this.ComPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComPort.FormattingEnabled = true;
			this.ComPort.Location = new System.Drawing.Point(96, 33);
			this.ComPort.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ComPort.Name = "ComPort";
			this.ComPort.Size = new System.Drawing.Size(121, 25);
			this.ComPort.TabIndex = 166;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(13, 36);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(77, 17);
			this.label4.TabIndex = 165;
			this.label4.Text = "Com Port:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(225, 36);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(174, 17);
			this.label7.TabIndex = 167;
			this.label7.Text = "Baud Rate:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// BaudRate
			// 
			this.BaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.BaudRate.FormattingEnabled = true;
			this.BaudRate.Location = new System.Drawing.Point(405, 33);
			this.BaudRate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.BaudRate.Name = "BaudRate";
			this.BaudRate.Size = new System.Drawing.Size(121, 25);
			this.BaudRate.TabIndex = 164;
			this.BaudRate.SelectedIndexChanged += new System.EventHandler(this.BaudRate_SelectedIndexChanged);
			// 
			// textBox2
			// 
			this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox2.Location = new System.Drawing.Point(12, 111);
			this.textBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.textBox2.Multiline = true;
			this.textBox2.Name = "textBox2";
			this.textBox2.ReadOnly = true;
			this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBox2.Size = new System.Drawing.Size(661, 215);
			this.textBox2.TabIndex = 168;
			// 
			// ButtonExit
			// 
			this.ButtonExit.Location = new System.Drawing.Point(578, 335);
			this.ButtonExit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ButtonExit.Name = "ButtonExit";
			this.ButtonExit.Size = new System.Drawing.Size(90, 47);
			this.ButtonExit.TabIndex = 169;
			this.ButtonExit.Text = "Exit";
			this.ButtonExit.UseVisualStyleBackColor = true;
			this.ButtonExit.Click += new System.EventHandler(this.ButtonExit_Click);
			// 
			// timer1
			// 
			this.timer1.Interval = 1000;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// ButtonGo
			// 
			this.ButtonGo.Location = new System.Drawing.Point(345, 335);
			this.ButtonGo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ButtonGo.Name = "ButtonGo";
			this.ButtonGo.Size = new System.Drawing.Size(227, 47);
			this.ButtonGo.TabIndex = 170;
			this.ButtonGo.Text = "Go";
			this.ButtonGo.UseVisualStyleBackColor = true;
			this.ButtonGo.Click += new System.EventHandler(this.ButtonGo_Click);
			// 
			// FirmwareUpload
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(680, 398);
			this.Controls.Add(this.ButtonGo);
			this.Controls.Add(this.ButtonExit);
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.ComPort);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.BaudRate);
			this.Controls.Add(this.label2);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.Name = "FirmwareUpload";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Upload to Nextion Device";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.USARTUP_FormClosing);
			this.Load += new System.EventHandler(this.USARTUP_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private void showDown()
		{
			long num = 0L;
			m_uptime += timer1.Interval;
			num = (m_sendsize * 1000) / m_uptime;
			if (num > 0L)
			{
				string[] strArray = new string[] {
					"File size:".Translate(), m_sizeTFT.ToString(), " ",
					"Uploaded:".Translate(), m_sendsize.ToString(), " ",
					"Speed:".Translate(), num.ToString(), " ",
					"Estimated time remaining:".Translate(),
					(((long)(m_sizeTFT - m_sendsize)) / num).ToString(), " ", "Seconds".Translate() };
				label2.Text = string.Concat(strArray);
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			if (m_isUpdata)
			{
				showDown();
			}
		}

		private void USARTUP_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!(!m_isStarted && ButtonGo.Enabled))
				e.Cancel = true;
		}

		private void USARTUP_Load(object sender, EventArgs e)
		{
			using (StreamReader reader = new StreamReader(m_binPath))
			{
				m_sizeTFT = (int)reader.BaseStream.Length;
				byte[] buffer = new byte[HmiOptions.InfoAppSize];
				if (reader.BaseStream.Length < (buffer.Length + 3))
				{
					MessageBox.Show("Wrong resource file or resource file has been damaged".Translate());
					reader.Close();
					reader.Dispose();
					base.Close();
				}

				reader.BaseStream.Read(buffer, 0, buffer.Length);
				m_appInf = Utility.ToStruct<InfoApp>(buffer);
				reader.Close();

				if (m_appInf.FileType != Utility.FILE_TYPE_CN_TFT
				 && m_appInf.FileType != Utility.FILE_TYPE_EN_TFT
					)
				{
					MessageBox.Show("Wrong resource file or resource file has been damaged".Translate());
					base.Close();
				}
			}

			BaudRate.Items.Clear();
			BaudRate.Items.Add("2400");
			BaudRate.Items.Add("4800");
			BaudRate.Items.Add("9600");
			BaudRate.Items.Add("19200");
			BaudRate.Items.Add("38400");
			BaudRate.Items.Add("57600");
			BaudRate.Items.Add("115200");
			BaudRate.SelectedIndex = BaudRate.Items.Count - 1;
			// BaudRate.Text = "115200";

			getComPorts();

			m_comUser.Port = m_port;
		}
	}
}
