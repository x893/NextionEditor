using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NextionEditor
{
	public class DeviceParameters : Form
	{
		private HmiApplication m_app;

		private Button btnOK;
		private Button btnCancel;
		private ComboBox DisplaySize;
		private IContainer components = null;
		private GroupBox groupBox1;
		private Label label1;
		private Label label2;
		private Label label3;
		private RadioButton rbHorizontal;
		private RadioButton rbVertical;

		public DeviceParameters(HmiApplication app)
		{
			m_app = app;
			InitializeComponent();
			Utility.Translate(this);
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(DisplaySize.Text))
			{
				string[] tokens = DisplaySize.Text.Split(new char[] { 'x' });
				if (tokens.Length == 2)
				{
					if (rbHorizontal.Checked)
						m_app.IsPotrait = false;
					else
						m_app.IsPotrait = true;

					m_app.LcdWidth = (m_app.IsPotrait) ? ushort.Parse(tokens[1]) : ushort.Parse(tokens[0]);
					m_app.LcdHeight = (m_app.IsPotrait) ? ushort.Parse(tokens[0]) : ushort.Parse(tokens[1]);
					convertPictures();
					foreach (HmiPage mpage in m_app.HmiPages)
						mpage.HmiObjects[0].SetScreenXY();
					base.DialogResult = DialogResult.OK;
				}
			}
			base.Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		private void DeviceParameters_Load(object sender, EventArgs e)
		{
			DisplaySize.Items.Clear();
			DisplaySize.Items.Add("220x176");
			DisplaySize.Items.Add("320x240");
			DisplaySize.Items.Add("400x240");
			DisplaySize.Items.Add("480x272");
			DisplaySize.Items.Add("480x320");
			DisplaySize.Items.Add("800x480");
			DisplaySize.Items.Add("1024x600");

			for (int i = 0; i < DisplaySize.Items.Count; i++)
			{
				if (!m_app.IsPotrait)
				{
					if (DisplaySize.Items[i].ToString() == (m_app.LcdWidth.ToString() + "x" + m_app.LcdHeight.ToString()))
					{
						DisplaySize.SelectedIndex = i;
						break;
					}
				}
				else if (DisplaySize.Items[i].ToString() == (m_app.LcdHeight.ToString() + "x" + m_app.LcdWidth.ToString()))
				{
					DisplaySize.SelectedIndex = i;
					break;
				}
			}

			if (m_app.IsPotrait)
				rbVertical.Checked = true;
			else
				rbHorizontal.Checked = true;
		}

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
			this.label1 = new System.Windows.Forms.Label();
			this.DisplaySize = new System.Windows.Forms.ComboBox();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.rbVertical = new System.Windows.Forms.RadioButton();
			this.rbHorizontal = new System.Windows.Forms.RadioButton();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(142, 26);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(236, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "Select Resolution of the Nextion Device";
			// 
			// DisplaySize
			// 
			this.DisplaySize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.DisplaySize.FormattingEnabled = true;
			this.DisplaySize.Location = new System.Drawing.Point(145, 47);
			this.DisplaySize.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.DisplaySize.Name = "DisplaySize";
			this.DisplaySize.Size = new System.Drawing.Size(207, 25);
			this.DisplaySize.TabIndex = 1;
			// 
			// btnOK
			// 
			this.btnOK.Location = new System.Drawing.Point(133, 245);
			this.btnOK.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(90, 43);
			this.btnOK.TabIndex = 2;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(230, 245);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(90, 43);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// label2
			// 
			this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
			this.label2.Location = new System.Drawing.Point(66, 162);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(350, 80);
			this.label2.TabIndex = 4;
			this.label2.Text = "Select the right parameters";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.rbVertical);
			this.groupBox1.Controls.Add(this.rbHorizontal);
			this.groupBox1.Location = new System.Drawing.Point(145, 84);
			this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.groupBox1.Size = new System.Drawing.Size(208, 73);
			this.groupBox1.TabIndex = 5;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Display direction";
			// 
			// rbVertical
			// 
			this.rbVertical.AutoSize = true;
			this.rbVertical.Location = new System.Drawing.Point(129, 29);
			this.rbVertical.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.rbVertical.Name = "rbVertical";
			this.rbVertical.Size = new System.Drawing.Size(68, 21);
			this.rbVertical.TabIndex = 1;
			this.rbVertical.Text = "Vertical";
			this.rbVertical.UseVisualStyleBackColor = true;
			// 
			// rbHorizontal
			// 
			this.rbHorizontal.AutoSize = true;
			this.rbHorizontal.Checked = true;
			this.rbHorizontal.Location = new System.Drawing.Point(23, 29);
			this.rbHorizontal.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.rbHorizontal.Name = "rbHorizontal";
			this.rbHorizontal.Size = new System.Drawing.Size(86, 21);
			this.rbHorizontal.TabIndex = 0;
			this.rbHorizontal.TabStop = true;
			this.rbHorizontal.Text = "Horizontal";
			this.rbHorizontal.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.ForeColor = System.Drawing.Color.Black;
			this.label3.Location = new System.Drawing.Point(14, 293);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(43, 17);
			this.label3.TabIndex = 6;
			this.label3.Text = "label3";
			this.label3.Visible = false;
			// 
			// DeviceParameters
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(510, 323);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.DisplaySize);
			this.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.Name = "DeviceParameters";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Nextion Device Parameters";
			this.Load += new System.EventHandler(this.DeviceParameters_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private void convertPictures()
		{
			for (int i = 0; i < m_app.Pictures.Count; i++)
			{
				label3.Visible = true;
				if (m_app.Pictures[i].IsPotrait != (m_app.IsPotrait ? 1 : 0))
				{
					label3.Text = "Converting resource file:".Translate() + i.ToString() + "/" + m_app.Pictures.Count.ToString();
					Application.DoEvents();

					m_app.PictureImages[i] = cropPicture(i);
					InfoPicture pictureInfo = m_app.Pictures[i];
					pictureInfo.IsPotrait = (byte)(m_app.IsPotrait ? 1 : 0);
					m_app.Pictures[i] = pictureInfo;
				}
			}
		}

		private byte[] cropPicture(int index)
		{
			int dstIdx = 0;
			byte[] dst = new byte[m_app.PictureImages[index].Length];
			byte[] buffer2 = m_app.PictureImages[index];
			for (int i = 0; i < m_app.Pictures[index].H; i++)
			{
				int srcIdx = (((i + 1) * m_app.Pictures[index].W) * 2) - 2;
				for (int j = 0; j < m_app.Pictures[index].W; j++)
				{
					dst[dstIdx] = buffer2[srcIdx];
					dst[dstIdx + 1] = buffer2[srcIdx + 1];
					dstIdx += 2;
					srcIdx -= 2;
				}
			}
			return dst;
		}
	}
}
