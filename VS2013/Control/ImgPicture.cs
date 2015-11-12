using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NextionEditor
{
	public class ImgPicture : PictureBox
	{
		public HmiApplication App;
		private IContainer components = null;
		public int No;

		public ImgPicture()
		{
			InitializeComponent();
		}

		#region InitializeComponent()
		protected override void Dispose(bool disposing)
		{
			if (disposing && (this.components != null))
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			base.SuspendLayout();
			base.ResumeLayout(false);
		}
		#endregion

		public void ViewPic(Color bcolor, Brush fontbrush)
		{
			InfoPicture infoPic = App.Pictures[No];
			if (base.Width >= 1)
			{
				try
				{
					int w;
					if (infoPic.W <= base.Width)
						w = infoPic.W;
					else
						w = base.Width;
					
					int height = (infoPic.H * w) / infoPic.W;
					base.Height = height + 40;
					Bitmap bg = new Bitmap(base.Width, base.Height);
					Graphics graphics = Graphics.FromImage(bg);
					graphics.Clear(bcolor);

					Bitmap img = Utility.GetBitmap(App.PictureImages[No], App.Pictures[No], false);
					graphics.DrawImage(
						img,
						new Rectangle(0, 20, w, height),
						new Rectangle(0, 0, img.Width, img.Height),
						GraphicsUnit.Pixel
						);
					graphics.DrawString(
						No.ToString() + "--SIZE:" + infoPic.W.ToString() + "x" + infoPic.H.ToString(),
						new Font(SystemFonts.DefaultFont.Name, 12f),
						fontbrush,
						(PointF)new Point(10, base.Height - 20)
						);
					base.Image = bg;
					img.Dispose();
				}
				catch (Exception exception)
				{
					MessageBox.Show(exception.Message);
				}
			}
		}
	}
}