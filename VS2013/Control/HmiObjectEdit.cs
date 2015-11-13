using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace NextionEditor
{
	public class HmiObjectEdit : UserControl
	{
		public event EventHandler ObjChange;
		public event EventHandler ObjMouseUp;

		public HmiObject HmiObject;
		public HmiRunScreen HmiRunScreen;
		public bool IsMove = false;
		public bool IsShowName = true;
		public bool IsSelected = false;

		private IContainer components = null;
		private Graphics m_gc;
		private Label label1;
		private Label label2;

		private HmiApplication m_app;
		private bool m_isChange = false;
		private Bitmap m_page_bg;
		private Point m_mousePoint;
		private MouseState m_mouseState = MouseState.Defaut;
		private int m_mouseChoose = 3;
		private Point m_objPoint;
		private Range m_objXY = new Range();
		private Pen m_pen = new Pen(Color.Red, 1f);

		public HmiObjectEdit()
		{
			InitializeComponent();
			base.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			m_gc = base.CreateGraphics();
		}

		public void SetApp(HmiApplication app)
		{
			m_app = app;
		}

		#region MakeBackground
		public unsafe void MakeBackground()
		{
			List<byte[]> listCmd = new List<byte[]>();
			byte[] buffer = new byte[1];
			Range range = new Range();
			try
			{
				Graphics.FromImage(HmiRunScreen.ThisBmp[1]).Clear(Color.FromArgb(0, 0, 0, 0));
				HmiRunScreen.IsTransparent = false;

				if (HmiObject.Attributes[0].Data[0] != HmiObjType.TIMER
				 && HmiObject.Attributes[0].Data[0] != HmiObjType.VAR
					)
				{
					if (HmiObject.ObjInfo.Panel.X < 0
					 || HmiObject.ObjInfo.Panel.EndX >= m_app.LcdWidth
					 || HmiObject.ObjInfo.Panel.Y < 0
					 || HmiObject.ObjInfo.Panel.EndY >= m_app.LcdHeight
						)
						MessageBox.Show(HmiObject.ObjName + "Drawing off screen. Cancelled.".Translate());
					else
					{
						if (HmiObject.Attributes[0].Data[0] < HmiObjType.OBJECT_TYPE_END)
						{
							HmiRunScreen.ThisBmpIndex = 1;
							buffer = new byte[0];
							HmiObject.GetObjRamBytes(ref buffer, 0);
							Utility.CopyTo(buffer, 0, ref HmiRunScreen.GuiApp.CustomData);
							buffer = new byte[1];
							HmiRunScreen.GuiObjControl[HmiObject.Attributes[0].Data[0]].OnInit(ref HmiObject.ObjInfo, (byte)HmiObject.ObjId);
							m_page_bg = new Bitmap(Width, Height);
							Graphics.FromImage(m_page_bg).DrawImage(
									HmiRunScreen.ThisBmp[1],
									new Rectangle(0, 0, Width, Height),
									new Rectangle(Left, Top, Width, Height),
									GraphicsUnit.Pixel
								);
						}
						else
						{
							HmiObject.GetCompileRefCodes(listCmd);
							if (listCmd.Count > 0)
							{
								range.Begin = 0;
								foreach (byte[] cmd in listCmd)
								{
									range.End = cmd.Length - 1;
									HmiRunScreen.CodeExecute(cmd, range, 1);
								}
								m_page_bg = new Bitmap(Width, Height);
								Graphics.FromImage(m_page_bg).DrawImage(
									HmiRunScreen.ThisBmp[1],
									new Rectangle(0, 0, Width, Height),
									new Rectangle(Left, Top, Width, Height),
									GraphicsUnit.Pixel
								);
							}
						}

						if (base.Parent != null && HmiObject.Attributes[0].Data[0] == HmiObjType.PAGE)
							base.Parent.BackgroundImage = m_page_bg;
					}
				}

				if (HmiRunScreen.IsTransparent)
					BackColor = Color.FromArgb(0, 0, 0, 0);
				Refresh();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error occurred during redrawig components ".Translate() + ex.Message);
			}
		}
		#endregion

		#region SetWidthXY
		public void SetWidthXY()
		{
			base.Location = new Point(HmiObject.ObjInfo.Panel.X, HmiObject.ObjInfo.Panel.Y);
			base.Width = (HmiObject.ObjInfo.Panel.EndX - HmiObject.ObjInfo.Panel.X) + 1;
			base.Height = (HmiObject.ObjInfo.Panel.EndY - HmiObject.ObjInfo.Panel.Y) + 1;
			HmiObject.ChangeXY();
			MakeBackground();
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
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(43, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "label1";
			this.label1.Visible = false;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 29);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(43, 17);
			this.label2.TabIndex = 1;
			this.label2.Text = "label2";
			this.label2.Visible = false;
			// 
			// HmiObjectEdit
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(149)))), ((int)(((byte)(253)))));
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Name = "HmiObjectEdit";
			this.Size = new System.Drawing.Size(150, 163);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.objedit_Paint);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.objedit_MouseDown);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.objedit_MouseMove);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.objedit_MouseUp);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		#region objedit_MouseDown
		private void objedit_MouseDown(object sender, MouseEventArgs e)
		{
			if (IsMove
			 && e.Button == MouseButtons.Left
			 && HmiObject.Attributes[0].Data[0] != HmiObjType.PAGE
				)
			{
				switch (m_mouseState)
				{
					case MouseState.Defaut:
						m_mousePoint = Control.MousePosition;
						m_objPoint = base.Location;
						m_objXY.Begin = (ushort)base.Width;
						m_objXY.End = (ushort)base.Height;
						if (e.X <= (base.Width - m_mouseChoose))
						{
							if (e.Y > (base.Height - m_mouseChoose))
							{
								Cursor = Cursors.SizeNS;
								m_mouseState = MouseState.Yadd;
							}
							else
							{
								Cursor = Cursors.Hand;
								m_mouseState = MouseState.Move;
							}
							break;
						}
						Cursor = Cursors.SizeWE;
						m_mouseState = MouseState.Xadd;
						break;
				}
			}
		}
		#endregion

		#region objedit_MouseMove
		private void objedit_MouseMove(object sender, MouseEventArgs e)
		{
			if (IsMove)
			{
				int dX = 0;
				int dY = 0;
				if (HmiObject.Attributes[0].Data[0] != HmiObjType.PAGE)
				{
					switch (m_mouseState)
					{
						case MouseState.Defaut:
							if (e.X <= (base.Width - m_mouseChoose))
							{
								if (e.Y > (base.Height - m_mouseChoose))
									Cursor = Cursors.SizeNS;
								else
									Cursor = Cursors.Hand;
							}
							else
								Cursor = Cursors.SizeWE;
							break;

						case MouseState.Move:
							dX = Control.MousePosition.X - m_mousePoint.X;
							dY = Control.MousePosition.Y - m_mousePoint.Y;
							if ((m_objPoint.X + dX + base.Width) >= m_app.LcdWidth)
								dX = m_app.LcdWidth - m_objPoint.X - base.Width;
							if ((m_objPoint.Y + dY + base.Height) >= m_app.LcdHeight)
								dY = m_app.LcdHeight - m_objPoint.Y - base.Height;
							if ((m_objPoint.X + dX) < 0)
								dX = -m_objPoint.X;
							if ((m_objPoint.Y + dY) < 0)
								dY = -m_objPoint.Y;

							base.Location = new Point(m_objPoint.X + dX, m_objPoint.Y + dY);
							m_isChange = true;
							break;

						case MouseState.Xadd:
							dX = (m_objXY.Begin + Control.MousePosition.X) - m_mousePoint.X;
							if (base.Left + dX > m_app.LcdWidth)
								dX = m_app.LcdWidth - base.Left;
							if (dX < 5)
								dX = 5;

							base.Width = dX;
							if (HmiObject.IsBinding == 2)
								base.Height = dX;

							m_isChange = true;
							Refresh();
							break;

						case MouseState.Yadd:
							dY = m_objXY.End + Control.MousePosition.Y - m_mousePoint.Y;
							if (base.Top + dY > m_app.LcdHeight)
								dY = m_app.LcdHeight - base.Top;
							if (dY < 5)
								dY = 5;

							base.Height = dY;
							if (HmiObject.IsBinding == 2)
								base.Width = dY;

							m_isChange = true;
							Refresh();
							break;
					}
				}
			}
		}
		#endregion

		#region objedit_MouseUp
		private void objedit_MouseUp(object sender, MouseEventArgs e)
		{
			m_mouseState = MouseState.Defaut;
			if (IsMove && m_isChange)
			{
				if (base.Top < 0) base.Top = 0;
				if (base.Left < 0) base.Left = 0;

				if (HmiObject.IsBinding == 1)
				{
					base.Width = HmiObject.ObjInfo.Panel.EndX - HmiObject.ObjInfo.Panel.X + 1;
					base.Height = HmiObject.ObjInfo.Panel.EndY - HmiObject.ObjInfo.Panel.Y + 1;
					HmiObject.ObjInfo.Panel.X = (ushort)base.Location.X;
					HmiObject.ObjInfo.Panel.Y = (ushort)base.Location.Y;
					HmiObject.ObjInfo.Panel.EndX = (ushort)(base.Left + base.Width - 1);
					HmiObject.ObjInfo.Panel.EndY = (ushort)(base.Top + base.Height - 1);
				}
				else
				{
					HmiObject.ObjInfo.Panel.X = (ushort)base.Location.X;
					HmiObject.ObjInfo.Panel.Y = (ushort)base.Location.Y;
					HmiObject.ObjInfo.Panel.EndX = (ushort)(base.Left + base.Width - 1);
					HmiObject.ObjInfo.Panel.EndY = (ushort)(base.Top + base.Height - 1);
				}

				HmiObject.ChangeXY();
				ObjChange(this, null);
				m_isChange = false;
			}
			SetSelected(true);
			ObjMouseUp(this, null);
		}
		#endregion

		#region objedit_Paint
		private void objedit_Paint(object sender, PaintEventArgs e)
		{
			m_gc = base.CreateGraphics();
			if (m_mouseState == MouseState.Defaut
			 && m_page_bg != null
			 && HmiObject.Attributes[0].Data[0] != HmiObjType.PAGE
				)
				m_gc.DrawImage(m_page_bg, 0, 0, base.Width, base.Height);

			Color black = Color.Black;
			if (IsSelected)
				m_pen.Color = Color.Red;
			else
				m_pen.Color = Color.White;

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
			// Draw border (red or black)
			if (IsSelected || (HmiObject.Attributes[0].Data[0] != HmiObjType.PAGE))
				m_gc.DrawLines(m_pen, points);

			if (IsShowName && HmiObject.Attributes[0].Data[0] != HmiObjType.PAGE)
			{
				string name = HmiObject.ObjName;
				m_gc.FillRectangle(
					new SolidBrush(Color.Yellow),
					1, 1,
					8 * name.Length, 14
					);
				m_gc.DrawString(
					name,
					new Font(Encoding.Default.EncodingName, 10f),
					new SolidBrush(black),
					(PointF)new Point(0, 0)
					);
			}
		}
		#endregion

		#region SetSelected
		public void SetSelected(bool isSelected)
		{
			IsSelected = isSelected;
			MakeBackground();
		}
		#endregion
	}
}