using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NextionEditor
{
    public class PageAdmin : UserControl
    {
		public event EventHandler PageChange;
		public event EventHandler SelectEnter;

		public PageImg MovePageImage;
		public PageImg[] PageImages;

		private int dpageimg = 0xffff;
		private HmiApplication m_app;

		private Button bt_DeleteAll;
        private Button bt_Delete;
        private Button bt_Add;
        private Button bt_Insert;
        private Button bt_Copy;
        private IContainer components = null;
        private Label label1;
        private Label label4;
        private Panel panel1;
        private Panel panel2;
        private Panel panel3;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;

        public PageAdmin()
        {
            InitializeComponent();
        }

        private void bt_DeleteAll_Click(object sender, EventArgs e)
        {
            m_app.DeleteAllPages();
            dpageimg = 0xffff;
            RefreshObject(dpageimg);
            SelectEnter(dpageimg, e);
            PageChange(null, null);
        }

        private void bt_Delete_Click(object sender, EventArgs e)
        {
            bt_Delete.Enabled = false;
            if (dpageimg != 0xffff)
            {
                m_app.DeletePage(dpageimg, true);
                if (dpageimg >= m_app.HmiPages.Count)
                    dpageimg = m_app.HmiPages.Count - 1;
                if (dpageimg < 0)
                    dpageimg = 0xffff;

                RefreshObject(dpageimg);
                SelectEnter(dpageimg, e);
                PageChange(null, null);
            }
            else
				MessageBox.Show("Select page".Translate());

            bt_Delete.Enabled = true;
        }

        private void bt_Add_Click(object sender, EventArgs e)
        {
            m_app.AddPage();
            RefreshObject(m_app.HmiPages.Count - 1);
            SelectEnter(dpageimg, e);
            PageChange(null, null);
        }

        private void bt_Insert_Click(object sender, EventArgs e)
        {
            m_app.InsertPage(dpageimg);
            RefreshObject(dpageimg);
            SelectEnter(dpageimg, e);
            PageChange(null, null);
        }

        private void bt_Copy_Click(object sender, EventArgs e)
        {
            if (dpageimg != 0xffff)
            {
                m_app.CopyPage(dpageimg);
                RefreshObject(m_app.HmiPages.Count - 1);
                SelectEnter(dpageimg, e);
                PageChange(null, null);
            }
            else
                MessageBox.Show("Select page".Translate());
            bt_Copy.Enabled = true;
        }

        private void buttonimg_MouseDown(object sender, EventArgs e)
        {
            PageImg pageimg = (PageImg) sender;
            if (dpageimg != 0xffff)
                PageImages[dpageimg].RefreshPageImg(false);
            
			pageimg.RefreshPageImg(true);
            dpageimg = pageimg.No;
            SelectEnter(dpageimg, e);
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
			this.label4 = new System.Windows.Forms.Label();
			this.bt_Delete = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.bt_DeleteAll = new System.Windows.Forms.Button();
			this.bt_Copy = new System.Windows.Forms.Button();
			this.bt_Insert = new System.Windows.Forms.Button();
			this.bt_Add = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// label4
			// 
			this.label4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(243)))), ((int)(((byte)(254)))));
			this.label4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label4.Location = new System.Drawing.Point(97, 1);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(278, 29);
			this.label4.TabIndex = 40;
			this.label4.Text = "Page";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// bt_Delete
			// 
			this.bt_Delete.Enabled = false;
			this.bt_Delete.Location = new System.Drawing.Point(1, 37);
			this.bt_Delete.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.bt_Delete.Name = "bt_Delete";
			this.bt_Delete.Size = new System.Drawing.Size(61, 30);
			this.bt_Delete.TabIndex = 45;
			this.bt_Delete.Text = "Delete";
			this.bt_Delete.UseVisualStyleBackColor = true;
			this.bt_Delete.Click += new System.EventHandler(this.bt_Delete_Click);
			// 
			// panel1
			// 
			this.panel1.AutoScroll = true;
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.Controls.Add(this.pictureBox2);
			this.panel1.Location = new System.Drawing.Point(100, 107);
			this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(278, 153);
			this.panel1.TabIndex = 44;
			// 
			// pictureBox2
			// 
			this.pictureBox2.BackColor = System.Drawing.Color.White;
			this.pictureBox2.Location = new System.Drawing.Point(-77, -157);
			this.pictureBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(153, 86);
			this.pictureBox2.TabIndex = 51;
			this.pictureBox2.TabStop = false;
			// 
			// bt_DeleteAll
			// 
			this.bt_DeleteAll.Enabled = false;
			this.bt_DeleteAll.Location = new System.Drawing.Point(69, 37);
			this.bt_DeleteAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.bt_DeleteAll.Name = "bt_DeleteAll";
			this.bt_DeleteAll.Size = new System.Drawing.Size(127, 30);
			this.bt_DeleteAll.TabIndex = 49;
			this.bt_DeleteAll.Text = "Del All";
			this.bt_DeleteAll.UseVisualStyleBackColor = true;
			this.bt_DeleteAll.Click += new System.EventHandler(this.bt_DeleteAll_Click);
			// 
			// bt_Copy
			// 
			this.bt_Copy.Enabled = false;
			this.bt_Copy.Location = new System.Drawing.Point(135, 4);
			this.bt_Copy.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.bt_Copy.Name = "bt_Copy";
			this.bt_Copy.Size = new System.Drawing.Size(61, 30);
			this.bt_Copy.TabIndex = 53;
			this.bt_Copy.Text = "Copy";
			this.bt_Copy.UseVisualStyleBackColor = true;
			this.bt_Copy.Click += new System.EventHandler(this.bt_Copy_Click);
			// 
			// bt_Insert
			// 
			this.bt_Insert.Enabled = false;
			this.bt_Insert.Location = new System.Drawing.Point(68, 4);
			this.bt_Insert.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.bt_Insert.Name = "bt_Insert";
			this.bt_Insert.Size = new System.Drawing.Size(61, 30);
			this.bt_Insert.TabIndex = 52;
			this.bt_Insert.Text = "Insert";
			this.bt_Insert.UseVisualStyleBackColor = true;
			this.bt_Insert.Click += new System.EventHandler(this.bt_Insert_Click);
			// 
			// bt_Add
			// 
			this.bt_Add.Enabled = false;
			this.bt_Add.Location = new System.Drawing.Point(1, 4);
			this.bt_Add.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.bt_Add.Name = "bt_Add";
			this.bt_Add.Size = new System.Drawing.Size(61, 30);
			this.bt_Add.TabIndex = 51;
			this.bt_Add.Text = "Add";
			this.bt_Add.UseVisualStyleBackColor = true;
			this.bt_Add.Click += new System.EventHandler(this.bt_Add_Click);
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(243)))), ((int)(((byte)(254)))));
			this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label1.Location = new System.Drawing.Point(279, 1);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(94, 26);
			this.label1.TabIndex = 53;
			this.label1.Text = "0";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(243)))), ((int)(((byte)(254)))));
			this.panel2.Controls.Add(this.panel3);
			this.panel2.Location = new System.Drawing.Point(97, 394);
			this.panel2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(278, 76);
			this.panel2.TabIndex = 54;
			this.panel2.Resize += new System.EventHandler(this.panel2_Resize);
			// 
			// panel3
			// 
			this.panel3.AutoScroll = true;
			this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(243)))), ((int)(((byte)(254)))));
			this.panel3.Controls.Add(this.bt_Copy);
			this.panel3.Controls.Add(this.pictureBox1);
			this.panel3.Controls.Add(this.bt_Add);
			this.panel3.Controls.Add(this.bt_Insert);
			this.panel3.Controls.Add(this.bt_DeleteAll);
			this.panel3.Controls.Add(this.bt_Delete);
			this.panel3.Location = new System.Drawing.Point(43, 2);
			this.panel3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(198, 72);
			this.panel3.TabIndex = 52;
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackColor = System.Drawing.Color.White;
			this.pictureBox1.Location = new System.Drawing.Point(-77, -157);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(153, 86);
			this.pictureBox1.TabIndex = 51;
			this.pictureBox1.TabStop = false;
			// 
			// PageAdmin
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.panel2);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "PageAdmin";
			this.Size = new System.Drawing.Size(398, 530);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.pageadmin_Paint);
			this.Resize += new System.EventHandler(this.pageadmin_Resize);
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
			this.panel2.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

        }
		#endregion

		private void pageadmin_Paint(object sender, PaintEventArgs e)
        {
            // this.DrawThisLine(Color.FromArgb(0x33, 0x99, 0xff), 1);
        }

        private void pageadmin_Resize(object sender, EventArgs e)
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
                panel1.Top = num;
                panel1.Left = 2;
                panel1.Width = base.Width - 4;
                panel1.Height = ((base.Height - num) - panel2.Height) - 2;
                panel2.Top = (base.Height - panel2.Height) - 2;
                panel2.Left = 2;
                panel2.Width = base.Width - 4;
            }
            catch
            {
            }
        }

        private void pageupdate_p(object sender, EventArgs e)
        {
            PageChange(null, null);
        }

        private void panel2_Resize(object sender, EventArgs e)
        {
			try
			{
				panel3.Left = panel2.Width - panel3.Width;
			}
			catch { }
        }

        public void RefreshObject(int select)
        {
            panel1.Controls.Clear();
            dpageimg = 0xffff;
            MovePageImage = null;
            if (m_app.HmiPages.Count > 0)
            {
                PageImages = new PageImg[m_app.HmiPages.Count];
                for (int i = 0; i < m_app.HmiPages.Count; i++)
                {
                    PageImages[i] = new PageImg();
                    PageImages[i].App = m_app;
                    PageImages[i].No = i;
                    PageImages[i].Page = m_app.HmiPages[i];
                    if (i == 0)
                        PageImages[i].Location = new Point(5, 0);
                    else
                        PageImages[i].Location = new Point(5, (PageImages[i - 1].Location.Y + PageImages[i - 1].Height) + 1);
                    
					PageImages[i].Size = new Size(panel1.Width - 0x19, 0x16);
                    panel1.Controls.Add(PageImages[i]);
                    PageImages[i].Visible = true;
                    PageImages[i].MouseClick += new MouseEventHandler(buttonimg_MouseDown);
                    PageImages[i].PageUpdate += new EventHandler(pageupdate_p);
                    PageImages[i].RefreshPageImg(false);
                }
                if (select < PageImages.Length)
                {
                    dpageimg = select;
                    PageImages[dpageimg].RefreshPageImg(true);
                    SelectEnter(dpageimg, null);
                }
            }
            label1.Text = m_app.HmiPages.Count.ToString();
        }

        public void SelectIndex(int index)
        {
            if (index < m_app.HmiPages.Count)
            {
                PageImages[index].RefreshPageImg(true);
                if (dpageimg != 0xffff)
                    PageImages[dpageimg].RefreshPageImg(false);
                dpageimg = index;
            }
        }

        public void SetAppInfo(HmiApplication app)
        {
            m_app = app;
            if (app == null)
            {
                panel1.Controls.Clear();
                bt_DeleteAll.Enabled = false;
                bt_Insert.Enabled = false;
                bt_Delete.Enabled = false;
                bt_Add.Enabled = false;
                bt_Copy.Enabled = false;
            }
            else
            {
                bt_DeleteAll.Enabled = true;
                bt_Insert.Enabled = true;
                bt_Copy.Enabled = true;
                bt_Delete.Enabled = true;
                bt_Add.Enabled = true;
            }
        }
    }
}

