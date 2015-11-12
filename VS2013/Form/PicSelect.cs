using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NextionEditor
{
	public class PicSelect : Form
	{
		#region Variables
		private Button btnOK;
		private IContainer components = null;
		private Label label1;
		private Panel panel1;
		private PicAdmin picAdmin;
		private RadioButton rbNormal;
		private RadioButton rbCrop;

		private FormParams m_params;
		private HmiApplication m_app;
		private int m_picIndex = -1;
		#endregion

		#region Constructor
		public PicSelect(HmiApplication app, FormParams formParams)
		{
			m_params = formParams;
			m_app = app;
			InitializeComponent();
			Utility.Translate(this);
		}
		#endregion

		#region btnOK_Click
		private void btnOK_Click(object sender, EventArgs e)
		{
			if (m_picIndex == -1)
				MessageBox.Show("No selected image".Translate());
			else
			{
				m_params.Strings[0] = m_picIndex.ToString();
				m_params.Strings[1] = rbNormal.Checked ? "0" : "1";
				base.DialogResult = DialogResult.OK;
			}
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
			this.label1 = new System.Windows.Forms.Label();
			this.rbNormal = new System.Windows.Forms.RadioButton();
			this.rbCrop = new System.Windows.Forms.RadioButton();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnOK = new System.Windows.Forms.Button();
			this.picAdmin = new PicAdmin();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(11, 25);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(121, 17);
			this.label1.TabIndex = 2;
			this.label1.Text = "Image loading way:";
			this.label1.Visible = false;
			// 
			// rbNormal
			// 
			this.rbNormal.AutoSize = true;
			this.rbNormal.Location = new System.Drawing.Point(14, 46);
			this.rbNormal.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.rbNormal.Name = "rbNormal";
			this.rbNormal.Size = new System.Drawing.Size(70, 21);
			this.rbNormal.TabIndex = 3;
			this.rbNormal.TabStop = true;
			this.rbNormal.Text = "Normal";
			this.rbNormal.UseVisualStyleBackColor = true;
			this.rbNormal.Visible = false;
			// 
			// rbCrop
			// 
			this.rbCrop.AutoSize = true;
			this.rbCrop.Location = new System.Drawing.Point(89, 46);
			this.rbCrop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.rbCrop.Name = "rbCrop";
			this.rbCrop.Size = new System.Drawing.Size(95, 21);
			this.rbCrop.TabIndex = 4;
			this.rbCrop.TabStop = true;
			this.rbCrop.Text = "Crop Image";
			this.rbCrop.UseVisualStyleBackColor = true;
			this.rbCrop.Visible = false;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.btnOK);
			this.panel1.Controls.Add(this.rbNormal);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.rbCrop);
			this.panel1.Location = new System.Drawing.Point(0, 398);
			this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(301, 92);
			this.panel1.TabIndex = 5;
			// 
			// btnOK
			// 
			this.btnOK.Location = new System.Drawing.Point(198, 35);
			this.btnOK.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(92, 43);
			this.btnOK.TabIndex = 5;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// picAdmin
			// 
			this.picAdmin.BackColor = System.Drawing.SystemColors.MenuHighlight;
			this.picAdmin.Location = new System.Drawing.Point(0, 0);
			this.picAdmin.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.picAdmin.Name = "picAdmin";
			this.picAdmin.Size = new System.Drawing.Size(301, 501);
			this.picAdmin.TabIndex = 0;
			this.picAdmin.PicSelect += new System.EventHandler(this.picAdmin_PicSelect);
			// 
			// PicSelect
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(302, 489);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.picAdmin);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.Name = "PicSelect";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Select Picture";
			this.Load += new System.EventHandler(this.PicSelect_Load);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion

		#region picAdmin_PicSelect
		private void picAdmin_PicSelect(object sender, EventArgs e)
		{
			m_picIndex = (int)sender;
		}
		#endregion

		#region PicSelect_Load
		private void PicSelect_Load(object sender, EventArgs e)
		{
			picAdmin.SetAppInfo(m_app);
			picAdmin.RefreshPictures();
			if (m_params.Strings[1] == "0")
				rbNormal.Checked = true;
			if (m_params.Strings[1] == "1")
				rbCrop.Checked = true;
		}
		#endregion
	}
}
