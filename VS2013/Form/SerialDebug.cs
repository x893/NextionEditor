using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NextionEditor.Properties;

namespace NextionEditor
{
	public class SerialDebug : Form
	{
		#region Variables
		private string m_binPath;
		private bool m_showXY = true;
		private bool m_enableMcuReceive = false;
		private int m_ddx = 0;
		private int m_counter_0xFF = 0;
		private string m_receivedMcuData;
		private string m_receivedSimData;
		private int m_ms_lastSimReceive = 0;
		private int m_ms_lastMcuReceive = 0;
		private int ms_interval = 0;
		private int m_stopSend = 0;
		private int m_curveOffset = 0;
		private int m_curveAmplitude = 0;
		private bool m_curveSendCom = false;
		private bool m_curveSendSim = false;
		private byte[] m_channelId = new byte[4];
		private ComUser m_comMcu = new ComUser();

		private Button btnClearLog;
		private Button btnSend;
		private ToolStripButton mi_Connect;
		private CheckBox cbPressEnter;
		private CheckBox cbRandom;
		private SerialPort m_com;
		private ToolStripComboBox SendTo;
		private ComboBox cbComPorts;
		private ToolStripComboBox SendToCom;
		private ComboBox cbBaudRates;
		private IContainer components = null;
		private GroupBox groupBox1;
		private Label label1;
		private Label label10;
		private Label label11;
		private Label label12;
		private Label label14;
		private Label label15;
		private Label label2;
		private Label label3;
		private Label label6;
		private Label label7;
		private Label label8;
		private Label label9;
		private Label lbl_SimParse;
		private Label lbl_McuParse;
		private LinkLabel link_RunAll;
		private LinkLabel link_SimClear;
		private LinkLabel link_McuClear;
		private LinkLabel link_Start;
		private LinkLabel linkWaveform;
		private ListBox lb_SimResponses;
		private ListBox lb_McuResponses;
		private ListBox lb_Log;
		private Panel panelDisplay;
		private Panel panel2;
		private RadioButton rbKeyboardInput;
		private RadioButton rbMcuInput;
		private HmiRunScreen ucRunScreen;
		private StatusStrip statusStrip;
		private TextBox tbManualCommand;
		private TextBox tbInterval;
		private TextBox tbChannel;
		private TextBox tbComponentId;
		private TextBox tbMinValue;
		private TextBox tbMaxValue;
		private Thread m_send_thread;
		private System.Windows.Forms.Timer timerMcuRead;
		private System.Windows.Forms.Timer TimerCom;
		private ToolStrip toolStrip;
		private ToolStripButton mi_Upload;
		private ToolStripLabel mi_SendCommandTo;
		private ToolStripButton mi_XY;
		private ToolStripLabel lbl_ComPort;
		private ToolStripStatusLabel lbl_statusText;
		#endregion

		#region Constructor
		public SerialDebug(string binPath)
		{
			m_binPath = binPath;
			InitializeComponent();
			Utility.Translate(this);

			ucRunScreen.GuiInit(m_binPath, null, false);
		}
		#endregion

		#region SerialDebug_Resize
		private void SerialDebug_Resize(object sender, EventArgs e)
		{
			try
			{
				int x_center = ((base.Width - label1.Left) - 10) / 2;
				label3.Left = label1.Left + x_center;
				lb_McuResponses.Left = label3.Left;
				lbl_McuParse.Left = label3.Left;
				lb_SimResponses.Width = x_center - 10;
				lb_McuResponses.Width = x_center - 10;
				link_SimClear.Left = (lb_SimResponses.Left + lb_SimResponses.Width) - link_SimClear.Width;
				link_McuClear.Left = (lb_McuResponses.Left + lb_McuResponses.Width) - link_McuClear.Width;
			}
			catch { }
		}
		#endregion

		#region btnClearLog_Click
		private void btnClearLog_Click(object sender, EventArgs e)
		{
			lb_Log.Items.Clear();
		}
		#endregion

		#region btnSend_Click
		private void btnSend_Click(object sender, EventArgs e)
		{
			if (btnSend.Text == "Send".Translate())
			{
				setChannel();
				if (m_channelId[0] == 0xff && m_channelId[1] == 0xff && m_channelId[2] == 0xff && m_channelId[3] == 0xff)
					MessageBox.Show("Please input correct Channel ID.".Translate());
				else
				{
					m_curveSendSim = false;
					m_curveSendCom = false;

					if (SendTo.SelectedIndex == 0 || SendTo.SelectedIndex == 2)
						m_curveSendSim = true;
					if (SendTo.SelectedIndex == 1 || SendTo.SelectedIndex == 2)
						m_curveSendCom = true;

					m_curveAmplitude = (int.Parse(tbMaxValue.Text) - int.Parse(tbMinValue.Text)) / 2;
					m_curveAmplitude *= 2;
					m_curveOffset = int.Parse(tbMinValue.Text) + (m_curveAmplitude / 2);

					if ((m_curveAmplitude / 2 + m_curveOffset) > 0xff)
						MessageBox.Show("Out-of-Range".Translate());
					else
					{
						btnSend.Text = "Stop".Translate();
						tbInterval.Enabled = false;
						tbChannel.Enabled = false;
						tbComponentId.Enabled = false;
						tbMinValue.Enabled = false;
						tbMaxValue.Enabled = false;
						mi_Connect.Enabled = false;
						SendTo.Enabled = false;
						SendToCom.Enabled = false;

						m_stopSend = 1;
						m_send_thread = new Thread(new ThreadStart(sendProcess));
						m_send_thread.Start();
					}
				}
			}
			else
				stopSend();
		}
		#endregion

		#region closeLink
		private void closeLink()
		{
			m_comMcu.ComClose();
			timerMcuRead.Enabled = false;
			mi_Connect.Text = "Connect".Translate();
			lbl_statusText.Text = "State:".Translate() + "Disconnected".Translate();
		}
		#endregion

		#region SendTo_SelectedIndexChanged
		private void SendTo_SelectedIndexChanged(object sender, EventArgs e)
		{
			rbMcuInput.Enabled = SendTo.SelectedIndex == 0;

			if (SendTo.SelectedIndex != 1 && SendTo.SelectedIndex != 2)
			{
				closeLink();
				SendToCom.Visible = false;
				mi_Connect.Visible = false;
				lbl_ComPort.Visible = false;
				lb_McuResponses.BackColor = BackColor;
				lb_SimResponses.BackColor = Color.White;
			}
			else
			{
				if (SendTo.SelectedIndex == 1)
					lb_SimResponses.BackColor = BackColor;
				else
					lb_SimResponses.BackColor = Color.White;

				lb_McuResponses.BackColor = Color.White;
				lbl_ComPort.Visible = true;
				SendToCom.Visible = true;
				mi_Connect.Visible = true;
			}
		}
		#endregion

		#region comClose
		private void comClose()
		{
			try
			{
				if (m_com != null && m_com.IsOpen)
					m_com.Close();
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
				if (m_com != null)
				{
					m_com.BaudRate = baurate;
					m_com.PortName = portname;
					m_com.Open();
					flag = true;
				}
			}
			catch (Exception ex)
			{
				flag = false;
				MessageBox.Show(ex.Message);
			}
			return flag;
		}
		#endregion

		#region linkStart_LinkClicked
		private void linkStart_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (!m_com.IsOpen)
			{
				if (comOpen(cbComPorts.Text, Utility.GetInt(cbBaudRates.Text)))
				{
					lb_McuResponses.Items.Clear();
					tbManualCommand.Text = "";
					link_Start.Text = "Stop".Translate();
					cbComPorts.Enabled = false;
					cbBaudRates.Enabled = false;
					timerMcuRead.Enabled = true;
				}
			}
			else
			{
				timerMcuRead.Enabled = false;
				comClose();
				link_Start.Text = "Start".Translate();
				cbComPorts.Enabled = true;
				cbBaudRates.Enabled = true;
			}
		}
		#endregion

		#region getPorts
		private void getPorts()
		{
			cbComPorts.Items.Clear();
			SendToCom.Items.Clear();
			SendToCom.Items.Add("Auto Search".Translate());
			string[] portNames = null;
			try
			{
				portNames = SerialPort.GetPortNames();
				foreach (string str in portNames)
				{
					SendToCom.Items.Add(str);
					cbComPorts.Items.Add(str);
				}
				SendToCom.SelectedIndex = 0;
				if (cbComPorts.Items.Count > 0)
					cbComPorts.SelectedIndex = 0;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Obtain COM list failed!\r\n error message:".Translate() + ex.Message);
			}
		}
		#endregion

		#region setChannel
		private void setChannel()
		{
			try
			{
				m_channelId[0] = 0xff;
				m_channelId[1] = 0xff;
				m_channelId[2] = 0xff;
				m_channelId[3] = 0xff;
				string[] digits = tbChannel.Text.Split(Utility.CHAR_COMMA);
				if (digits.Length < 5)
					for (int i = 0; i < digits.Length; i++)
						m_channelId[i] = byte.Parse(digits[i]);
			}
			catch
			{
				m_channelId[0] = 0xff;
				m_channelId[1] = 0xff;
				m_channelId[2] = 0xff;
				m_channelId[3] = 0xff;
			}
		}
		#endregion

		#region InitializeComponent
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SerialDebug));
			this.tbManualCommand = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.cbPressEnter = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.m_com = new System.IO.Ports.SerialPort(this.components);
			this.timerMcuRead = new System.Windows.Forms.Timer(this.components);
			this.toolStrip = new System.Windows.Forms.ToolStrip();
			this.mi_Upload = new System.Windows.Forms.ToolStripButton();
			this.mi_SendCommandTo = new System.Windows.Forms.ToolStripLabel();
			this.SendTo = new System.Windows.Forms.ToolStripComboBox();
			this.lbl_ComPort = new System.Windows.Forms.ToolStripLabel();
			this.SendToCom = new System.Windows.Forms.ToolStripComboBox();
			this.mi_Connect = new System.Windows.Forms.ToolStripButton();
			this.mi_XY = new System.Windows.Forms.ToolStripButton();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.lbl_statusText = new System.Windows.Forms.ToolStripStatusLabel();
			this.link_RunAll = new System.Windows.Forms.LinkLabel();
			this.link_SimClear = new System.Windows.Forms.LinkLabel();
			this.lb_SimResponses = new System.Windows.Forms.ListBox();
			this.lb_McuResponses = new System.Windows.Forms.ListBox();
			this.lbl_SimParse = new System.Windows.Forms.Label();
			this.lbl_McuParse = new System.Windows.Forms.Label();
			this.link_McuClear = new System.Windows.Forms.LinkLabel();
			this.lb_Log = new System.Windows.Forms.ListBox();
			this.btnClearLog = new System.Windows.Forms.Button();
			this.panelDisplay = new System.Windows.Forms.Panel();
			this.ucRunScreen = new NextionEditor.HmiRunScreen();
			this.rbKeyboardInput = new System.Windows.Forms.RadioButton();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.rbMcuInput = new System.Windows.Forms.RadioButton();
			this.label7 = new System.Windows.Forms.Label();
			this.cbComPorts = new System.Windows.Forms.ComboBox();
			this.cbBaudRates = new System.Windows.Forms.ComboBox();
			this.label8 = new System.Windows.Forms.Label();
			this.link_Start = new System.Windows.Forms.LinkLabel();
			this.btnSend = new System.Windows.Forms.Button();
			this.label9 = new System.Windows.Forms.Label();
			this.tbInterval = new System.Windows.Forms.TextBox();
			this.linkWaveform = new System.Windows.Forms.LinkLabel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.label12 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.tbMaxValue = new System.Windows.Forms.TextBox();
			this.label14 = new System.Windows.Forms.Label();
			this.tbMinValue = new System.Windows.Forms.TextBox();
			this.cbRandom = new System.Windows.Forms.CheckBox();
			this.label11 = new System.Windows.Forms.Label();
			this.tbComponentId = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.tbChannel = new System.Windows.Forms.TextBox();
			this.TimerCom = new System.Windows.Forms.Timer(this.components);
			this.toolStrip.SuspendLayout();
			this.statusStrip.SuspendLayout();
			this.panelDisplay.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// tbManualCommand
			// 
			this.tbManualCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.tbManualCommand.BackColor = System.Drawing.Color.White;
			this.tbManualCommand.ForeColor = System.Drawing.Color.Black;
			this.tbManualCommand.Location = new System.Drawing.Point(4, 594);
			this.tbManualCommand.Multiline = true;
			this.tbManualCommand.Name = "tbManualCommand";
			this.tbManualCommand.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tbManualCommand.Size = new System.Drawing.Size(444, 98);
			this.tbManualCommand.TabIndex = 1;
			this.tbManualCommand.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbManualCommand_KeyPress);
			// 
			// label6
			// 
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 570);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(128, 17);
			this.label6.TabIndex = 150;
			this.label6.Text = "Istruction Input Area:";
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(469, 570);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(139, 17);
			this.label1.TabIndex = 154;
			this.label1.Text = "Simulator Return Data:";
			// 
			// cbPressEnter
			// 
			this.cbPressEnter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cbPressEnter.AutoSize = true;
			this.cbPressEnter.Checked = true;
			this.cbPressEnter.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbPressEnter.Location = new System.Drawing.Point(10, 698);
			this.cbPressEnter.Name = "cbPressEnter";
			this.cbPressEnter.Size = new System.Drawing.Size(239, 21);
			this.cbPressEnter.TabIndex = 156;
			this.cbPressEnter.Text = "Press Enter to run the last command";
			this.cbPressEnter.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.AutoSize = true;
			this.label2.ForeColor = System.Drawing.Color.Black;
			this.label2.Location = new System.Drawing.Point(254, 570);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(194, 17);
			this.label2.TabIndex = 157;
			this.label2.Text = "Note: One command in one line";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(742, 570);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(152, 17);
			this.label3.TabIndex = 159;
			this.label3.Text = "User MCU returned data";
			// 
			// m_com
			// 
			this.m_com.WriteBufferSize = 204800;
			// 
			// timerMcuRead
			// 
			this.timerMcuRead.Tick += new System.EventHandler(this.timerMcuRead_Tick);
			// 
			// toolStrip
			// 
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mi_Upload,
            this.mi_SendCommandTo,
            this.SendTo,
            this.lbl_ComPort,
            this.SendToCom,
            this.mi_Connect,
            this.mi_XY});
			this.toolStrip.Location = new System.Drawing.Point(0, 0);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = new System.Drawing.Size(1016, 29);
			this.toolStrip.TabIndex = 168;
			this.toolStrip.Text = "toolStrip1";
			// 
			// mi_Upload
			// 
			this.mi_Upload.Image = ((System.Drawing.Image)(resources.GetObject("mi_Upload.Image")));
			this.mi_Upload.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.mi_Upload.Name = "mi_Upload";
			this.mi_Upload.Size = new System.Drawing.Size(156, 26);
			this.mi_Upload.Text = "Upload to Nextion";
			this.mi_Upload.Click += new System.EventHandler(this.miUpload_Click);
			// 
			// mi_SendCommandTo
			// 
			this.mi_SendCommandTo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.mi_SendCommandTo.Image = ((System.Drawing.Image)(resources.GetObject("mi_SendCommandTo.Image")));
			this.mi_SendCommandTo.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.mi_SendCommandTo.Name = "mi_SendCommandTo";
			this.mi_SendCommandTo.Size = new System.Drawing.Size(140, 26);
			this.mi_SendCommandTo.Text = "Send command to:";
			// 
			// SendTo
			// 
			this.SendTo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.SendTo.Name = "SendTo";
			this.SendTo.Size = new System.Drawing.Size(190, 29);
			this.SendTo.SelectedIndexChanged += new System.EventHandler(this.SendTo_SelectedIndexChanged);
			// 
			// lbl_ComPort
			// 
			this.lbl_ComPort.Name = "lbl_ComPort";
			this.lbl_ComPort.Size = new System.Drawing.Size(78, 26);
			this.lbl_ComPort.Text = "Com Port:";
			// 
			// SendToCom
			// 
			this.SendToCom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.SendToCom.Name = "SendToCom";
			this.SendToCom.Size = new System.Drawing.Size(121, 29);
			// 
			// mi_Connect
			// 
			this.mi_Connect.Image = global::NextionEditor.Properties.Resources.glzy8_com_148;
			this.mi_Connect.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.mi_Connect.Name = "mi_Connect";
			this.mi_Connect.Size = new System.Drawing.Size(87, 26);
			this.mi_Connect.Text = "Connect";
			this.mi_Connect.Click += new System.EventHandler(this.miConnect_Click);
			// 
			// mi_XY
			// 
			this.mi_XY.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.mi_XY.Image = ((System.Drawing.Image)(resources.GetObject("mi_XY.Image")));
			this.mi_XY.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.mi_XY.Name = "mi_XY";
			this.mi_XY.Size = new System.Drawing.Size(32, 26);
			this.mi_XY.Text = "XY";
			this.mi_XY.Click += new System.EventHandler(this.miXY_Click);
			// 
			// statusStrip
			// 
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lbl_statusText});
			this.statusStrip.Location = new System.Drawing.Point(0, 766);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Size = new System.Drawing.Size(1016, 26);
			this.statusStrip.TabIndex = 169;
			this.statusStrip.Text = "statusStrip1";
			// 
			// lbl_statusText
			// 
			this.lbl_statusText.Name = "lbl_statusText";
			this.lbl_statusText.Size = new System.Drawing.Size(1001, 21);
			this.lbl_statusText.Spring = true;
			this.lbl_statusText.Text = "Nextion state: Disconnected";
			this.lbl_statusText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// link_RunAll
			// 
			this.link_RunAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.link_RunAll.Location = new System.Drawing.Point(332, 705);
			this.link_RunAll.Name = "link_RunAll";
			this.link_RunAll.Size = new System.Drawing.Size(119, 17);
			this.link_RunAll.TabIndex = 171;
			this.link_RunAll.TabStop = true;
			this.link_RunAll.Text = "Run all commands";
			this.link_RunAll.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.link_RunAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkRunAll_LinkClicked);
			// 
			// link_SimClear
			// 
			this.link_SimClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.link_SimClear.AutoSize = true;
			this.link_SimClear.Location = new System.Drawing.Point(698, 570);
			this.link_SimClear.Name = "link_SimClear";
			this.link_SimClear.Size = new System.Drawing.Size(38, 17);
			this.link_SimClear.TabIndex = 172;
			this.link_SimClear.TabStop = true;
			this.link_SimClear.Text = "Clear";
			this.link_SimClear.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkSimRespClear_LinkClicked);
			// 
			// lb_SimResponses
			// 
			this.lb_SimResponses.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lb_SimResponses.FormattingEnabled = true;
			this.lb_SimResponses.HorizontalScrollbar = true;
			this.lb_SimResponses.ItemHeight = 17;
			this.lb_SimResponses.Location = new System.Drawing.Point(471, 594);
			this.lb_SimResponses.Name = "lb_SimResponses";
			this.lb_SimResponses.Size = new System.Drawing.Size(265, 89);
			this.lb_SimResponses.TabIndex = 173;
			this.lb_SimResponses.SelectedIndexChanged += new System.EventHandler(this.listSimResponses_SelectedIndexChanged);
			this.lb_SimResponses.DoubleClick += new System.EventHandler(this.listSimResponses_DoubleClick);
			// 
			// lb_McuResponses
			// 
			this.lb_McuResponses.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lb_McuResponses.FormattingEnabled = true;
			this.lb_McuResponses.HorizontalScrollbar = true;
			this.lb_McuResponses.ItemHeight = 17;
			this.lb_McuResponses.Location = new System.Drawing.Point(742, 594);
			this.lb_McuResponses.Name = "lb_McuResponses";
			this.lb_McuResponses.Size = new System.Drawing.Size(265, 89);
			this.lb_McuResponses.TabIndex = 174;
			this.lb_McuResponses.SelectedIndexChanged += new System.EventHandler(this.listMcuResponses_SelectedIndexChanged);
			this.lb_McuResponses.DoubleClick += new System.EventHandler(this.listMcuResponses_DoubleClick);
			// 
			// lbl_SimParse
			// 
			this.lbl_SimParse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lbl_SimParse.Location = new System.Drawing.Point(469, 690);
			this.lbl_SimParse.Name = "lbl_SimParse";
			this.lbl_SimParse.Size = new System.Drawing.Size(267, 31);
			this.lbl_SimParse.TabIndex = 175;
			this.lbl_SimParse.Text = "Parse:";
			// 
			// lbl_McuParse
			// 
			this.lbl_McuParse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lbl_McuParse.Location = new System.Drawing.Point(742, 690);
			this.lbl_McuParse.Name = "lbl_McuParse";
			this.lbl_McuParse.Size = new System.Drawing.Size(265, 31);
			this.lbl_McuParse.TabIndex = 176;
			this.lbl_McuParse.Text = "Parse:";
			// 
			// link_McuClear
			// 
			this.link_McuClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.link_McuClear.AutoSize = true;
			this.link_McuClear.Location = new System.Drawing.Point(966, 570);
			this.link_McuClear.Name = "link_McuClear";
			this.link_McuClear.Size = new System.Drawing.Size(38, 17);
			this.link_McuClear.TabIndex = 177;
			this.link_McuClear.TabStop = true;
			this.link_McuClear.Text = "Clear";
			this.link_McuClear.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.McuRespClear_LinkClicked);
			// 
			// lb_Log
			// 
			this.lb_Log.FormattingEnabled = true;
			this.lb_Log.ItemHeight = 17;
			this.lb_Log.Location = new System.Drawing.Point(653, 2);
			this.lb_Log.Name = "lb_Log";
			this.lb_Log.Size = new System.Drawing.Size(352, 123);
			this.lb_Log.TabIndex = 178;
			this.lb_Log.Visible = false;
			// 
			// btnClearLog
			// 
			this.btnClearLog.Location = new System.Drawing.Point(895, 102);
			this.btnClearLog.Name = "btnClearLog";
			this.btnClearLog.Size = new System.Drawing.Size(110, 35);
			this.btnClearLog.TabIndex = 179;
			this.btnClearLog.Text = "button1";
			this.btnClearLog.UseVisualStyleBackColor = true;
			this.btnClearLog.Visible = false;
			this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
			// 
			// panelDisplay
			// 
			this.panelDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelDisplay.AutoScroll = true;
			this.panelDisplay.BackColor = System.Drawing.Color.Gray;
			this.panelDisplay.Controls.Add(this.btnClearLog);
			this.panelDisplay.Controls.Add(this.lb_Log);
			this.panelDisplay.Controls.Add(this.ucRunScreen);
			this.panelDisplay.Location = new System.Drawing.Point(0, 30);
			this.panelDisplay.Name = "panelDisplay";
			this.panelDisplay.Size = new System.Drawing.Size(1016, 531);
			this.panelDisplay.TabIndex = 180;
			this.panelDisplay.Paint += new System.Windows.Forms.PaintEventHandler(this.panelDisplay_Paint);
			this.panelDisplay.Resize += new System.EventHandler(this.panelDisplay_Resize);
			// 
			// ucRunScreen
			// 
			this.ucRunScreen.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.ucRunScreen.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.ucRunScreen.Location = new System.Drawing.Point(50, 62);
			this.ucRunScreen.Name = "ucRunScreen";
			this.ucRunScreen.Size = new System.Drawing.Size(100, 100);
			this.ucRunScreen.TabIndex = 0;
			this.ucRunScreen.SendByte += new System.EventHandler(this.ucRunScreen_SendByte);
			this.ucRunScreen.SendRunCode += new System.EventHandler(this.ucRunScreen_SendRunCode);
			this.ucRunScreen.Resize += new System.EventHandler(this.ucRunScreen_Resize);
			// 
			// rbKeyboardInput
			// 
			this.rbKeyboardInput.AutoSize = true;
			this.rbKeyboardInput.Checked = true;
			this.rbKeyboardInput.Location = new System.Drawing.Point(6, 15);
			this.rbKeyboardInput.Name = "rbKeyboardInput";
			this.rbKeyboardInput.Size = new System.Drawing.Size(116, 21);
			this.rbKeyboardInput.TabIndex = 181;
			this.rbKeyboardInput.TabStop = true;
			this.rbKeyboardInput.Text = "Keyboard Input";
			this.rbKeyboardInput.UseVisualStyleBackColor = true;
			this.rbKeyboardInput.CheckedChanged += new System.EventHandler(this.rbKeyboardMcyInput_CheckedChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.AutoSize = true;
			this.groupBox1.Controls.Add(this.rbMcuInput);
			this.groupBox1.Controls.Add(this.rbKeyboardInput);
			this.groupBox1.Location = new System.Drawing.Point(4, 715);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(445, 60);
			this.groupBox1.TabIndex = 182;
			this.groupBox1.TabStop = false;
			// 
			// rbMcuInput
			// 
			this.rbMcuInput.AutoSize = true;
			this.rbMcuInput.Location = new System.Drawing.Point(258, 15);
			this.rbMcuInput.Name = "rbMcuInput";
			this.rbMcuInput.Size = new System.Drawing.Size(119, 21);
			this.rbMcuInput.TabIndex = 182;
			this.rbMcuInput.Text = "User MCU Input";
			this.rbMcuInput.UseVisualStyleBackColor = true;
			this.rbMcuInput.CheckedChanged += new System.EventHandler(this.rbKeyboardMcyInput_CheckedChanged);
			// 
			// label7
			// 
			this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(543, 733);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(63, 17);
			this.label7.TabIndex = 183;
			this.label7.Text = "Com Port";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label7.Visible = false;
			// 
			// cbComPorts
			// 
			this.cbComPorts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cbComPorts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbComPorts.FormattingEnabled = true;
			this.cbComPorts.Location = new System.Drawing.Point(608, 730);
			this.cbComPorts.Name = "cbComPorts";
			this.cbComPorts.Size = new System.Drawing.Size(85, 25);
			this.cbComPorts.TabIndex = 184;
			this.cbComPorts.Visible = false;
			// 
			// cbBaudRates
			// 
			this.cbBaudRates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cbBaudRates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbBaudRates.FormattingEnabled = true;
			this.cbBaudRates.Location = new System.Drawing.Point(742, 730);
			this.cbBaudRates.Name = "cbBaudRates";
			this.cbBaudRates.Size = new System.Drawing.Size(83, 25);
			this.cbBaudRates.TabIndex = 186;
			this.cbBaudRates.Visible = false;
			// 
			// label8
			// 
			this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(699, 733);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(37, 17);
			this.label8.TabIndex = 185;
			this.label8.Text = "Baud";
			this.label8.Visible = false;
			// 
			// link_Start
			// 
			this.link_Start.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.link_Start.AutoSize = true;
			this.link_Start.Location = new System.Drawing.Point(831, 731);
			this.link_Start.Name = "link_Start";
			this.link_Start.Size = new System.Drawing.Size(35, 17);
			this.link_Start.TabIndex = 187;
			this.link_Start.TabStop = true;
			this.link_Start.Text = "Start";
			this.link_Start.Visible = false;
			this.link_Start.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkStart_LinkClicked);
			// 
			// btnSend
			// 
			this.btnSend.Location = new System.Drawing.Point(356, 48);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(81, 27);
			this.btnSend.TabIndex = 188;
			this.btnSend.Text = "Send";
			this.btnSend.UseVisualStyleBackColor = true;
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(277, 14);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(75, 17);
			this.label9.TabIndex = 189;
			this.label9.Text = "Interval(ms)";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tbInterval
			// 
			this.tbInterval.Location = new System.Drawing.Point(399, 11);
			this.tbInterval.Name = "tbInterval";
			this.tbInterval.Size = new System.Drawing.Size(38, 25);
			this.tbInterval.TabIndex = 190;
			this.tbInterval.Text = "0";
			this.tbInterval.TextChanged += new System.EventHandler(this.tbInterval_TextChanged);
			// 
			// linkWaveform
			// 
			this.linkWaveform.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.linkWaveform.AutoSize = true;
			this.linkWaveform.Location = new System.Drawing.Point(877, 732);
			this.linkWaveform.Name = "linkWaveform";
			this.linkWaveform.Size = new System.Drawing.Size(128, 17);
			this.linkWaveform.TabIndex = 188;
			this.linkWaveform.TabStop = true;
			this.linkWaveform.Text = "Waveform Gererator";
			this.linkWaveform.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.linkWaveform.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.WaveformGererator_LinkClicked);
			// 
			// panel2
			// 
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.panel2.Controls.Add(this.label12);
			this.panel2.Controls.Add(this.label15);
			this.panel2.Controls.Add(this.tbMaxValue);
			this.panel2.Controls.Add(this.label14);
			this.panel2.Controls.Add(this.tbMinValue);
			this.panel2.Controls.Add(this.cbRandom);
			this.panel2.Controls.Add(this.label11);
			this.panel2.Controls.Add(this.tbComponentId);
			this.panel2.Controls.Add(this.label10);
			this.panel2.Controls.Add(this.tbChannel);
			this.panel2.Controls.Add(this.label9);
			this.panel2.Controls.Add(this.tbInterval);
			this.panel2.Controls.Add(this.btnSend);
			this.panel2.Location = new System.Drawing.Point(4, 594);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(445, 130);
			this.panel2.TabIndex = 189;
			this.panel2.Visible = false;
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.label12.Location = new System.Drawing.Point(6, 78);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(385, 17);
			this.label12.TabIndex = 204;
			this.label12.Text = "Mulit-channel generator. ID argument is a comma-separated list.";
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(156, 53);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(33, 17);
			this.label15.TabIndex = 202;
			this.label15.Text = "Max";
			this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tbMaxValue
			// 
			this.tbMaxValue.Location = new System.Drawing.Point(213, 52);
			this.tbMaxValue.Name = "tbMaxValue";
			this.tbMaxValue.Size = new System.Drawing.Size(58, 25);
			this.tbMaxValue.TabIndex = 203;
			this.tbMaxValue.Text = "100";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(49, 55);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(30, 17);
			this.label14.TabIndex = 200;
			this.label14.Text = "Min";
			this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tbMinValue
			// 
			this.tbMinValue.Location = new System.Drawing.Point(113, 52);
			this.tbMinValue.Name = "tbMinValue";
			this.tbMinValue.Size = new System.Drawing.Size(37, 25);
			this.tbMinValue.TabIndex = 201;
			this.tbMinValue.Text = "0";
			// 
			// cbRandom
			// 
			this.cbRandom.AutoSize = true;
			this.cbRandom.Location = new System.Drawing.Point(280, 54);
			this.cbRandom.Name = "cbRandom";
			this.cbRandom.Size = new System.Drawing.Size(76, 21);
			this.cbRandom.TabIndex = 199;
			this.cbRandom.Text = "Random";
			this.cbRandom.UseVisualStyleBackColor = true;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(15, 11);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(92, 17);
			this.label11.TabIndex = 193;
			this.label11.Text = "Component ID";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tbComponentId
			// 
			this.tbComponentId.Location = new System.Drawing.Point(113, 11);
			this.tbComponentId.Name = "tbComponentId";
			this.tbComponentId.Size = new System.Drawing.Size(37, 25);
			this.tbComponentId.TabIndex = 194;
			this.tbComponentId.Text = "1";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(156, 14);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(54, 17);
			this.label10.TabIndex = 191;
			this.label10.Text = "Channel";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tbChannel
			// 
			this.tbChannel.Location = new System.Drawing.Point(213, 11);
			this.tbChannel.Name = "tbChannel";
			this.tbChannel.Size = new System.Drawing.Size(58, 25);
			this.tbChannel.TabIndex = 192;
			this.tbChannel.Text = "0";
			this.tbChannel.TextChanged += new System.EventHandler(this.tbChannel_TextChanged);
			// 
			// TimerCom
			// 
			this.TimerCom.Enabled = true;
			this.TimerCom.Interval = 30;
			this.TimerCom.Tick += new System.EventHandler(this.timerSimMcuLog_Tick);
			// 
			// SerialDebug
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1016, 792);
			this.Controls.Add(this.cbPressEnter);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.linkWaveform);
			this.Controls.Add(this.link_Start);
			this.Controls.Add(this.cbBaudRates);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.cbComPorts);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.panelDisplay);
			this.Controls.Add(this.link_McuClear);
			this.Controls.Add(this.lbl_McuParse);
			this.Controls.Add(this.lbl_SimParse);
			this.Controls.Add(this.lb_McuResponses);
			this.Controls.Add(this.lb_SimResponses);
			this.Controls.Add(this.link_SimClear);
			this.Controls.Add(this.link_RunAll);
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.tbManualCommand);
			this.Controls.Add(this.statusStrip);
			this.Controls.Add(this.groupBox1);
			this.DoubleBuffered = true;
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Name = "SerialDebug";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Serial Debug";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SerialDebug_FormClosing);
			this.Load += new System.EventHandler(this.SerialDebug_Load);
			this.Resize += new System.EventHandler(this.SerialDebug_Resize);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.panelDisplay.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		#region linkRunAll_LinkClicked
		private void linkRunAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			sendAll();
		}
		#endregion

		#region linkSimRespClear_LinkClicked
		private void linkSimRespClear_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			lb_SimResponses.Items.Clear();
		}
		#endregion

		#region McuRespClear_LinkClicked
		private void McuRespClear_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			lb_McuResponses.Items.Clear();
		}
		#endregion

		#region WaveformGererator_LinkClicked
		private void WaveformGererator_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (!panel2.Visible)
				panel2.Visible = true;
			else
			{
				stopSend();
				panel2.Visible = false;
			}
		}
		#endregion

		#region listSimResponses_DoubleClick
		private void listSimResponses_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				Clipboard.SetDataObject(lb_SimResponses.SelectedItem.ToString());
			}
			catch { }
		}
		#endregion

		#region showError
		private void showError(Label label, string response)
		{
			label.Text = "Meaning:".Translate() + Utility.GetErrorText(response);

			int num = Convert.ToInt32(response.Split(Utility.CHAR_SPACE)[0], 16);
			if (num == 0 || (num > 1 && num < 0x65))
				label.ForeColor = Color.Red;
			else
				label.ForeColor = Color.Black;
		}
		#endregion

		#region listSimResponses_SelectedIndexChanged
		private void listSimResponses_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (lb_SimResponses.Items.Count > 0 && lb_SimResponses.SelectedIndex >= 0)
				showError(lbl_SimParse, lb_SimResponses.SelectedItem.ToString());
		}
		#endregion

		#region listMcuResponses_SelectedIndexChanged
		private void listMcuResponses_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (lb_McuResponses.Items.Count > 0 && lb_McuResponses.SelectedIndex >= 0)
				showError(lbl_McuParse, lb_McuResponses.SelectedItem.ToString());
		}
		#endregion

		#region McuResponses_DoubleClick
		private void listMcuResponses_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				Clipboard.SetDataObject(lb_McuResponses.SelectedItem.ToString());
			}
			catch { }
		}
		#endregion

		#region openMcuLink
		private void openMcuLink()
		{
			string status = string.Empty;
			if (m_comMcu.GetDevicePort(ref status, SendToCom.Text, 0) == 1)
			{
				m_enableMcuReceive = false;
				timerMcuRead.Enabled = true;
				mi_Connect.Text = "Disconnect".Translate();
			}
			lbl_statusText.Text = status;
		}
		#endregion

		#region panelDisplay_Paint
		private void panelDisplay_Paint(object sender, PaintEventArgs e)
		{
			try
			{
				int offset = 7;
				Pen pen = new Pen(Color.Yellow, 1f);
				Graphics graphics = panelDisplay.CreateGraphics();
				graphics.Clear(panelDisplay.BackColor);
				if (ucRunScreen.Visible && m_showXY)
				{
					graphics.DrawString("(0,0)", new Font(Encoding.Default.EncodingName, 9f), new SolidBrush(Color.Yellow), (PointF)new Point((ucRunScreen.Left - offset) - 0x11, (ucRunScreen.Top - offset) - 15));
					graphics.DrawString("X", new Font(Encoding.Default.EncodingName, 9f), new SolidBrush(Color.Yellow), (PointF)new Point((ucRunScreen.Left + (ucRunScreen.Width / 2)) - 5, (ucRunScreen.Top - offset) - 0x11));
					graphics.DrawString("Y", new Font(Encoding.Default.EncodingName, 9f), new SolidBrush(Color.Yellow), (PointF)new Point(ucRunScreen.Left - 20, (ucRunScreen.Top + (ucRunScreen.Height / 2)) - 5));
					graphics.DrawLine(pen, new Point(ucRunScreen.Left - offset, ucRunScreen.Top - offset), new Point(ucRunScreen.Left + ucRunScreen.Width, ucRunScreen.Top - offset));
					graphics.DrawLine(pen, new Point((ucRunScreen.Left + ucRunScreen.Width) - 8, (ucRunScreen.Top - offset) - 4), new Point(ucRunScreen.Left + ucRunScreen.Width, ucRunScreen.Top - offset));
					graphics.DrawLine(pen, new Point(ucRunScreen.Left - offset, ucRunScreen.Top - offset), new Point(ucRunScreen.Left - offset, ucRunScreen.Top + ucRunScreen.Height));
					graphics.DrawLine(pen, new Point((ucRunScreen.Left - offset) - 4, (ucRunScreen.Top + ucRunScreen.Height) - 8), new Point(ucRunScreen.Left - offset, ucRunScreen.Top + ucRunScreen.Height));
				}
			}
			catch { }
		}
		#endregion

		#region panelDisplay_Resize
		private void panelDisplay_Resize(object sender, EventArgs e)
		{
			resizeForm();
		}
		#endregion

		#region rbKeyboardMcyInput_CheckedChanged
		private void rbKeyboardMcyInput_CheckedChanged(object sender, EventArgs e)
		{
			link_RunAll.Visible =
			SendTo.Enabled =
			linkWaveform.Visible =
			mi_Upload.Enabled = rbKeyboardInput.Checked;

			link_Start.Visible =
			label7.Visible =
			label8.Visible =
			cbComPorts.Visible =
			cbBaudRates.Visible =
			tbManualCommand.ReadOnly = rbMcuInput.Checked;

			m_enableMcuReceive = !rbKeyboardInput.Checked;

			if (rbKeyboardInput.Checked && m_com.IsOpen)
			{
				timerMcuRead.Enabled = false;
				comClose();

				link_Start.Text = "Start".Translate();
				cbComPorts.Enabled = true;
				cbBaudRates.Enabled = true;
			}
		}
		#endregion

		#region resizeForm
		private void resizeForm()
		{
			try
			{
				ucRunScreen.Left = (panelDisplay.Width - ucRunScreen.Width) / 2;
				ucRunScreen.Top = (panelDisplay.Height - ucRunScreen.Height) / 2;
				if (ucRunScreen.Left < 25)
					ucRunScreen.Left = 25;
				if (ucRunScreen.Top < 25)
					ucRunScreen.Top = 25;
			}
			catch { }
		}
		#endregion

		#region SerialDebug_FormClosing
		private void SerialDebug_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (m_comMcu.RunState == 0)
			{
				m_comMcu.RunState = 2;
				e.Cancel = true;
			}
			else
			{
				stopSend();
				m_comMcu.ComClose();
				ucRunScreen.RunStop();
			}
		}
		#endregion

		#region ucRunScreen_MouseWheel
		private void ucRunScreen_MouseWheel(object sender, MouseEventArgs e)
		{
			if (ucRunScreen.Visible)
			{
				setZoomFactor(e.Delta);
				((HandledMouseEventArgs)e).Handled = true;
			}
		}
		private void setZoomFactor(int delta)
		{
			if (panelDisplay.InvokeRequired)
				panelDisplay.Invoke(new Action<int>(setZoomFactor), delta);
			else if (ucRunScreen.SetZoom(delta))
			{
				resizeForm();
				panelDisplay.Refresh();
			}
		}
		#endregion

		#region SerialDebug_Load
		private void SerialDebug_Load(object sender, EventArgs e)
		{
			base.Icon = HmiOptions.Icon;
			Text = HmiOptions.SoftName;

			//!!! wheel ucRunScreen.MouseWheel += new MouseEventHandler(ucRunScreen_MouseWheel);

			m_channelId[0] = 0;
			m_channelId[1] = 0xff;
			m_channelId[2] = 0xff;
			m_channelId[3] = 0xff;

			cbBaudRates.Items.Clear();
			cbBaudRates.Items.Add("2400");
			cbBaudRates.Items.Add("4800");
			cbBaudRates.Items.Add("9600");
			cbBaudRates.Items.Add("19200");
			cbBaudRates.Items.Add("38400");
			cbBaudRates.Items.Add("57600");
			cbBaudRates.Items.Add("115200");
			cbBaudRates.Text = "9600";

			SendToCom.Visible = false;
			mi_Connect.Visible = false;
			lbl_ComPort.Visible = false;
			m_receivedMcuData = "";
			SendTo.Items.Clear();
			SendTo.Items.Add("Current Simulator".Translate());
			SendTo.Items.Add("Nextion Device".Translate());
			SendTo.Items.Add("Simulator and Nextion Device".Translate());
			SendTo.SelectedIndex = 0;
			getPorts();
			m_comMcu.Port = m_com;
		}
		#endregion

		#region ucRunScreen_Resize
		private void ucRunScreen_Resize(object sender, EventArgs e)
		{
			resizeForm();
		}
		#endregion

		#region ucRunScreen_SendRunCode
		private void ucRunScreen_SendRunCode(object sender, EventArgs e)
		{
			addToLog((string)sender);
		}
		#endregion

		#region ucRunScreen_SendByte
		private void ucRunScreen_SendByte(object sender, EventArgs e)
		{
			m_ms_lastSimReceive = 0;
			int num = (int)sender;
			string str = Convert.ToString(num, 16);
			if (str.Length == 1)
				str = "0" + str;

			m_receivedSimData = m_receivedSimData + "0x" + str + " ";
			if (m_enableMcuReceive && m_com.IsOpen)
			{
				byte[] buffer = new byte[] { (byte)num };
				m_com.Write(buffer, 0, 1);
			}
			if (str == "ff")
				m_ddx++;
			else
				m_ddx = 0;

			if (m_ddx >= 3)
			{
				m_ddx = 0;
				addToSimResponse(m_receivedSimData.Trim());
				m_receivedSimData = "";
			}
		}
		#endregion

		#region sendAll
		private void sendAll()
		{
			lb_SimResponses.Items.Clear();
			lb_McuResponses.Items.Clear();
			if (((SendTo.SelectedIndex == 1) || (SendTo.SelectedIndex == 2)) && !m_com.IsOpen)
			{
				MessageBox.Show("Nextion device is not connected".Translate());
			}
			if (tbManualCommand.Lines.Length > 0)
				for (int i = 0; i < tbManualCommand.Lines.Length; i++)
					if (tbManualCommand.Lines[i].Trim().Length > 0)
						sendCode(tbManualCommand.Lines[i].Trim());
		}
		#endregion

		#region sendProcess
		private void sendProcess()
		{
			Random random = new Random();
			List<int> curve = new List<int>();
			int idxCurve = 0;
			int sendedToSim = 0;
			Utility.GetCurve(m_curveAmplitude, m_curveOffset, ref curve);

			while (m_stopSend == 1)
			{
				try
				{
					Application.DoEvents();
					ms_interval = Utility.GetInt(tbInterval.Text.Trim());

					sendedToSim = 0;
					for (int idxChannel = 0; idxChannel < 4; idxChannel++)
						if (m_channelId[idxChannel] < 4)
						{
							byte num2;
							if (cbRandom.Checked)
								num2 = (byte)random.Next(int.Parse(tbMinValue.Text), int.Parse(tbMaxValue.Text));
							else if ((idxCurve + 16 * idxChannel) >= curve.Count)
							{
								int num5 = idxCurve + 16 * idxChannel;
								while (num5 >= curve.Count)
									num5 -= curve.Count;
								num2 = (byte)curve[num5];
							}
							else
								num2 = (byte)curve[idxCurve + (16 * idxChannel)];

							string cmd = "add " + tbComponentId.Text + ","
										+ m_channelId[idxChannel].ToString() + ","
										+ num2.ToString();
							if (m_curveSendSim)
							{
								sendSim(cmd);
								sendedToSim += cmd.Length;
							}
							if (m_curveSendCom && m_com.IsOpen)
							{
								m_com.SendStringEnd(cmd);
								sendedToSim = 0;
							}
						}

					int ms_count = 0;
					while (ms_count < ms_interval)
					{
						Thread.Sleep(1);
						Application.DoEvents();
						if (m_stopSend != 1)
							break;
						ms_count++;
					}

					for (ms_count = 0; ms_count < sendedToSim; ms_count++)
					{
						Thread.Sleep(1);
						Application.DoEvents();
						if (m_stopSend != 1)
							break;
					}

					idxCurve++;
					if (idxCurve == curve.Count)
						idxCurve = 0;
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
					m_stopSend = 1;
					while (m_stopSend == 1)
						Application.DoEvents();
				}
			}
			m_stopSend = 0;
		}
		#endregion

		#region stopSend
		private void stopSend()
		{
			if (m_send_thread != null && m_send_thread.IsAlive)
			{
				m_stopSend = 2;
				while (m_stopSend != 0)
				{
					Application.DoEvents();
				}
			}

			tbInterval.Enabled = true;
			tbChannel.Enabled = true;
			tbComponentId.Enabled = true;
			tbMinValue.Enabled = true;
			tbMaxValue.Enabled = true;
			btnSend.Text = "Send".Translate();
			mi_Connect.Enabled = true;
			SendTo.Enabled = true;
			SendToCom.Enabled = true;
		}
		#endregion

		#region tbManualCommand_KeyPress
		private void tbManualCommand_KeyPress(object sender, KeyPressEventArgs e)
		{
			int keyChar = e.KeyChar;
			if (cbPressEnter.Checked && keyChar == 13)
			{
				if (!m_com.IsOpen && (SendTo.SelectedIndex == 1 || SendTo.SelectedIndex == 2))
					MessageBox.Show("Nextion device is not connected".Translate());
				else if (tbManualCommand.Lines.Length > 0)
					for (int i = tbManualCommand.Lines.Length - 1; i >= 0; i--)
						if (tbManualCommand.Lines[i].Length > 2)
						{
							sendCode(tbManualCommand.Lines[i]);
							break;
						}
			}
		}
		#endregion

		#region sendCode
		private void sendCode(string j)
		{
			if (SendTo.SelectedIndex == 0 || SendTo.SelectedIndex == 2)
				sendSim(j);
			if (SendTo.SelectedIndex == 1 || SendTo.SelectedIndex == 2)
				m_com.SendStringEnd(j);
		}
		#endregion

		#region sendSim
		private void sendSim(string cmd)
		{
			if (ucRunScreen.InvokeRequired)
				ucRunScreen.Invoke(new Action<string>(sendSim), cmd);
			else
			{
				cmd = cmd.Trim();
				if (cmd.Length > 2)
				{
					byte[] cmdBytes = cmd.ToBytes();
					for (int i = 0; i < cmdBytes.Length; i++)
						ucRunScreen.SendComData(cmdBytes[i]);

					ucRunScreen.SendComData(0xff);
					ucRunScreen.SendComData(0xff);
					ucRunScreen.SendComData(0xff);
				}
			}
		}
		#endregion

		#region addToSimResponse
		/// <summary>
		/// Add response to SIM Responses list
		/// </summary>
		/// <param name="response"></param>
		private void addToSimResponse(string response)
		{
			if (lb_SimResponses.InvokeRequired)
				lb_SimResponses.Invoke(new Action<string>(addToSimResponse), response);
			else
			{
				lb_SimResponses.Items.Add(response);
				lb_SimResponses.SelectedIndex = lb_SimResponses.Items.Count - 1;
			}
		}
		#endregion

		#region addMcuResponse
		/// <summary>
		/// Add response to MCU Responses list
		/// </summary>
		/// <param name="response"></param>
		private void addMcuResponse(string response)
		{
			if (lb_SimResponses.InvokeRequired)
				lb_SimResponses.Invoke(new Action<string>(addMcuResponse), response);
			else
			{
				lb_McuResponses.Items.Add(response);
				lb_McuResponses.SelectedIndex = lb_McuResponses.Items.Count - 1;
			}
		}
		#endregion

		#region addToLog
		/// <summary>
		/// Add response to Log
		/// </summary>
		/// <param name="response"></param>
		private void addToLog(string response)
		{
			if (lb_Log.Visible)
			{
				if (lb_Log.InvokeRequired)
				{
					// lb_Log.Invoke(new Action<string>(addToSimResponse), response);
				}
				else if (lb_Log.Visible)
				{
					lb_Log.Items.Add(response);
					lb_Log.SelectedIndex = lb_Log.Items.Count - 1;
				}
			}
		}
		#endregion

		#region tbInterval_TextChanged
		private void tbInterval_TextChanged(object sender, EventArgs e)
		{
			try
			{
				ms_interval = int.Parse(tbInterval.Text);
				if (ms_interval > 1000)
				{
					tbInterval.Text = "1000";
					ms_interval = 1000;
				}
			}
			catch { }
		}
		#endregion

		#region tbChannel_TextChanged
		private void tbChannel_TextChanged(object sender, EventArgs e)
		{
			setChannel();
		}
		#endregion

		#region timerMcuRead_Tick
		private void timerMcuRead_Tick(object sender, EventArgs e)
		{
			string str;
			int data;
			timerMcuRead.Enabled = false;
			if (m_com.IsOpen)
			{
				while (m_com.BytesToRead > 0)
				{
					m_ms_lastMcuReceive = 0;
					data = m_com.ReadByte();
					if (m_enableMcuReceive)
						ucRunScreen.SendComData((byte)data);
					if (data == 0xFF)
						m_counter_0xFF++;
					else
						m_counter_0xFF = 0;

					str = data.ToString("X2");
					m_receivedMcuData = m_receivedMcuData + " 0x" + str;
					
					if (m_counter_0xFF > 2)
					{	// More than 2 0xFF means end of package
						addMcuResponse(m_receivedMcuData.Trim());
						m_receivedMcuData = "";
						m_counter_0xFF = 0;
					}
				}
			}
			timerMcuRead.Enabled = true;
		}
		#endregion

		#region timerSimMcuLog_Tick 300 ms
		private void timerSimMcuLog_Tick(object sender, EventArgs e)
		{
			TimerCom.Enabled = false;
			if (m_ms_lastSimReceive > 300)
			{
				if (!string.IsNullOrEmpty(m_receivedSimData))
				{
					m_ddx = 0;
					addToSimResponse(m_receivedSimData.Trim());
					m_receivedSimData = "";
					m_ms_lastSimReceive = 0;
				}
			}
			else
				m_ms_lastSimReceive += TimerCom.Interval;

			if (m_ms_lastMcuReceive > 300)
			{
				if (!string.IsNullOrEmpty(m_receivedMcuData))
				{
					addMcuResponse(m_receivedMcuData.Trim());
					m_receivedMcuData = "";
					m_counter_0xFF = 0;
					m_ms_lastMcuReceive = 0;
				}
			}
			else
				m_ms_lastMcuReceive += TimerCom.Interval;
			
			TimerCom.Enabled = true;
		}
		#endregion

		#region miUpload_Click
		private void miUpload_Click(object sender, EventArgs e)
		{
			bool flag = false;
			stopSend();
			if (m_com.IsOpen)
			{
				flag = true;
				closeLink();
			}
			new FirmwareUpload(m_binPath).ShowDialog();
			if (flag)
			{
				mi_Connect.Enabled = false;
				mi_Upload.Enabled = false;
				openMcuLink();
				mi_Connect.Enabled = true;
				mi_Upload.Enabled = true;
			}
		}
		#endregion

		#region miXY_Click
		private void miXY_Click(object sender, EventArgs e)
		{
			m_showXY = !m_showXY;
			panelDisplay.Refresh();
		}
		#endregion

		#region miConnect_Click
		private void miConnect_Click(object sender, EventArgs e)
		{
			mi_Connect.Enabled = false;
			mi_Upload.Enabled = false;
			try
			{
				if (mi_Connect.Text == "Disconnect".Translate())
					closeLink();
				else
					openMcuLink();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			mi_Connect.Enabled = true;
			mi_Upload.Enabled = true;
		}
		#endregion
	}
}
