using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace NextionEditor
{
    public class FontView : Form
    {
        private Bitmap bm;
        private Button button2;
        private IContainer components = null;
        private int dpage = 0;
        private FormParams m_params = new FormParams();
        private int h;
        private int hangjuh;
        private int hangjuw;
        private Label label1;
        private LinkLabel linkLabel1;
        private LinkLabel linkLabel2;
        private HmiApplication m_app;
        private int pageqyt = 0;
        private PictureBox pictureBox1;
        private TextBox textBox1;
        private Timer timer1;
        private int w;
        private int m_fontIndex;
        private int zpages = 0;

        public FontView(HmiApplication app, int fontIndex, FormParams formParams)
        {
			m_app = app;
			m_fontIndex = fontIndex;
			m_params = formParams;
            InitializeComponent();
			Utility.Translate(this);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = "";
            m_params.Strings[0] = textBox1.Text;
            m_params.Strings[1] = h.ToString();
            m_params.Strings[2] = Encoding.Default.GetString(m_app.FontImages[m_fontIndex], 0, m_app.Fonts[m_fontIndex].NameEnd + 1);
            Form form = new FontCreator(m_params);
            form.ShowDialog();
            if (form.DialogResult == DialogResult.OK)
            {
				if (MessageBox.Show("Replace the generated font?".Translate(), "Tips".Translate(), MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    path = m_params.Strings[0];
                    int count = Marshal.SizeOf(typeof(InfoFont));
                    StreamReader reader = new StreamReader(path);

                    byte[] buffer = new byte[count];
                    reader.BaseStream.Read(buffer, 0, count);
					InfoFont infoFont = Utility.ToStruct<InfoFont>(buffer);
                    m_app.Fonts[m_fontIndex] = infoFont;

                    buffer = new byte[infoFont.Size];
                    reader.BaseStream.Read(buffer, 0, buffer.Length);
                    m_app.FontImages[m_fontIndex] = buffer;
                    reader.Close();
                    reader.Dispose();
                    showzi();
                    m_params.Strings[1] = "edit";
                    RefreshMe();
                }
            }
            else
            {
                m_params.Strings[1] = "";
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
			this.components = new System.ComponentModel.Container();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.linkLabel2 = new System.Windows.Forms.LinkLabel();
			this.label1 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.button2 = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBox1.BackColor = System.Drawing.Color.White;
			this.pictureBox1.Location = new System.Drawing.Point(0, 3);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(644, 581);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// linkLabel1
			// 
			this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.Location = new System.Drawing.Point(369, 628);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(58, 17);
			this.linkLabel1.TabIndex = 2;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "Page Up";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			// 
			// linkLabel2
			// 
			this.linkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.linkLabel2.AutoSize = true;
			this.linkLabel2.Location = new System.Drawing.Point(433, 628);
			this.linkLabel2.Name = "linkLabel2";
			this.linkLabel2.Size = new System.Drawing.Size(74, 17);
			this.linkLabel2.TabIndex = 3;
			this.linkLabel2.TabStop = true;
			this.linkLabel2.Text = "Page Down";
			this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(514, 628);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(43, 17);
			this.label1.TabIndex = 4;
			this.label1.Text = "label1";
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox1.BackColor = System.Drawing.Color.White;
			this.textBox1.Location = new System.Drawing.Point(14, 592);
			this.textBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(493, 25);
			this.textBox1.TabIndex = 5;
			// 
			// button2
			// 
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button2.Location = new System.Drawing.Point(514, 590);
			this.button2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(129, 37);
			this.button2.TabIndex = 6;
			this.button2.Text = "Change Font";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Visible = false;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// FontView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(645, 653);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.linkLabel2);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.pictureBox1);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "FontView";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Font Preview";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.Load += new System.EventHandler(this.zikuview_Load);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (dpage > 0)
            {
                dpage--;
            }
            RefreshMe();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (dpage < (zpages - 1))
            {
                dpage++;
            }
            RefreshMe();
        }

        private void RefreshMe()
        {
            w = m_app.Fonts[m_fontIndex].Width;
            h = m_app.Fonts[m_fontIndex].Height;
            hangjuw = 0;
            hangjuh = 0;
            pageqyt = (pictureBox1.Width / (w + hangjuw)) * (pictureBox1.Height / (w + hangjuh));
            zpages = ((int) (((long) m_app.Fonts[m_fontIndex].Length) / ((long) pageqyt))) + 1;
            int index = dpage * pageqyt;
            int dxianshix = 0;
            int dxianshiy = 0;
            bm = new Bitmap(pictureBox1.Width + 1, pictureBox1.Height + 1);
            Graphics.FromImage(bm).Clear(Color.White);
            while (dxianshiy < (pictureBox1.Height - h))
            {
                while (dxianshix < (pictureBox1.Width - w))
                {
                    Xianshi(index, dxianshix, dxianshiy);
                    index++;
                    dxianshix += w + hangjuw;
                }
                dxianshix = 0;
                dxianshiy += h + hangjuh;
            }
            pictureBox1.Image = bm;
            label1.Text = "Page:" + ((dpage + 1)).ToString() + "/" + zpages.ToString();
        }

        private void showzi()
        {
            w = m_app.Fonts[m_fontIndex].Width;
            h = m_app.Fonts[m_fontIndex].Height;
            hangjuw = 0;
            hangjuh = 0;
            if ((m_app.Fonts[m_fontIndex].State == 2) && (HmiOptions.Language == 0))
            {
                textBox1.Visible = true;
                button2.Visible = true;
            }
            else
            {
                textBox1.Visible = false;
                button2.Visible = false;
                return;
            }
            byte num = "?".ToBytes()[0];
            byte[] bytes = new byte[m_app.Fonts[m_fontIndex].Length * 2];
            byte[] buffer2 = m_app.FontImages[m_fontIndex];
            int num2 = (w * h) / 8;
            num2 += 2;
            for (int i = 0; i < m_app.Fonts[m_fontIndex].Length; i++)
            {
                bytes[i * 2] = buffer2[((i * num2) + m_app.Fonts[m_fontIndex].NameEnd) + 1];
                bytes[(i * 2) + 1] = buffer2[(((i * num2) + 1) + m_app.Fonts[m_fontIndex].NameEnd) + 1];
                if (bytes[i * 2] == 0)
                {
                    bytes[i * 2] = num;
                }
            }
            textBox1.Text = Encoding.Default.GetString(bytes);
            textBox1.Text = textBox1.Text.Replace("?", "");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            showzi();
            RefreshMe();
        }

        private void Xianshi(int index, int dxianshix, int dxianshiy)
        {
            int num = (w * h) / 8;
            int num2 = 0;
            int num3 = 0;
            byte num4 = 0;
            int num5 = 0;
            num5 = (((index * (num + ((m_app.Fonts[m_fontIndex].State == 2) ? 2 : 0))) + ((m_app.Fonts[m_fontIndex].State == 2) ? 2 : 0)) + m_app.Fonts[m_fontIndex].NameEnd) + 1;
            if (num5 < m_app.Fonts[m_fontIndex].Size)
            {
                while (num2 < w)
                {
                    while (num3 < h)
                    {
                        num4 = m_app.FontImages[m_fontIndex][num5];
                        num5++;
                        for (int i = 0; i < 8; i++)
                        {
                            if ((num4 & (((int) 1) << (7 - i))) > 0)
                            {
                                bm.SetPixel(dxianshix + num2, dxianshiy + num3, Color.Black);
                            }
                            num3++;
                        }
                    }
                    num3 = 0;
                    num2++;
                }
            }
        }

        private void zikuview_Load(object sender, EventArgs e)
        {
        }
    }
}

