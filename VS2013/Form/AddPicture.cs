using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NextionEditor
{
    public class AddPicture : Form
    {
        private IContainer components = null;
        private Label label1;
        private HmiApplication m_app;
        private Timer m_timer;
		private int m_addIndex = 0;

        public AddPicture(HmiApplication app, int index)
        {
            m_app = app;
            m_addIndex = index;
            InitializeComponent();

			Utility.Translate(this);
        }

		private void timer1_Tick(object sender, EventArgs e)
		{
			base.Visible = false;
			m_timer.Enabled = false;

			if (addPicture(m_addIndex))
				base.DialogResult = DialogResult.OK;
			else
				base.DialogResult = DialogResult.Cancel;
		}

		private bool openPicture(ref Bitmap bm, string file)
		{
			try
			{
				using (PictureBox box = new PictureBox())
				{
					box.SizeMode = PictureBoxSizeMode.AutoSize;
					box.Load(file);
					bm = (Bitmap)box.Image;
				}
				return true;
			}
			catch { }
			return false;
		}

		private bool addPicture(int index)
        {
            Bitmap bm = new Bitmap(1, 1);
            int errors = 0;
            int loaded = 0;
            OpenFileDialog op = new OpenFileDialog {
                Multiselect = (index == 0xffff) ? true : false,
                Filter = "All files|*.*".Translate()
            };
            Utility.SetInitialPath(op, "pic");
            if (op.ShowDialog() != DialogResult.OK)
                return false;

			Utility.SavePath(op, "pic");

            InfoPicture infoPicture = new InfoPicture();
            base.Visible = true;
			int numPicture = 1;
			foreach (string filename in op.FileNames)
            {
				label1.Text = "Converting Pictures".Translate() + numPicture.ToString();
                numPicture++;
                Application.DoEvents();

                bm = new Bitmap(1, 1);
                if (openPicture(ref bm, filename))
                {
                    ++loaded;
                    byte[] buffer = new byte[bm.Width * bm.Height * 2];
                    int idx = 0;
                    for (int iY = 0; iY < bm.Height; iY++)
                    {
                        ushort num;
                        int iX;
						infoPicture.IsPotrait = (byte)(m_app.IsPotrait ? 1 : 0);
                        if (m_app.IsPotrait)
							for (iX = bm.Width - 1; iX >= 0; --iX)
							{
								if (bm.GetPixel(iX, iY).A == 0)
								{
									buffer[idx] = (byte)(HmiOptions.ColorTransparent % 0x100);
									buffer[idx + 1] = 0;
								}
								else
								{
									num = Utility.Get16Color(bm.GetPixel(iX, iY));
									buffer[idx] = (byte)(num % 0x100);
									buffer[idx + 1] = (byte)(num / 0x100);
									if (buffer[idx] == HmiOptions.ColorTransparent && buffer[idx + 1] == 0)
										buffer[idx] = (byte)HmiOptions.ColorTransparentReplace;
								}
								idx += 2;
							}
                        else
                            for (iX = 0; iX < bm.Width; iX++)
                            {
                                if (bm.GetPixel(iX, iY).A == 0)
                                {
                                    buffer[idx] = (byte) (HmiOptions.ColorTransparent % 0x100);
                                    buffer[idx + 1] = 0;
                                }
                                else
                                {
									num = Utility.Get16Color(bm.GetPixel(iX, iY));
                                    buffer[idx] = (byte) (num % 0x100);
                                    buffer[idx + 1] = (byte) (num / 0x100);
                                    if (buffer[idx] == HmiOptions.ColorTransparent && buffer[idx + 1] == 0)
                                        buffer[idx] = (byte) HmiOptions.ColorTransparentReplace;
                                }
                                idx += 2;
                            }
                    }
                    infoPicture.H = (ushort) bm.Height;
                    infoPicture.Size = (uint) buffer.Length;
                    infoPicture.W = (ushort) bm.Width;
                    infoPicture.IsOne = 1;
                    infoPicture.ShowTime = 0;
                    infoPicture.SizeZi = infoPicture.Size;
                    if (index == 0xffff)
                    {
                        m_app.Pictures.Add(infoPicture);
                        m_app.PictureImages.Add(buffer);
                    }
                    else
                    {
                        if (index >= m_app.Pictures.Count)
                            return false;

                        m_app.Pictures.Insert(index, infoPicture);
                        m_app.PictureImages.Insert(index, buffer);
                    }
                }
                else
                    errors++;

                if (bm != null)
                    bm.Dispose();
            }
            if (errors == 0)
				MessageBox.Show(string.Concat("Import successfully ".Translate(), loaded, " pieces".Translate()));
            else
				MessageBox.Show(string.Concat("Import successfully ".Translate(), loaded, " pieces, ERROR ".Translate(), errors));

            return true;
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
			this.label1 = new System.Windows.Forms.Label();
			this.m_timer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.ForeColor = System.Drawing.Color.Blue;
			this.label1.Location = new System.Drawing.Point(33, 50);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(140, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "Converting the first pic";
			// 
			// m_timer
			// 
			this.m_timer.Enabled = true;
			this.m_timer.Interval = 1;
			this.m_timer.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// AddPicture
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(455, 135);
			this.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.Name = "AddPicture";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Add Picture";
			this.ResumeLayout(false);
			this.PerformLayout();
        }
		#endregion
    }
}

