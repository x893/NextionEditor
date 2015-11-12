using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace NextionEditor
{
	public static class GdiFont
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct InfoGdiFont
		{
			public float FontSize;
			public int Xpi;
			public int Ypi;
			public int Width;
			public int Height;
		}

		private static Font m_font;
		private static Graphics m_graphics;

		private static bool findRedHorizontal(int y, Bitmap bm)
		{
			for (int i = 0; i < bm.Width; i++)
				if (bm.GetPixel(i, y).R > 0)
					return true;
			return false;
		}

		private static bool findRedVertical(int x, Bitmap bm)
		{
			for (int i = 0; i < bm.Height; i++)
				if (bm.GetPixel(x, i).R > 0)
					return true;
			return false;
		}

		public static int GetImageHeight(Bitmap image)
		{
			return (image.Height - GetYFree(image) - GetY2Free(image));
		}

		public static int GetImageWidth(Bitmap image)
		{
			return (image.Width - GetXFree(image) - GetX2Free(image));
		}

		public static int GetX2Free(Bitmap image)
		{
			int x = image.Width - 1;
			while (!findRedVertical(x, image))
				x--;
			return ((image.Width - 1) - x);
		}

		public static int GetXFree(Bitmap image)
		{
			int x = 0;
			while (!findRedVertical(x, image))
				x++;
			return x;
		}

		public static int GetY2Free(Bitmap image)
		{
			int y = image.Height - 1;
			while (!findRedHorizontal(y, image))
				y--;
			return ((image.Height - 1) - y);
		}

		public static int GetYFree(Bitmap image)
		{
			int y = 0;
			while (!findRedHorizontal(y, image))
				y++;
			return y;
		}

		public static InfoGdiFont GetFontSize(string sample, string fontName, FontStyle fonStyle, int width, int height, float intsize, bool isAuto, PictureBox picView)
		{
			InfoGdiFont showFont = new InfoGdiFont();
			try
			{
				Bitmap bitmap;
				int bmWidth;
				int bmHeight;
				int delay;

				showFont.FontSize = (intsize * 4f) / 5f;
				if (height > width)
					bitmap = new Bitmap(height * 2, height * 2);
				else
					bitmap = new Bitmap(width * 2, width * 2);

				m_graphics = Graphics.FromImage(bitmap);
				int widthM2 = width - 2;
				int heightM2 = height - 2;
				float sizeDecrement = 0.5f;
				if (height > 56)
					sizeDecrement = 1f;

				m_graphics.Clear(Color.FromArgb(0, 0, 0));
				m_font = new Font(fontName, showFont.FontSize, fonStyle);
				m_graphics.DrawString(sample, m_font, Brushes.Red, (PointF)new Point(1, 1));

				if (picView.Visible)
				{
					picView.Image = bitmap;
					delay = 0;
					while (delay < 100)
					{
						Thread.Sleep(1);
						Application.DoEvents();
						delay++;
					}
				}

				if (isAuto)
				{
					bmWidth = GetImageWidth(bitmap);
					bmHeight = GetImageHeight(bitmap);
					if (bmWidth <= widthM2 && bmHeight <= heightM2)
					{
						while ((bmWidth < widthM2) && (bmHeight < heightM2))
						{
							Application.DoEvents();
							if ((widthM2 - bmWidth) > (heightM2 - bmHeight))
							{
								if ((widthM2 - bmWidth) > 3)
									showFont.FontSize += (widthM2 - bmWidth) / 2;
							}
							else if ((heightM2 - bmHeight) > 3)
								showFont.FontSize += (heightM2 - bmHeight) / 2;

							m_graphics.Clear(Color.FromArgb(0, 0, 0));
							m_font = new Font(fontName, showFont.FontSize, fonStyle);
							m_graphics.DrawString(sample, m_font, Brushes.Red, (PointF)new Point(1, 1));
							bmWidth = GetImageWidth(bitmap);
							bmHeight = GetImageHeight(bitmap);
							if (picView.Visible)
							{
								picView.Image = bitmap;
								for (delay = 0; delay < 100; delay++)
								{
									Thread.Sleep(1);
									Application.DoEvents();
								}
							}
						}

						if ((bmWidth > widthM2) || (bmHeight > heightM2))
						{
							bitmap = new Bitmap((bitmap.Width - GetX2Free(bitmap)) + 2, (bitmap.Height - GetY2Free(bitmap)) + 2);
							m_graphics = Graphics.FromImage(bitmap);
							while ((bmWidth > widthM2) || (bmHeight > heightM2))
							{
								Application.DoEvents();
								if ((bmWidth - widthM2) > (bmHeight - heightM2))
								{
									if ((bmWidth - widthM2) > 3)
										showFont.FontSize -= (bmWidth - widthM2) / 2;
									else
										showFont.FontSize -= sizeDecrement;
								}
								else if ((bmHeight - heightM2) > 3)
									showFont.FontSize -= (bmHeight - heightM2) / 2;
								else
									showFont.FontSize -= sizeDecrement;

								m_graphics.Clear(Color.FromArgb(0, 0, 0));
								m_font = new Font(fontName, showFont.FontSize, fonStyle);
								m_graphics.DrawString(sample, m_font, Brushes.Red, (PointF)new Point(1, 1));
								bmWidth = GetImageWidth(bitmap);
								bmHeight = GetImageHeight(bitmap);
								if (picView.Visible)
								{
									picView.Image = bitmap;
									for (delay = 0; delay < 100; delay++)
									{
										Thread.Sleep(1);
										Application.DoEvents();
									}
								}
							}
						}
					}
					else
					{
						bitmap = new Bitmap((bitmap.Width - GetX2Free(bitmap)) + 2, (bitmap.Height - GetY2Free(bitmap)) + 2);
						m_graphics = Graphics.FromImage(bitmap);
						while ((bmWidth > widthM2) || (bmHeight > heightM2))
						{
							Application.DoEvents();
							if ((bmWidth - widthM2) > (bmHeight - heightM2))
							{
								if ((bmWidth - widthM2) > 3)
									showFont.FontSize -= (bmWidth - widthM2) / 2;
								else
									showFont.FontSize -= sizeDecrement;
							}
							else if ((bmHeight - heightM2) > 3)
								showFont.FontSize -= (bmHeight - heightM2) / 2;
							else
								showFont.FontSize -= sizeDecrement;

							m_graphics.Clear(Color.FromArgb(0, 0, 0));
							m_font = new Font(fontName, showFont.FontSize, fonStyle);
							m_graphics.DrawString(sample, m_font, Brushes.Red, (PointF)new Point(1, 1));
							bmWidth = GetImageWidth(bitmap);
							bmHeight = GetImageHeight(bitmap);

							if (picView.Visible)
							{
								picView.Image = bitmap;
								for (delay = 0; delay < 100; delay++)
								{
									Thread.Sleep(1);
									Application.DoEvents();
								}
							}
						}
					}
				}

				Application.DoEvents();
				showFont.Width = GetImageWidth(bitmap);
				showFont.Height = GetImageHeight(bitmap);
				showFont.Xpi = GetXFree(bitmap) - 1;
				showFont.Ypi = GetYFree(bitmap) - 1;
			}
			catch
			{
				showFont.FontSize = 0f;
			}
			return showFont;
		}
	}
}
