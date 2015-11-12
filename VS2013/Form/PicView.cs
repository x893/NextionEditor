using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NextionEditor
{
    public class PicView : Form
    {
		private HmiApplication m_app;
		private int m_picIndex = 0;
		
		private IContainer components = null;
		private Label label;
        private Panel panel;
        private PictureBox pictureBox;

        public PicView(HmiApplication app, int picIndex)
        {
            m_app = app;
            m_picIndex = picIndex;
            InitializeComponent();
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
			this.panel = new System.Windows.Forms.Panel();
			this.label = new System.Windows.Forms.Label();
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// panel
			// 
			this.panel.AutoScroll = true;
			this.panel.BackColor = System.Drawing.Color.Silver;
			this.panel.Controls.Add(this.label);
			this.panel.Controls.Add(this.pictureBox);
			this.panel.Location = new System.Drawing.Point(14, 17);
			this.panel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.panel.Name = "panel";
			this.panel.Size = new System.Drawing.Size(415, 366);
			this.panel.TabIndex = 0;
			// 
			// label
			// 
			this.label.AutoSize = true;
			this.label.Location = new System.Drawing.Point(16, 13);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(43, 17);
			this.label.TabIndex = 1;
			this.label.Text = "label1";
			// 
			// pictureBox
			// 
			this.pictureBox.Location = new System.Drawing.Point(20, 34);
			this.pictureBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(376, 310);
			this.pictureBox.TabIndex = 0;
			this.pictureBox.TabStop = false;
			// 
			// PicView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ClientSize = new System.Drawing.Size(446, 401);
			this.Controls.Add(this.panel);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.Name = "PicView";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Image Preview";
			this.Load += new System.EventHandler(this.PicView_Load);
			this.panel.ResumeLayout(false);
			this.panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.ResumeLayout(false);

        }
		#endregion

        private void PicView_Load(object sender, EventArgs e)
        {
            try
            {
				Bitmap bitmap = Utility.GetBitmap(m_app.PictureImages[m_picIndex], m_app.Pictures[m_picIndex], false);
                pictureBox.Width = bitmap.Width;
                pictureBox.Height = bitmap.Height;
                int num = (panel.Width - pictureBox.Width) / 2;
                int num2 = (panel.Height - pictureBox.Height) / 2;

                if (num < 0)
                    num = 0;
                if (num2 < 30)
                    num2 = 30;

                label.Left = num;
                label.Top = (num2 - label.Height) - 3;
                label.Text = "Size:" + pictureBox.Width.ToString() + "*" + pictureBox.Height.ToString();
                pictureBox.Top = num2;
                pictureBox.Left = num;
                pictureBox.Image = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

