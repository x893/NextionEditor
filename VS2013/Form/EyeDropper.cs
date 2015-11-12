using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NextionEditor
{
	public class EyeDropper : Form
	{
		private Button btnExit;
		private IContainer components = null;
		private Label labelCurrent;
		private Label labelHex;
		private Label labelDecimal;
		private LinkLabel linkSelectColor;
		private PictureBox pictureBox;
		private TextBox colorDecimal;

		public EyeDropper()
		{
			InitializeComponent();
			Utility.Translate(this);
		}

		private void btnExit_Click(object sender, EventArgs e)
		{
			base.Close();
		}

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
			this.linkSelectColor = new System.Windows.Forms.LinkLabel();
			this.labelCurrent = new System.Windows.Forms.Label();
			this.labelDecimal = new System.Windows.Forms.Label();
			this.colorDecimal = new System.Windows.Forms.TextBox();
			this.btnExit = new System.Windows.Forms.Button();
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.labelHex = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// linkSelectColor
			// 
			this.linkSelectColor.AutoSize = true;
			this.linkSelectColor.Location = new System.Drawing.Point(199, 64);
			this.linkSelectColor.Name = "linkSelectColor";
			this.linkSelectColor.Size = new System.Drawing.Size(78, 17);
			this.linkSelectColor.TabIndex = 0;
			this.linkSelectColor.TabStop = true;
			this.linkSelectColor.Text = "Select Color";
			this.linkSelectColor.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkSelectColor_LinkClicked);
			// 
			// labelCurrent
			// 
			this.labelCurrent.Location = new System.Drawing.Point(14, 64);
			this.labelCurrent.Name = "labelCurrent";
			this.labelCurrent.Size = new System.Drawing.Size(140, 17);
			this.labelCurrent.TabIndex = 1;
			this.labelCurrent.Text = "Current color:";
			this.labelCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDecimal
			// 
			this.labelDecimal.AutoSize = true;
			this.labelDecimal.Location = new System.Drawing.Point(28, 140);
			this.labelDecimal.Name = "labelDecimal";
			this.labelDecimal.Size = new System.Drawing.Size(131, 17);
			this.labelDecimal.TabIndex = 4;
			this.labelDecimal.Text = "Decimal color values:";
			// 
			// colorDecimal
			// 
			this.colorDecimal.Location = new System.Drawing.Point(161, 137);
			this.colorDecimal.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.colorDecimal.Name = "colorDecimal";
			this.colorDecimal.Size = new System.Drawing.Size(80, 25);
			this.colorDecimal.TabIndex = 5;
			// 
			// btnExit
			// 
			this.btnExit.Location = new System.Drawing.Point(297, 220);
			this.btnExit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new System.Drawing.Size(87, 44);
			this.btnExit.TabIndex = 6;
			this.btnExit.Text = "Exit";
			this.btnExit.UseVisualStyleBackColor = true;
			this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
			// 
			// pictureBox
			// 
			this.pictureBox.BackColor = System.Drawing.Color.Black;
			this.pictureBox.Location = new System.Drawing.Point(161, 64);
			this.pictureBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(31, 35);
			this.pictureBox.TabIndex = 2;
			this.pictureBox.TabStop = false;
			// 
			// labelHex
			// 
			this.labelHex.AutoSize = true;
			this.labelHex.Location = new System.Drawing.Point(248, 140);
			this.labelHex.Name = "labelHex";
			this.labelHex.Size = new System.Drawing.Size(90, 17);
			this.labelHex.TabIndex = 7;
			this.labelHex.Text = "(Hexadecimal)";
			// 
			// EyeDropper
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(399, 281);
			this.Controls.Add(this.labelHex);
			this.Controls.Add(this.btnExit);
			this.Controls.Add(this.colorDecimal);
			this.Controls.Add(this.labelDecimal);
			this.Controls.Add(this.pictureBox);
			this.Controls.Add(this.labelCurrent);
			this.Controls.Add(this.linkSelectColor);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.Name = "EyeDropper";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "EyeDropper";
			this.Load += new System.EventHandler(this.EyeDropper_Load);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private void linkSelectColor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ColorDialog dialog = new ColorDialog();
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				pictureBox.BackColor = dialog.Color;
				colorDecimal.Text = Utility.Get16Color(pictureBox.BackColor).ToString();
				labelHex.Text =
					"(" + "Hexadecimal".Translate()
					+ ":0x" + Convert.ToString((int)Utility.Get16Color(pictureBox.BackColor), 16)
					+ ")";
			}
		}

		private void EyeDropper_Load(object sender, EventArgs e)
		{
			colorDecimal.Text = Utility.Get16Color(pictureBox.BackColor).ToString();
			labelHex.Text = "(" + "Hexadecimal".Translate()
				+ ":0x" + Convert.ToString((int)Utility.Get16Color(pictureBox.BackColor), 16) + ")";
		}
	}
}
