using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NextionEditor
{
    public class PicAdmin : UserControl
    {
        private Button ButtonDeleteAll;
        private Button btn_Delete;
        private Button btn_Add;
        private Button ButtonInsert;
        private Button ButtonReplace;
        private IContainer components = null;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem mi_Export;
        private Label label1;
        private Label label4;
        private Panel panelList;
        private Panel panel2;
        private Panel panel3;
        private PictureBox pictureBox1;
        private ToolStripMenuItem mi_DeleteAll;
        private ToolStripButton toolStripButton1;
        private ToolStripMenuItem mi_Add;
        private ToolStripMenuItem mi_Delete;
        private ToolStripMenuItem mi_Insert;
        private ToolStripMenuItem mi_Replace;

        public event EventHandler PicSelect;
        public event EventHandler PicUpdate;
		public ImgPicture DImgPic;

		private HmiApplication m_app;

        public PicAdmin()
        {
            InitializeComponent();
            Utility.Translate(contextMenu);
        }

		private void btn_DeleteAll_Click(object sender, EventArgs e)
        {
			if (m_app.Pictures.Count > 0)
			{
				m_app.DeleteAllPictures();
				RefreshPictures();
				PicUpdate(null, null);
			}
		}

        private void btn_Delete_Click(object sender, EventArgs e)
        {
			if (DImgPic != null)
			{
				m_app.DeletePicture(DImgPic.No);
				RefreshPictures();
				PicUpdate(null, null);
			}
		}

        private void btn_Add_Click(object sender, EventArgs e)
        {
			Form form = new AddPicture(m_app, 0xffff);
			form.ShowDialog();
			if (form.DialogResult == DialogResult.OK)
			{
				RefreshPictures();
				PicUpdate(null, null);
			}
		}
		private void mi_Add_Click(object sender, EventArgs e)
		{
			MessageBox.Show(contextMenu.GetType().ToString());
			btn_Add_Click(sender, e);
		}

		private void btn_Insert_Click(object sender, EventArgs e)
		{
			if (DImgPic == null)
				return;

			Form form = new AddPicture(m_app, DImgPic.No);
			form.ShowDialog();
			if (form.DialogResult == DialogResult.OK)
			{
				RefreshPictures();
				PicUpdate(null, null);
			}
		}

		private void btn_Replace_Click(object sender, EventArgs e)
		{
			if (DImgPic == null)
				return;

			Form form = new AddPicture(m_app, DImgPic.No);
			form.ShowDialog();
			if (form.DialogResult == DialogResult.OK)
			{
				m_app.DeletePicture(DImgPic.No + 1);
				RefreshPictures();
				PicUpdate(null, null);
			}
		}

        private void imgPicture_DoubleClick(object sender, EventArgs e)
        {
            ImgPicture imgpicture = (ImgPicture) sender;
            new PicView(m_app, imgpicture.No).ShowDialog();
        }

        private void imgPicture_MouseDown(object sender, EventArgs e)
        {
			try
			{
				ImgPicture imgpicture = (ImgPicture)sender;
				if (DImgPic != null)
				{
					DImgPic.ViewPic(panelList.BackColor, Brushes.Black);
				}
				DImgPic = imgpicture;
				DImgPic.ViewPic(Color.Blue, Brushes.White);
				PicSelect(DImgPic.No, null);
			}
			catch { }
        }

		private void exportPic()
		{
			Bitmap bitmap = null;
			if (DImgPic == null)
				return;

			try
			{
				SaveFileDialog op = new SaveFileDialog {
											Filter = "jpg|*.jpg|bmp|*.bmp|png|*.png".Translate()
										};
				Utility.SetInitialPath(op, "topic");
				if (op.ShowDialog() == DialogResult.OK)
				{
					Utility.SavePath(op, "topic");
					using (bitmap = Utility.GetBitmap(m_app.PictureImages[DImgPic.No], m_app.Pictures[DImgPic.No], true))
					{
						EncoderParameters encoderParams = new EncoderParameters();
						long[] numArray = new long[] { 100L };
						EncoderParameter parameter = new EncoderParameter(Encoder.Quality, numArray);
						encoderParams.Param[0] = parameter;
						if (Path.GetExtension(op.FileName) == ".jpg")
							bitmap.Save(op.FileName, getCodecInfo("image/jpeg"), encoderParams);
						else if (Path.GetExtension(op.FileName) == ".bmp")
							bitmap.Save(op.FileName, getCodecInfo("image/bmp"), encoderParams);
						else if (Path.GetExtension(op.FileName) == ".png")
							bitmap.Save(op.FileName, getCodecInfo("image/png"), encoderParams);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private ImageCodecInfo getCodecInfo(string mimeType)
		{
			ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
			foreach (ImageCodecInfo info in imageEncoders)
				if (info.MimeType == mimeType)
					return info;
			MessageBox.Show("not find ImageCodecInfo" + mimeType);
			return null;
		}

        private void mi_Export_Click(object sender, EventArgs e)
        {
            exportPic();
        }

        private void panel2_Resize(object sender, EventArgs e)
        {
			try
			{
				panel3.Left = panel2.Width - panel3.Width;
			}
			catch { }
        }

        private void picadmin_Paint(object sender, PaintEventArgs e)
        {
            // this.DrawThisLine(Color.FromArgb(0x33, 0x99, 0xff), 1);
        }

        private void picadmin_Resize(object sender, EventArgs e)
        {
            int num = 1;
			try
			{
				label4.Top = num;
				label4.Left = 2;
				label4.Width = base.Width - 4;
				label1.Top = num;
				label1.Left = (base.Width - label1.Width) - 2;
				num += label4.Height;
				panelList.Top = num;
				panelList.Left = 2;
				panelList.Width = base.Width - 4;
				panelList.Height = ((base.Height - num) - panel2.Height) - 2;
				panel2.Top = (base.Height - panel2.Height) - 2;
				panel2.Left = 2;
				panel2.Width = base.Width - 4;
				if (base.Width > 10)
					RefreshPictures();
			}
			catch { }
        }

        public void RefreshPictures()
        {
            DImgPic = null;
            panelList.Controls.Clear();

            if (m_app != null)
            {
                try
                {
					ImgPicture prevPicture = null;
					for (int i = 0; i < m_app.Pictures.Count; i++)
                        if (m_app.Pictures[i].W < 2000)
                        {
                            ImgPicture imgPicture = new ImgPicture();
                            if (prevPicture == null)
                                imgPicture.Location = new Point(0, 0);
                            else
                                imgPicture.Location = new Point(0, prevPicture.Location.Y + prevPicture.Height);
                            
							imgPicture.No = i;
                            imgPicture.Size = new Size((panelList.Width > 24) ? panelList.Width - 24 : 0, 10);
                            panelList.Controls.Add(imgPicture);

                            imgPicture.Visible = true;
                            imgPicture.Focus();
                            imgPicture.MouseDown += new MouseEventHandler(imgPicture_MouseDown);
                            imgPicture.DoubleClick += new EventHandler(imgPicture_DoubleClick);
                            imgPicture.ContextMenuStrip = contextMenu;
                            imgPicture.App = m_app;
                            imgPicture.ViewPic(Color.White, Brushes.Black);

                            prevPicture = imgPicture;
                        }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                label1.Text = m_app.Pictures.Count.ToString();
            }
        }

        public void SetAppInfo(HmiApplication app)
        {
            m_app = app;
            if (app == null)
            {
                panelList.Controls.Clear();
                ButtonDeleteAll.Enabled = false;
                ButtonInsert.Enabled = false;
                ButtonReplace.Enabled = false;
                btn_Delete.Enabled = false;
                btn_Add.Enabled = false;
            }
            else
            {
                ButtonDeleteAll.Enabled = true;
                ButtonInsert.Enabled = true;
                ButtonReplace.Enabled = true;
                btn_Delete.Enabled = true;
                btn_Add.Enabled = true;
            }
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PicAdmin));
			this.label4 = new System.Windows.Forms.Label();
			this.btn_Add = new System.Windows.Forms.Button();
			this.btn_Delete = new System.Windows.Forms.Button();
			this.panelList = new System.Windows.Forms.Panel();
			this.ButtonDeleteAll = new System.Windows.Forms.Button();
			this.ButtonInsert = new System.Windows.Forms.Button();
			this.ButtonReplace = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.mi_Add = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_Delete = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_Insert = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_Replace = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_Export = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_DeleteAll = new System.Windows.Forms.ToolStripMenuItem();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.contextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// label4
			// 
			this.label4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(243)))), ((int)(((byte)(254)))));
			this.label4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label4.Location = new System.Drawing.Point(22, 46);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(338, 29);
			this.label4.TabIndex = 40;
			this.label4.Text = "Picture";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// btn_Add
			// 
			this.btn_Add.Enabled = false;
			this.btn_Add.Location = new System.Drawing.Point(3, 4);
			this.btn_Add.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btn_Add.Name = "btn_Add";
			this.btn_Add.Size = new System.Drawing.Size(70, 30);
			this.btn_Add.TabIndex = 46;
			this.btn_Add.Text = "Add";
			this.btn_Add.UseVisualStyleBackColor = true;
			this.btn_Add.Click += new System.EventHandler(this.btn_Add_Click);
			// 
			// btn_Delete
			// 
			this.btn_Delete.Enabled = false;
			this.btn_Delete.Location = new System.Drawing.Point(77, 4);
			this.btn_Delete.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btn_Delete.Name = "btn_Delete";
			this.btn_Delete.Size = new System.Drawing.Size(70, 30);
			this.btn_Delete.TabIndex = 45;
			this.btn_Delete.Text = "Delete";
			this.btn_Delete.UseVisualStyleBackColor = true;
			this.btn_Delete.Click += new System.EventHandler(this.btn_Delete_Click);
			// 
			// panelList
			// 
			this.panelList.AutoScroll = true;
			this.panelList.BackColor = System.Drawing.Color.White;
			this.panelList.Location = new System.Drawing.Point(20, 169);
			this.panelList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.panelList.Name = "panelList";
			this.panelList.Size = new System.Drawing.Size(338, 289);
			this.panelList.TabIndex = 44;
			// 
			// ButtonDeleteAll
			// 
			this.ButtonDeleteAll.Enabled = false;
			this.ButtonDeleteAll.Location = new System.Drawing.Point(77, 37);
			this.ButtonDeleteAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ButtonDeleteAll.Name = "ButtonDeleteAll";
			this.ButtonDeleteAll.Size = new System.Drawing.Size(143, 30);
			this.ButtonDeleteAll.TabIndex = 49;
			this.ButtonDeleteAll.Text = "Del All";
			this.ButtonDeleteAll.UseVisualStyleBackColor = true;
			this.ButtonDeleteAll.Click += new System.EventHandler(this.btn_DeleteAll_Click);
			// 
			// ButtonInsert
			// 
			this.ButtonInsert.Enabled = false;
			this.ButtonInsert.Location = new System.Drawing.Point(3, 37);
			this.ButtonInsert.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ButtonInsert.Name = "ButtonInsert";
			this.ButtonInsert.Size = new System.Drawing.Size(70, 30);
			this.ButtonInsert.TabIndex = 50;
			this.ButtonInsert.Text = "Insert";
			this.ButtonInsert.UseVisualStyleBackColor = true;
			this.ButtonInsert.Click += new System.EventHandler(this.btn_Insert_Click);
			// 
			// ButtonReplace
			// 
			this.ButtonReplace.Enabled = false;
			this.ButtonReplace.Location = new System.Drawing.Point(150, 4);
			this.ButtonReplace.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ButtonReplace.Name = "ButtonReplace";
			this.ButtonReplace.Size = new System.Drawing.Size(70, 30);
			this.ButtonReplace.TabIndex = 51;
			this.ButtonReplace.Text = "Replace";
			this.ButtonReplace.UseVisualStyleBackColor = true;
			this.ButtonReplace.Click += new System.EventHandler(this.btn_Replace_Click);
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(243)))), ((int)(((byte)(254)))));
			this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label1.Location = new System.Drawing.Point(241, 46);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(117, 29);
			this.label1.TabIndex = 54;
			this.label1.Text = "0";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(243)))), ((int)(((byte)(254)))));
			this.panel2.Controls.Add(this.panel3);
			this.panel2.Location = new System.Drawing.Point(20, 497);
			this.panel2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(340, 76);
			this.panel2.TabIndex = 55;
			this.panel2.Resize += new System.EventHandler(this.panel2_Resize);
			// 
			// panel3
			// 
			this.panel3.AutoScroll = true;
			this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(243)))), ((int)(((byte)(254)))));
			this.panel3.Controls.Add(this.btn_Delete);
			this.panel3.Controls.Add(this.ButtonReplace);
			this.panel3.Controls.Add(this.btn_Add);
			this.panel3.Controls.Add(this.ButtonDeleteAll);
			this.panel3.Controls.Add(this.ButtonInsert);
			this.panel3.Location = new System.Drawing.Point(72, 2);
			this.panel3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(226, 72);
			this.panel3.TabIndex = 52;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(577, 310);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(48, 95);
			this.pictureBox1.TabIndex = 43;
			this.pictureBox1.TabStop = false;
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
			this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(123, 22);
			this.toolStripButton1.Text = "toolStripButton1";
			// 
			// contextMenu
			// 
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mi_Add,
            this.mi_Delete,
            this.mi_Insert,
            this.mi_Replace,
            this.mi_Export,
            this.mi_DeleteAll});
			this.contextMenu.Name = "contextMenu";
			this.contextMenu.Size = new System.Drawing.Size(116, 136);
			// 
			// mi_Add
			// 
			this.mi_Add.Name = "mi_Add";
			this.mi_Add.Size = new System.Drawing.Size(115, 22);
			this.mi_Add.Text = "Add";
			this.mi_Add.Click += new System.EventHandler(this.mi_Add_Click);
			// 
			// mi_Delete
			// 
			this.mi_Delete.Name = "mi_Delete";
			this.mi_Delete.Size = new System.Drawing.Size(115, 22);
			this.mi_Delete.Text = "Delete";
			this.mi_Delete.Click += new System.EventHandler(this.btn_Delete_Click);
			// 
			// mi_Insert
			// 
			this.mi_Insert.Name = "mi_Insert";
			this.mi_Insert.Size = new System.Drawing.Size(115, 22);
			this.mi_Insert.Text = "Insert";
			this.mi_Insert.Click += new System.EventHandler(this.btn_Insert_Click);
			// 
			// mi_Replace
			// 
			this.mi_Replace.Name = "mi_Replace";
			this.mi_Replace.Size = new System.Drawing.Size(115, 22);
			this.mi_Replace.Text = "Replace";
			this.mi_Replace.Click += new System.EventHandler(this.btn_Replace_Click);
			// 
			// mi_Export
			// 
			this.mi_Export.Name = "mi_Export";
			this.mi_Export.Size = new System.Drawing.Size(115, 22);
			this.mi_Export.Text = "Export";
			this.mi_Export.Click += new System.EventHandler(this.mi_Export_Click);
			// 
			// mi_DeleteAll
			// 
			this.mi_DeleteAll.Name = "mi_DeleteAll";
			this.mi_DeleteAll.Size = new System.Drawing.Size(115, 22);
			this.mi_DeleteAll.Text = "Del All";
			this.mi_DeleteAll.Click += new System.EventHandler(this.btn_DeleteAll_Click);
			// 
			// PicAdmin
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.panelList);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.panel2);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "PicAdmin";
			this.Size = new System.Drawing.Size(378, 597);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.picadmin_Paint);
			this.Resize += new System.EventHandler(this.picadmin_Resize);
			this.panel2.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.contextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
