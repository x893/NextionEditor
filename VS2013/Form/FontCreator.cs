using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace NextionEditor
{
	public class FontCreator : Form
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct FontDescriptor
		{
			public int Xpi;
			public int Ypi;
			public int intXpi;
			public int intYpi;
			public float FontSize;
			public string FontName;
			public FontStyle FontStyle;
		}

		#region Variables
		private Button btnGenerate;
		private CheckBox cbBold;
		private ComboBox cbHeight;
		private ComboBox cbChinese;
		private ComboBox cbEncode;
		private ComboBox cbLetter;
		private ComboBox cbRange;
		private ComboBox cbCompensation;
		private IContainer components = null;
		private GroupBox groupBox1;
		private Label label1;
		private Label label10;
		private Label label2;
		private Label label3;
		private Label label4;
		private Label label5;
		private Label label6;
		private Label label7;
		private Label label8;
		private Label label9;
		private PictureBox pictureBox;
		private PictureBox pictureBoxL;
		private PictureBox pictureBoxC;
		private TextBox tbFontName;
		private TextBox textBox2;

		private FormParams m_params;
		private GdiFont.InfoGdiFont showFont = new GdiFont.InfoGdiFont();
		private Bitmap m_image;
		private bool m_closing = false;
		private FontDescriptor m_font;
		private InterpolationMode m_settings;
		private bool m_custom = false;
		private FontDescriptor m_char;
		private string m_fontString = "";
		#endregion

		#region Constructor
		public FontCreator(FormParams formParams)
		{
			m_params = formParams;
			if (!string.IsNullOrEmpty(m_params.Strings[0]))
				m_custom = true;
			InitializeComponent();
			Utility.Translate(this);
		}
		#endregion

		#region btnGenerate_Click
		private void btnGenerate_Click(object sender, EventArgs e)
		{
			btnGenerate.Enabled = true;
			m_closing = false;
			if (string.IsNullOrEmpty(tbFontName.Text))
				MessageBox.Show("Define font's name".Translate());
			else if (create() && m_custom)
				base.DialogResult = DialogResult.OK;
		}
		#endregion

		#region cbBold_CheckedChanged
		private void cbBold_CheckedChanged(object sender, EventArgs e)
		{
			if (pictureBoxC.Visible)
				generateFont();
			refreshChars();
		}

		private void cbHeight_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (pictureBoxC.Visible)
				generateFont();
			refreshChars();
		}
		#endregion

		#region cbChinese_SelectedIndexChanged
		private void cbChinese_SelectedIndexChanged(object sender, EventArgs e)
		{
			generateFont();
		}
		#endregion

		#region cbEncode_SelectedIndexChanged
		private void cbEncode_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (pictureBoxC.Visible)
				generateFont();
			refreshChars();
		}
		#endregion

		#region cbLetter_SelectedIndexChanged
		private void cbLetter_SelectedIndexChanged(object sender, EventArgs e)
		{
			refreshChars();
		}
		#endregion

		#region cbRange_SelectedIndexChanged
		private void cbRange_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (cbRange.SelectedIndex == 0)
				{
					cbChinese.Visible = false;
					pictureBoxC.Visible = false;
					label8.Visible = false;
				}
				else
				{
					cbChinese.Visible = true;
					pictureBoxC.Visible = true;
					label8.Visible = true;
				}
				int num = base.Height - base.ClientRectangle.Height;
				if (cbRange.SelectedIndex == 2)
					base.Height = ((textBox2.Top + textBox2.Height) + num) + 10;
				else
					base.Height = ((btnGenerate.Top + btnGenerate.Height) + num) + 10;
			}
			finally { }
		}
		#endregion

		#region cbCompensation_SelectedIndexChanged
		private void cbCompensation_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (cbCompensation.SelectedIndex)
			{
				case 0:
					m_settings = InterpolationMode.Low;
					break;
				case 1:
					m_settings = InterpolationMode.NearestNeighbor;
					break;
				case 2:
					m_settings = InterpolationMode.Bicubic;
					break;
				case 3:
					m_settings = InterpolationMode.Bicubic;
					break;
			}
		}
		#endregion

		#region create()
		private bool create()
		{
			Font font;
			int num3;
			int num4;
			int num5;
			int num6;
			int num7;
			int num10;
			int num12;
			int index = 0;
			byte[] bytes = new byte[2];
			SaveFileDialog dialog = new SaveFileDialog
			{
				Filter = "zi|*.zi"
			};
			if (dialog.ShowDialog() != DialogResult.OK)
				return false;

			string fileName = dialog.FileName;
			byte[] fontBytes;
			byte[] fontNameBytes = tbFontName.Text.ToBytes();

			int fontHeight = int.Parse(cbHeight.Text);

			InfoFont fontInfo = new InfoFont();
			fontInfo.State = (byte)cbRange.SelectedIndex;
			fontInfo.DataOffset = 0;
			fontInfo.NameStart = 0;
			fontInfo.NameEnd = (ushort)(fontNameBytes.Length - 1);
			fontInfo.Height = byte.Parse(fontHeight.ToString());
			if (fontInfo.State == 0)
			{
				num12 = fontInfo.Height / 2;
				fontInfo.Width = byte.Parse(num12.ToString());
				fontInfo.Length = 0x5f;
				num10 = (fontInfo.Height * fontInfo.Width) / 8;
				fontBytes = new byte[0x5f * num10];
			}
			else
			{
				fontInfo.Width = fontInfo.Height;
				fontInfo.CodeHStart = 0xA1;
				fontInfo.CodeHEnd = 0xF7;
				fontInfo.CodeLStart = 0xA1;
				fontInfo.CodeLEnd = 0xFE;

				if (fontInfo.State == 1)
					fontInfo.Length = 8273;

				if (fontInfo.State == 2)
				{
					m_fontString = "";
					num6 = 0;
					while (num6 < textBox2.Text.Length)
					{
						fontStringAdd(textBox2.Text.Substring(num6, 1));
						num6++;
					}
					m_fontString = m_fontString + "?";
					if (m_fontString == "?")
					{
						MessageBox.Show("No available font".Translate());
						return false;
					}
					textBox2.Text = m_fontString;
					fontInfo.Length = (uint)m_fontString.Length;
				}
				num10 = (fontInfo.Height * fontInfo.Width) / 8;
				if (fontInfo.State == 2)
					fontBytes = new byte[fontInfo.Length * (num10 + 2)];
				else
					fontBytes = new byte[fontInfo.Length * num10];
			}

			Bitmap image = new Bitmap(fontInfo.Width, fontInfo.Height);
			Graphics graphics = Graphics.FromImage(image);
			Encoding ec = Encoding.GetEncoding(cbEncode.Text);
			label3.Text = "";
			label3.Visible = true;
			if (fontInfo.State == 1)
			{
				font = new Font(m_font.FontName, m_font.FontSize, m_font.FontStyle);
				for (int i = 0xa1; i < 0xf8; i++)
				{
					Application.DoEvents();
					num12 = ((i - 160) * 100) / 0x57;
					label3.Text = "Progress:".Translate() + num12.ToString() + "%";
					num3 = 0xa1;
					while (num3 < 0xff)
					{
						if (m_closing)
						{
							return false;
						}
						Application.DoEvents();
						bytes[0] = byte.Parse(i.ToString());
						bytes[1] = byte.Parse(num3.ToString());
						graphics.Clear(Color.FromArgb(0, 0, 0));
						graphics.DrawString(ec.GetString(bytes), font, Brushes.Red, (PointF)new Point(m_font.Xpi, m_font.Ypi));
						num4 = 0;
						num5 = 0;
						num6 = 0;
						while (num6 < num10)
						{
							fontBytes[index] = 0;
							num7 = 0;
							while (num7 < 8)
							{
								if (image.GetPixel(num4, num5).R > 0)
								{
									fontBytes[index] = (byte)(fontBytes[index] + (((int)1) << (7 - num7)));
								}
								num5++;
								if (num5 >= image.Height)
								{
									num4++;
									num5 = 0;
								}
								num7++;
							}
							index++;
							num6++;
						}
						num3++;
					}
				}
			}
			if (fontInfo.State == 0 || fontInfo.State == 1)
			{
				font = new Font(m_char.FontName, m_char.FontSize, m_char.FontStyle);
				for (num3 = 0x20; num3 < 0x7f; num3++)
				{
					Application.DoEvents();
					label3.Text = (num3 - 0x1f).ToString();
					bytes[0] = byte.Parse(num3.ToString());
					if (!adjustmentFont(image, m_char, ec, (byte)num3))
					{
						return false;
					}
					num4 = 0;
					num5 = 0;
					num6 = 0;
					while (num6 < num10)
					{
						fontBytes[index] = 0;
						num7 = 0;
						while (num7 < 8)
						{
							if (image.GetPixel(num4, num5).R > 0)
							{
								fontBytes[index] = (byte)(fontBytes[index] + (((int)1) << (7 - num7)));
							}
							num5++;
							if (num5 >= image.Height)
							{
								num4++;
								num5 = 0;
							}
							num7++;
						}
						index++;
						num6++;
					}
				}
			}

			if (fontInfo.State == 2)
			{
				for (int j = 0; j < m_fontString.Length; j++)
				{
					Application.DoEvents();
					label3.Text = j.ToString();
					byte[] buffer4 = m_fontString.Substring(j, 1).ToBytes();
					if (buffer4.Length >= 0)
					{
						if ((buffer4[0] > 0x1f) && (buffer4[0] < 0x7f))
						{
							if (!adjustmentFont(image, m_char, ec, buffer4[0]))
								return false;
							
							num4 = 0;
							num5 = 0;
							fontBytes[index] = 0;
							index++;
							fontBytes[index] = buffer4[0];
							index++;
							num6 = 0;
							while (num6 < num10)
							{
								fontBytes[index] = 0;
								num7 = 0;
								while (num7 < 8)
								{
									if (image.GetPixel(num4, num5).R > 0)
									{
										fontBytes[index] = (byte)(fontBytes[index] + (((int)1) << (7 - num7)));
									}
									num5++;
									if (num5 >= image.Height)
									{
										num4++;
										num5 = 0;
									}
									num7++;
								}
								index++;
								num6++;
							}
						}
						else if ((buffer4[0] > 0x7f) && (buffer4.Length == 2))
						{
							font = new Font(m_font.FontName, m_font.FontSize, m_font.FontStyle);
							graphics.Clear(Color.FromArgb(0, 0, 0));
							graphics.DrawString(ec.GetString(buffer4), font, Brushes.Red, (PointF)new Point(m_font.Xpi, m_font.Ypi));
							num4 = 0;
							num5 = 0;
							fontBytes[index] = buffer4[0];
							index++;
							fontBytes[index] = buffer4[1];
							index++;
							for (num6 = 0; num6 < num10; num6++)
							{
								fontBytes[index] = 0;
								for (num7 = 0; num7 < 8; num7++)
								{
									if (image.GetPixel(num4, num5).R > 0)
									{
										fontBytes[index] = (byte)(fontBytes[index] + (((int)1) << (7 - num7)));
									}
									num5++;
									if (num5 >= image.Height)
									{
										num4++;
										num5 = 0;
									}
								}
								index++;
							}
						}
					}
				}
			}

			fontInfo.Size = (uint)(fontBytes.Length + fontNameBytes.Length);

			using (StreamWriter writer = new StreamWriter(fileName))
			{
				bytes = Utility.ToBytes(fontInfo);
				writer.BaseStream.Write(bytes, 0, bytes.Length);

				writer.BaseStream.Write(fontNameBytes, 0, fontNameBytes.Length);
				writer.BaseStream.Write(fontBytes, 0, fontBytes.Length);
				writer.Close();
			}
			m_params.Strings[0] = fileName;
			label3.Text = label3.Text + "  " + "Font Size:".Translate() + fontInfo.Size.ToString("0,000");
			MessageBox.Show("Font generate finished, size:".Translate() + fontInfo.Size.ToString("0,000"));
			return true;
		}
		#endregion

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
			this.cbHeight = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.cbChinese = new System.Windows.Forms.ComboBox();
			this.btnGenerate = new System.Windows.Forms.Button();
			this.pictureBoxC = new System.Windows.Forms.PictureBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.cbEncode = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.tbFontName = new System.Windows.Forms.TextBox();
			this.pictureBoxL = new System.Windows.Forms.PictureBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.cbLetter = new System.Windows.Forms.ComboBox();
			this.label9 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.cbRange = new System.Windows.Forms.ComboBox();
			this.cbBold = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.cbCompensation = new System.Windows.Forms.ComboBox();
			this.label10 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxC)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxL)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// cbHeight
			// 
			this.cbHeight.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbHeight.FormattingEnabled = true;
			this.cbHeight.Location = new System.Drawing.Point(86, 24);
			this.cbHeight.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.cbHeight.Name = "cbHeight";
			this.cbHeight.Size = new System.Drawing.Size(65, 25);
			this.cbHeight.TabIndex = 0;
			this.cbHeight.SelectedIndexChanged += new System.EventHandler(this.cbHeight_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(31, 27);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(49, 17);
			this.label1.TabIndex = 1;
			this.label1.Text = "Height:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cbChinese
			// 
			this.cbChinese.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbChinese.FormattingEnabled = true;
			this.cbChinese.Location = new System.Drawing.Point(82, 26);
			this.cbChinese.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.cbChinese.Name = "cbChinese";
			this.cbChinese.Size = new System.Drawing.Size(103, 25);
			this.cbChinese.TabIndex = 2;
			this.cbChinese.SelectedIndexChanged += new System.EventHandler(this.cbChinese_SelectedIndexChanged);
			// 
			// btnGenerate
			// 
			this.btnGenerate.Location = new System.Drawing.Point(376, 400);
			this.btnGenerate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnGenerate.Name = "btnGenerate";
			this.btnGenerate.Size = new System.Drawing.Size(128, 48);
			this.btnGenerate.TabIndex = 4;
			this.btnGenerate.Text = "Generate font";
			this.btnGenerate.UseVisualStyleBackColor = true;
			this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
			// 
			// pictureBoxC
			// 
			this.pictureBoxC.BackColor = System.Drawing.Color.Red;
			this.pictureBoxC.Location = new System.Drawing.Point(82, 59);
			this.pictureBoxC.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pictureBoxC.Name = "pictureBoxC";
			this.pictureBoxC.Size = new System.Drawing.Size(30, 29);
			this.pictureBoxC.TabIndex = 5;
			this.pictureBoxC.TabStop = false;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(17, 431);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(43, 17);
			this.label3.TabIndex = 6;
			this.label3.Text = "label3";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(171, 28);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(42, 17);
			this.label5.TabIndex = 10;
			this.label5.Text = "Code:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cbCode
			// 
			this.cbEncode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbEncode.FormattingEnabled = true;
			this.cbEncode.Location = new System.Drawing.Point(219, 25);
			this.cbEncode.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.cbEncode.Name = "cbCode";
			this.cbEncode.Size = new System.Drawing.Size(124, 25);
			this.cbEncode.TabIndex = 9;
			this.cbEncode.SelectedIndexChanged += new System.EventHandler(this.cbEncode_SelectedIndexChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(76, 292);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(0, 17);
			this.label6.TabIndex = 12;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(64, 371);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(57, 17);
			this.label7.TabIndex = 14;
			this.label7.Text = "Range:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(5, 456);
			this.textBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.textBox2.Multiline = true;
			this.textBox2.Name = "textBox2";
			this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBox2.Size = new System.Drawing.Size(499, 200);
			this.textBox2.TabIndex = 15;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(281, 371);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(89, 17);
			this.label4.TabIndex = 18;
			this.label4.Text = "Font Name:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tbFontName
			// 
			this.tbFontName.Location = new System.Drawing.Point(376, 367);
			this.tbFontName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.tbFontName.Name = "tbFontName";
			this.tbFontName.Size = new System.Drawing.Size(128, 25);
			this.tbFontName.TabIndex = 19;
			// 
			// pictureBoxL
			// 
			this.pictureBoxL.BackColor = System.Drawing.Color.Red;
			this.pictureBoxL.Location = new System.Drawing.Point(350, 59);
			this.pictureBoxL.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pictureBoxL.Name = "pictureBoxL";
			this.pictureBoxL.Size = new System.Drawing.Size(30, 29);
			this.pictureBoxL.TabIndex = 24;
			this.pictureBoxL.TabStop = false;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.pictureBox);
			this.groupBox1.Controls.Add(this.cbLetter);
			this.groupBox1.Controls.Add(this.label9);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.pictureBoxC);
			this.groupBox1.Controls.Add(this.pictureBoxL);
			this.groupBox1.Controls.Add(this.cbChinese);
			this.groupBox1.Location = new System.Drawing.Point(5, 107);
			this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.groupBox1.Size = new System.Drawing.Size(499, 252);
			this.groupBox1.TabIndex = 25;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Preview Area";
			// 
			// pictureBox
			// 
			this.pictureBox.BackColor = System.Drawing.Color.Red;
			this.pictureBox.Location = new System.Drawing.Point(148, 86);
			this.pictureBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(167, 156);
			this.pictureBox.TabIndex = 35;
			this.pictureBox.TabStop = false;
			this.pictureBox.Visible = false;
			// 
			// cbLetter
			// 
			this.cbLetter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbLetter.FormattingEnabled = true;
			this.cbLetter.Location = new System.Drawing.Point(350, 26);
			this.cbLetter.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.cbLetter.Name = "cbLetter";
			this.cbLetter.Size = new System.Drawing.Size(103, 25);
			this.cbLetter.TabIndex = 34;
			this.cbLetter.SelectedIndexChanged += new System.EventHandler(this.cbLetter_SelectedIndexChanged);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(294, 29);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(50, 17);
			this.label9.TabIndex = 26;
			this.label9.Text = "Letters:";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(20, 29);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(56, 17);
			this.label8.TabIndex = 25;
			this.label8.Text = "Chinese:";
			// 
			// cbRange
			// 
			this.cbRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbRange.FormattingEnabled = true;
			this.cbRange.Location = new System.Drawing.Point(127, 367);
			this.cbRange.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.cbRange.Name = "cbRange";
			this.cbRange.Size = new System.Drawing.Size(145, 25);
			this.cbRange.TabIndex = 26;
			this.cbRange.SelectedIndexChanged += new System.EventHandler(this.cbRange_SelectedIndexChanged);
			// 
			// cbBold
			// 
			this.cbBold.AutoSize = true;
			this.cbBold.Location = new System.Drawing.Point(355, 27);
			this.cbBold.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.cbBold.Name = "cbBold";
			this.cbBold.Size = new System.Drawing.Size(53, 21);
			this.cbBold.TabIndex = 27;
			this.cbBold.Text = "Bold";
			this.cbBold.UseVisualStyleBackColor = true;
			this.cbBold.CheckedChanged += new System.EventHandler(this.cbBold_CheckedChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(17, 53);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(134, 17);
			this.label2.TabIndex = 29;
			this.label2.Text = "compression scheme:";
			// 
			// cbCompensation
			// 
			this.cbCompensation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbCompensation.FormattingEnabled = true;
			this.cbCompensation.Location = new System.Drawing.Point(20, 74);
			this.cbCompensation.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.cbCompensation.Name = "cbCompensation";
			this.cbCompensation.Size = new System.Drawing.Size(72, 25);
			this.cbCompensation.TabIndex = 28;
			this.cbCompensation.SelectedIndexChanged += new System.EventHandler(this.cbCompensation_SelectedIndexChanged);
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(98, 77);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(206, 17);
			this.label10.TabIndex = 30;
			this.label10.Text = "Default scheme is recommended. ";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FontCreator
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(512, 665);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.cbCompensation);
			this.Controls.Add(this.cbBold);
			this.Controls.Add(this.cbRange);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.tbFontName);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.cbEncode);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.btnGenerate);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cbHeight);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.Name = "FontCreator";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Font Creator";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FontCreator_FormClosing);
			this.Load += new System.EventHandler(this.FontCreator_Load);
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxC)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxL)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		#region generateFont
		private void generateFont()
		{
			btnGenerate.Enabled = false;
			try
			{
				if (cbBold.Checked)
					m_font.FontStyle = FontStyle.Bold;
				else
					m_font.FontStyle = FontStyle.Regular;
				
				int num = int.Parse(cbHeight.Text);
				pictureBoxC.Width = num;
				pictureBoxC.Height = num;
				m_font.FontName = cbChinese.Text;
				GdiFont.InfoGdiFont e_ = new GdiFont.InfoGdiFont();
				e_ = GdiFont.GetFontSize(
								"S".Translate(),
								m_font.FontName,
								m_font.FontStyle,
								pictureBoxC.Width,
								pictureBoxC.Height,
								(float)Utility.GetInt(cbHeight.Text),
								true,
								pictureBox
							);
				if (e_.FontSize != 0f)
				{
					m_font.FontSize = e_.FontSize;
					m_font.Xpi = ((pictureBoxC.Width - e_.Width) / 2) - e_.Xpi;
					m_font.Ypi = ((pictureBoxC.Height - e_.Height) / 2) - e_.Ypi;
					Font font = new Font(m_font.FontName, m_font.FontSize, m_font.FontStyle);
					m_image = new Bitmap(pictureBoxC.Width, pictureBoxC.Height);
					Graphics graphics = Graphics.FromImage(m_image);
					graphics.Clear(Color.FromArgb(0, 0, 0));
					graphics.DrawString("S".Translate(), font, Brushes.Red, (PointF)new Point(m_font.Xpi, m_font.Ypi));
					pictureBoxC.Image = m_image;
					btnGenerate.Enabled = true;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
		#endregion

		#region refreshChars
		private void refreshChars()
		{
			btnGenerate.Enabled = false;
			try
			{
				if (cbBold.Checked)
					m_char.FontStyle = FontStyle.Bold;
				else
					m_char.FontStyle = FontStyle.Regular;

				int num = int.Parse(cbHeight.Text);
				pictureBoxL.Width = num / 2;
				pictureBoxL.Height = num;
				m_char.FontName = cbLetter.Text;
				showFont = GdiFont.GetFontSize(
								"8",
								m_char.FontName,
								m_char.FontStyle,
								pictureBoxL.Width,
								pictureBoxL.Height,
								(float)Utility.GetInt(cbHeight.Text),
								true,
								pictureBox
							);
				if (showFont.FontSize != 0f)
				{
					m_char.FontSize = showFont.FontSize;
					m_char.Xpi = ((pictureBoxL.Width - showFont.Width) / 2) - showFont.Xpi;
					m_char.Ypi = ((pictureBoxL.Height - showFont.Height) / 2) - showFont.Ypi;
					Font font = new Font(m_char.FontName, m_char.FontSize, m_char.FontStyle);
					m_image = new Bitmap(pictureBoxL.Width, pictureBoxL.Height);
					adjustmentFont(m_image, m_char, Encoding.GetEncoding(cbEncode.Text), (byte)'X');
					pictureBoxL.Image = m_image;
					btnGenerate.Enabled = true;
				}
			}
			catch { }
		}
		#endregion

		#region adjustment
		private bool adjustmentFont(Bitmap image, FontDescriptor zipos, Encoding ec, byte ascii)
		{
			Graphics graphics = null;
			try
			{
				bool flag = true;
				int num = 0;
				byte[] bytes = new byte[] { ascii };
				string s = ec.GetString(bytes, 0, 1);
				FontDescriptor fontDesc = new FontDescriptor();
				fontDesc = zipos;
				int width = image.Width;
				int height = image.Height;

				if (width != height / 2)
					width = height / 2;
				
				Graphics.FromImage(image).Clear(Color.FromArgb(0, 0, 0));
				Bitmap bmi = new Bitmap(width * 3, height * 2);
				using (graphics = Graphics.FromImage(bmi))
				{
					if (s == "w")
						s = "w";
					
					if (s != "" && s != " ")
					{
						while (flag)
						{
							int num7;
							Graphics graphics2;
							int num4 = 0;
							fontDesc.Ypi--;

							while (num4 == 0)
							{
								fontDesc.Ypi++;
								Font font = new Font(fontDesc.FontName, fontDesc.FontSize, fontDesc.FontStyle);
								graphics.Clear(Color.FromArgb(0, 0, 0));
								graphics.DrawString(s, font, Brushes.Red, (PointF)new Point(0, fontDesc.Ypi));
								num4 = GdiFont.GetYFree(bmi);
							}
							Bitmap bitmap2 = new Bitmap(bmi.Width, height);
							int height2 = bmi.Height - GdiFont.GetY2Free(bmi);
							if (height2 > height)
								Graphics.FromImage(bitmap2).DrawImage(
															bmi,
															new Rectangle(0, 0, bitmap2.Width, bitmap2.Height),
															new Rectangle(0, 0, bitmap2.Width, height2),
															GraphicsUnit.Pixel
															);
							else
								Graphics.FromImage(bitmap2).DrawImage(
															bmi,
															new Rectangle(0, 0, bitmap2.Width, bitmap2.Height),
															new Rectangle(0, 0, bitmap2.Width, bitmap2.Height),
															GraphicsUnit.Pixel
															);

							int bmwidth = GdiFont.GetImageWidth(bitmap2);
							if ((width > 24 && bmwidth < width) || (width <= 24 && bmwidth <= width + 1))
							{
								num4 = GdiFont.GetXFree(bitmap2);
								num7 = (width - bmwidth) / 2;
								graphics2 = Graphics.FromImage(image);
								if (num > 0)
								{
									int num8 = GdiFont.GetY2Free(bitmap2);
									Graphics.FromImage(image).DrawImage(
																bitmap2,
																new Rectangle(0, 0, width, height),
																new Rectangle(num4 - num7, num - num8, width, height),
																GraphicsUnit.Pixel
																);
								}
								else
									Graphics.FromImage(image).DrawImage(
																bitmap2,
																new Rectangle(0, 0, width, height),
																new Rectangle(num4 - num7, 0, width, height),
																GraphicsUnit.Pixel
																);
								graphics2.Dispose();
								flag = false;
							}
							else if (cbCompensation.SelectedIndex == 3)
							{
								if (num == 0)
									num = GdiFont.GetY2Free(bitmap2);
								fontDesc.FontSize -= 0.5f;
								flag = true;
							}
							else
							{
								num7 = GdiFont.GetXFree(bitmap2);
								using (graphics2 = Graphics.FromImage(image))
								{
									graphics2.InterpolationMode = m_settings;
									if (width > 24)
										graphics2.DrawImage(
													bitmap2,
													new Rectangle(1, 0, width - 2, height),
													new Rectangle(num7, 0, bmwidth, height),
													GraphicsUnit.Pixel
													);
									else
										graphics2.DrawImage(
													bitmap2,
													new Rectangle(0, 0, width, height),
													new Rectangle(num7, 0, bmwidth, height),
													GraphicsUnit.Pixel
													);
									if (s == "X")
										s = "X";
								}
								flag = false;
							}
						}
					}
					pictureBoxL.Image = image;
					Application.DoEvents();
				}
				return true;
			}
			catch (Exception ex)
			{
				graphics.Dispose();
				MessageBox.Show(ex.Message);
			}
			return false;
		}
		#endregion

		#region FontCreator_Load
		private void FontCreator_Load(object sender, EventArgs e)
		{
			int num2;
			if (HmiOptions.Language == 0)
			{
				label5.Enabled = true;
				label9.Visible = true;
				label8.Visible = true;
				pictureBoxC.Visible = true;
			}
			else
			{
				label5.Enabled = false;
				label9.Visible = false;
				label8.Visible = false;
				pictureBoxC.Visible = false;
				cbLetter.Left = (groupBox1.Width - cbLetter.Width) / 2;
				pictureBoxL.Left = cbLetter.Left;
				cbChinese.Visible = false;
			}
			label3.Visible = false;

			cbHeight.Items.Clear();
			cbHeight.Items.Add("16");
			cbHeight.Items.Add("24");
			cbHeight.Items.Add("32");
			cbHeight.Items.Add("40");
			cbHeight.Items.Add("48");
			cbHeight.Items.Add("56");
			cbHeight.Items.Add("64");
			cbHeight.Items.Add("72");
			cbHeight.Items.Add("80");
			cbHeight.Items.Add("96");
			cbHeight.Items.Add("112");
			cbHeight.Items.Add("128");
			cbHeight.Items.Add("144");
			cbHeight.Items.Add("160");
			cbHeight.SelectedIndex = 0;

			cbChinese.Items.Clear();
			cbLetter.Items.Clear();

			cbCompensation.Items.Clear();
			cbCompensation.Items.Add("Mode0");
			cbCompensation.Items.Add("Mode1");
			cbCompensation.Items.Add("Mode2");
			cbCompensation.Items.Add("Mode3");
			cbCompensation.SelectedIndex = 0;

			InstalledFontCollection fonts = new InstalledFontCollection();
			FontFamily[] families = fonts.Families;
			int length = families.Length;
			for (num2 = 0; num2 < length; num2++)
			{
				string familyName = families[num2].Name;
				cbChinese.Items.Add(familyName);
				cbLetter.Items.Add(familyName);
			}
			cbChinese.Text = SystemFonts.DefaultFont.Name;
			cbLetter.Text = SystemFonts.DefaultFont.Name;

			cbEncode.Items.Clear();
			foreach (EncodingInfo info in Encoding.GetEncodings())
				cbEncode.Items.Add(info.Name);
			cbEncode.Text = Encoding.Default.BodyName;

			cbRange.Items.Clear();
			cbRange.Items.Add("ASCII Character".Translate());
			cbRange.Items.Add("All characters".Translate());
			cbRange.Items.Add("Specified character".Translate());

			cbRange.SelectedIndex = (HmiOptions.Language == 0) ? 1 : 0;

			if (m_custom)
			{
				cbRange.SelectedIndex = (HmiOptions.Language == 0) ? 2 : 0;
				textBox2.Text = m_params.Strings[0];
				tbFontName.Text = m_params.Strings[2];
				for (num2 = 0; num2 < cbHeight.Items.Count; num2++)
					if (cbHeight.Items[num2].ToString() == m_params.Strings[1])
					{
						cbHeight.SelectedIndex = num2;
						break;
					}
			}
		}
		#endregion

		#region FontCreator_FormClosing
		private void FontCreator_FormClosing(object sender, FormClosingEventArgs e)
		{
			m_closing = true;
		}
		#endregion

		#region fontStringAdd
		private void fontStringAdd(string text)
		{
			if (!m_fontString.Contains(text))
			{
				byte[] bytes = text.ToBytes();
				if (bytes.Length >= 0)
				{
					if (bytes[0] > 0x1F && bytes[0] < 0x7F)
						stringInsert(text);
					else if (bytes[0] > 0x7F && bytes.Length == 2)
						stringInsert(text);
				}
			}
		}

		private int getStringCode(string text)
		{
			int value = 0;
			byte[] bytes = text.ToBytes();
			if (bytes.Length == 1)
				return bytes[0];
			if (bytes.Length == 2)
				value = (bytes[0] << 8) + bytes[1];

			return value;
		}

		private void stringInsert(string text)
		{
			int stringCode = getStringCode(text);
			int length = m_fontString.Length;
			for (int i = 0; i < length; i++)
			{
				if (getStringCode(m_fontString.Substring(i, 1)) > stringCode)
				{
					m_fontString = m_fontString.Insert(i, text);
					return;
				}
			}
			m_fontString = m_fontString + text;
		}
		#endregion
	}
}
