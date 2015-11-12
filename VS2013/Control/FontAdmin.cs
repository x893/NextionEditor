using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace NextionEditor
{
    public class FontAdmin : UserControl
    {
        private Button btnDelAll;
        private Button btnDelete;
        private Button btnAdd;
        private Button btnPreview;
        private IContainer components = null;
        private Label lblFontCount;
        private Label label4;
        private ListBox listFonts;
        private Panel panel1;
        private Panel panel2;
        private Panel panel3;
		private HmiApplication m_app;

        public event EventHandler FontUpdate;

        public FontAdmin()
        {
            InitializeComponent();
        }

		#region AddFont
		public void AddFont(string path)
        {
            int size = Marshal.SizeOf(typeof(InfoFont));

			using (StreamReader reader = new StreamReader(path))
			{
				byte[] buffer = new byte[size];
				reader.BaseStream.Read(buffer, 0, size);
				InfoFont item = Utility.ToStruct<InfoFont>(buffer);
				m_app.Fonts.Add(item);

				buffer = new byte[item.Size];
				reader.BaseStream.Read(buffer, 0, buffer.Length);
				m_app.FontImages.Add(buffer);

				reader.Close();
			}
			FontUpdate(this, EventArgs.Empty);
		}
		#endregion

		private void btnDelAll_Click(object sender, EventArgs e)
        {
            if (m_app.Fonts.Count > 0)
            {
                m_app.DeleteAllFonts();
                RefreshFonts();
				FontUpdate(this, EventArgs.Empty);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if ((listFonts.Items.Count > 0) && (listFonts.SelectedIndex > -1))
            {
                m_app.DeleteFont(listFonts.SelectedIndex);
                RefreshFonts();
				FontUpdate(this, EventArgs.Empty);
            }
        }

		#region btnAdd_Click
		private void btnAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog {
				Filter = "Font|*.zi|All file|*.*".Translate(),
                Multiselect = true
            };
            Utility.SetInitialPath(op, "font");
            if (op.ShowDialog() == DialogResult.OK)
            {
                Utility.SavePath(op, "font");
                foreach (string str in op.FileNames)
                    AddFont(str);
                RefreshFonts();
				FontUpdate(this, EventArgs.Empty);
            }
        }
		#endregion
		private void btnPreview_Click(object sender, EventArgs e)
        {
            if (listFonts.SelectedIndex >= 0)
            {
				FormParams formParams = new FormParams();
				formParams.Strings = new string[3] { "", null, null };
				new FontView(m_app, listFonts.SelectedIndex, formParams).ShowDialog();
                if (!string.IsNullOrEmpty(formParams.Strings[1]))
                {
                    RefreshFonts();
					FontUpdate(this, EventArgs.Empty);
                }
            }
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
			this.btnAdd = new System.Windows.Forms.Button();
			this.btnDelete = new System.Windows.Forms.Button();
			this.btnDelAll = new System.Windows.Forms.Button();
			this.listFonts = new System.Windows.Forms.ListBox();
			this.btnPreview = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.lblFontCount = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnAdd
			// 
			this.btnAdd.Enabled = false;
			this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.btnAdd.Location = new System.Drawing.Point(4, 4);
			this.btnAdd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(61, 30);
			this.btnAdd.TabIndex = 46;
			this.btnAdd.Text = "Add";
			this.btnAdd.UseVisualStyleBackColor = true;
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			// 
			// btnDelete
			// 
			this.btnDelete.Enabled = false;
			this.btnDelete.Location = new System.Drawing.Point(71, 4);
			this.btnDelete.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.Size = new System.Drawing.Size(61, 30);
			this.btnDelete.TabIndex = 45;
			this.btnDelete.Text = "Delete";
			this.btnDelete.UseVisualStyleBackColor = true;
			this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
			// 
			// btnDelAll
			// 
			this.btnDelAll.Enabled = false;
			this.btnDelAll.Location = new System.Drawing.Point(139, 37);
			this.btnDelAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnDelAll.Name = "btnDelAll";
			this.btnDelAll.Size = new System.Drawing.Size(91, 30);
			this.btnDelAll.TabIndex = 49;
			this.btnDelAll.Text = "Del All";
			this.btnDelAll.UseVisualStyleBackColor = true;
			this.btnDelAll.Click += new System.EventHandler(this.btnDelAll_Click);
			// 
			// listFonts
			// 
			this.listFonts.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listFonts.FormattingEnabled = true;
			this.listFonts.HorizontalScrollbar = true;
			this.listFonts.ItemHeight = 17;
			this.listFonts.Location = new System.Drawing.Point(27, 18);
			this.listFonts.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.listFonts.Name = "listFonts";
			this.listFonts.Size = new System.Drawing.Size(245, 204);
			this.listFonts.TabIndex = 50;
			// 
			// btnPreview
			// 
			this.btnPreview.Enabled = false;
			this.btnPreview.Location = new System.Drawing.Point(139, 4);
			this.btnPreview.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnPreview.Name = "btnPreview";
			this.btnPreview.Size = new System.Drawing.Size(91, 30);
			this.btnPreview.TabIndex = 51;
			this.btnPreview.Text = "Preview";
			this.btnPreview.UseVisualStyleBackColor = true;
			this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
			// 
			// label4
			// 
			this.label4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(243)))), ((int)(((byte)(254)))));
			this.label4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label4.Location = new System.Drawing.Point(31, 10);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(313, 29);
			this.label4.TabIndex = 56;
			this.label4.Text = "Font";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblFontCount
			// 
			this.lblFontCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(243)))), ((int)(((byte)(254)))));
			this.lblFontCount.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.lblFontCount.Location = new System.Drawing.Point(264, 10);
			this.lblFontCount.Name = "lblFontCount";
			this.lblFontCount.Size = new System.Drawing.Size(78, 29);
			this.lblFontCount.TabIndex = 57;
			this.lblFontCount.Text = "0";
			this.lblFontCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(243)))), ((int)(((byte)(254)))));
			this.panel2.Controls.Add(this.panel3);
			this.panel2.Location = new System.Drawing.Point(31, 443);
			this.panel2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(308, 76);
			this.panel2.TabIndex = 58;
			this.panel2.Resize += new System.EventHandler(this.panel2_Resize);
			// 
			// panel3
			// 
			this.panel3.AutoScroll = true;
			this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(243)))), ((int)(((byte)(254)))));
			this.panel3.Controls.Add(this.btnDelete);
			this.panel3.Controls.Add(this.btnDelAll);
			this.panel3.Controls.Add(this.btnPreview);
			this.panel3.Controls.Add(this.btnAdd);
			this.panel3.Location = new System.Drawing.Point(77, 2);
			this.panel3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(231, 72);
			this.panel3.TabIndex = 60;
			// 
			// panel1
			// 
			this.panel1.AutoScroll = true;
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.Controls.Add(this.listFonts);
			this.panel1.Location = new System.Drawing.Point(35, 48);
			this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(309, 255);
			this.panel1.TabIndex = 59;
			this.panel1.Resize += new System.EventHandler(this.panel1_Resize);
			// 
			// FontAdmin
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.lblFontCount);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.panel2);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "FontAdmin";
			this.Size = new System.Drawing.Size(376, 583);
			this.Resize += new System.EventHandler(this.fontAdmin_Resize);
			this.panel2.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        private void panel1_Resize(object sender, EventArgs e)
        {
            listFonts.SetSizeToParent();
        }

        private void panel2_Resize(object sender, EventArgs e)
        {
			try
			{
				panel3.Left = panel2.Width - panel3.Width;
			}
			catch { }
        }

        public void RefreshFonts()
        {
            int idx = 0;
            listFonts.Items.Clear();
            foreach (InfoFont fontInfo in m_app.Fonts)
            {
                listFonts.Items.Add(
					idx.ToString()
					+ "-" + Encoding.Default.GetString(m_app.FontImages[idx], 0, fontInfo.NameEnd + 1)
					+ "-" + fontInfo.Width.ToString() + "x" + fontInfo.Height.ToString()
					+ ",(length:" + fontInfo.Length.ToString()
					+ ";size:" + fontInfo.Size.ToString("0.000")
					+ ")");
                idx++;
            }
            lblFontCount.Text = m_app.Fonts.Count.ToString();
        }

        public void SetAppInfo(HmiApplication app)
        {
            m_app = app;
            if (app == null)
            {
                listFonts.Items.Clear();
                btnDelAll.Enabled = false;
                btnPreview.Enabled = false;
                btnDelete.Enabled = false;
                btnAdd.Enabled = false;
            }
            else
            {
                btnDelAll.Enabled = true;
                btnPreview.Enabled = true;
                btnDelete.Enabled = true;
                btnAdd.Enabled = true;
            }
        }

		#region fontAdmin_Resize
		private void fontAdmin_Resize(object sender, EventArgs e)
        {
            int top = 1;
			try
			{
				label4.Top = top;
				label4.Left = 2;
				label4.Width = base.Width - 4;

				lblFontCount.Top = top;
				lblFontCount.Left = base.Width - lblFontCount.Width - 2;

				top += label4.Height;

				panel1.Top = top;
				panel1.Left = 2;
				panel1.Width = base.Width - 4;
				panel1.Height = base.Height - top - panel2.Height - 2;

				panel2.Top = base.Height - panel2.Height - 2;
				panel2.Left = 2;
				panel2.Width = base.Width - 4;
			}
			catch { }
		}
		#endregion
	}
}

