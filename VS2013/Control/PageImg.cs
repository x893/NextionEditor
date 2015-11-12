using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace NextionEditor
{
	public class PageImg : UserControl
	{
		public event EventHandler PageUpdate;

		public HmiApplication App;
		public HmiPage Page;
		public int No = 0;

		private Label lblName = new Label();
		private TextBox tbName = null;

		private bool m_isDraw = false;
		private Pen m_pen = new Pen(Color.Red);
		private Bitmap m_bmp;
		private Color m_bcolor;
		private Color m_pcolor;
		private Graphics m_gc;

		public PageImg()
		{
			InitializeComponent();
		}

		#region Draw(bool show)
		public void Draw(bool show)
		{
			m_pen.Color = m_bcolor;

			if (show)
				m_pen.Color = Color.Blue;

			Point[] points = new Point[5];
			points[0].X = 0;
			points[0].Y = 0;
			points[1].X = base.Width - 1;
			points[1].Y = 0;
			points[2].X = base.Width - 1;
			points[2].Y = base.Height - 1;
			points[3].X = 0;
			points[3].Y = base.Height - 1;
			points[4].X = 0;
			points[4].Y = 0;
			m_gc = base.CreateGraphics();
			m_gc.DrawLines(m_pen, points);
		}
		#endregion

		#region InitializeComponent()
		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// PageImg
			// 
			this.Name = "PageImg";
			this.DoubleClick += new System.EventHandler(this.PageImg_DoubleClick);
			this.MouseLeave += new System.EventHandler(this.PageImg_MouseLeave);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PageImg_MouseMove);
			this.ResumeLayout(false);

		}
		#endregion

		#region PageImg_DoubleClick
		private void PageImg_DoubleClick(object sender, EventArgs e)
		{
			tbName = new TextBox();
			tbName.Text = Page.Name;
			tbName.Location = new Point(0, 0);
			tbName.Width = base.Width;
			tbName.Height = base.Height;
			base.Controls.Add(tbName);
			tbName.Visible = true;
			tbName.KeyPress += new KeyPressEventHandler(tbName_KeyPress);
			tbName.Focus();
		}
		#endregion

		#region PageImg_MouseLeave
		private void PageImg_MouseLeave(object sender, EventArgs e)
		{
			if (m_isDraw)
			{
				Draw(false);
				m_isDraw = false;
			}
		}
		#endregion

		#region PageImg_MouseMove
		private void PageImg_MouseMove(object sender, EventArgs e)
		{
			if (!m_isDraw)
			{
				Draw(true);
				m_isDraw = true;
			}
		}
		#endregion

		#region RefreshPageImg
		public void RefreshPageImg(bool selected)
		{
			if (base.Width == 0 || base.Height == 0)
				return;

			if (selected)
			{
				m_bcolor = Color.Blue;
				m_pcolor = Color.White;
			}
			else
			{
				m_bcolor = Color.White;
				m_pcolor = Color.Black;
			}

			m_bmp = new Bitmap(base.Width, base.Height);
			m_gc = Graphics.FromImage(m_bmp);
			m_gc.Clear(m_bcolor);

			Font font = new Font(Encoding.Default.EncodingName, 9f);
			SolidBrush pen = new SolidBrush(m_pcolor);
			m_gc.DrawString(Page.Name,
				font,
				pen,
				(PointF)new Point(0, 0)
				);
			m_gc.DrawString(
				No.ToString(),
				font,
				pen,
				(PointF)new Point(base.Width - 15, 0)
				);
			BackgroundImage = m_bmp;

			if (tbName != null && !selected)
			{
				tbName.Dispose();
				tbName = null;
				base.Controls.Remove(tbName);
				PageUpdate(null, null);
			}
		}
		#endregion

		#region tbName_KeyPress
		private void tbName_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r')
			{
				string name = Page.Name;
				string imgName = tbName.Text.Trim();
				int length = imgName.ToBytes().Length;

				if (length == 0 || length > 16)
				{
					MessageBox.Show("Name Length 1 bytes minimum, 16 bytes maximum".Translate());
				}
				else if (!Utility.IsNameValid(tbName.Text.Trim()))
				{
					tbName.Text = name;
				}
				else if (App.FindPageByName(imgName))
				{
					tbName.Text = name;
					MessageBox.Show("Duplicate Name!".Translate());
				}
				else
				{
					tbName.Dispose();
					tbName = null;
					base.Controls.Remove(tbName);

					if (name != imgName)
					{
						if (Page.HmiObjects.Count > 0
						 && Page.HmiObjects[0].Attributes[0].InfoAttribute.AttrType == HmiAttributeType.State
						 && Page.HmiObjects[0].Attributes[0].Data[0] == HmiObjType.PAGE
							)
						{
							Page.HmiObjects[0].ObjName = imgName;
						}
						Page.Name = imgName;
						RefreshPageImg(true);
						PageUpdate(null, null);
					}
				}
			}
		}
		#endregion
	}
}
