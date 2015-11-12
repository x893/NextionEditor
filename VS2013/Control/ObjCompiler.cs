using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NextionEditor
{
	public class ObjCompiler : UserControl
	{
		public event EventHandler ChangeAttribute;

		private HmiApplication m_AppInf;
		private int m_strStart = 0;
		private int m_codeIndex = 0xffff;
		private HmiObject m_Object = null;
		private HmiPage m_Page = null;

		private CheckBox cbTPPressCompId;
		private CheckBox cbTPReleaseCompId;
		private IContainer components = null;
		private Label label1;
		private RadioButton rbAutoLoad;
		private RadioButton rbLoadCommands;
		private TabControl tabControl;
		private TabPage pageInitialization;
		private TabPage pageTouchPressEvent;
		private TabPage pageTouchReleaseEvent;
		private TabPage pageTouchMove;
		private RichTextBox tbUserCode;

		public ObjCompiler()
		{
			InitializeComponent();
		}

		private void attrLoad(int index)
		{
			if (m_AppInf != null)
			{
				List<byte[]> list = new List<byte[]>();
				tbUserCode.Text = Utility.ToStrings(m_Object.Codes[index]);
				m_codeIndex = index;
				Utility.SetLineSelect(tbUserCode);
			}
		}

		private void cbTPPressCompId_Click(object sender, EventArgs e)
		{
			if (cbTPPressCompId.Checked)
				m_Object.ObjInfo.Panel.SendKey = (byte)(m_Object.ObjInfo.Panel.SendKey | 2);
			else
				m_Object.ObjInfo.Panel.SendKey = (byte)(m_Object.ObjInfo.Panel.SendKey & 1);
			
			ChangeAttribute(this, null);
			RefreshHead();
		}

		private void cbTPReleaseCompId_Click(object sender, EventArgs e)
		{
			if (cbTPReleaseCompId.Checked)
				m_Object.ObjInfo.Panel.SendKey = (byte)(m_Object.ObjInfo.Panel.SendKey | 1);
			else
				m_Object.ObjInfo.Panel.SendKey = (byte)(m_Object.ObjInfo.Panel.SendKey & 2);
			
			ChangeAttribute(this, null);
			RefreshHead();
		}

		private void Clear()
		{
			try
			{
				SaveCodes();
				tbUserCode.Text = "";
				tbUserCode.Enabled = false;
				tabControl.Enabled = false;
				rbAutoLoad.Enabled = false;
				rbLoadCommands.Enabled = false;
				cbTPPressCompId.Enabled = false;
				cbTPReleaseCompId.Enabled = false;
				m_codeIndex = 0xffff;
				if (tabControl.TabPages.Contains(pageTouchMove))
				{
					tabControl.TabPages.Remove(pageTouchMove);
					tabControl.SelectedIndex = 0;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Crear Attribute Compiler Error:\n".Translate() + ex.Message);
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
			this.tabControl = new System.Windows.Forms.TabControl();
			this.pageInitialization = new System.Windows.Forms.TabPage();
			this.tbUserCode = new System.Windows.Forms.RichTextBox();
			this.rbLoadCommands = new System.Windows.Forms.RadioButton();
			this.rbAutoLoad = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.pageTouchPressEvent = new System.Windows.Forms.TabPage();
			this.cbTPPressCompId = new System.Windows.Forms.CheckBox();
			this.pageTouchReleaseEvent = new System.Windows.Forms.TabPage();
			this.cbTPReleaseCompId = new System.Windows.Forms.CheckBox();
			this.pageTouchMove = new System.Windows.Forms.TabPage();
			this.tabControl.SuspendLayout();
			this.pageInitialization.SuspendLayout();
			this.pageTouchPressEvent.SuspendLayout();
			this.pageTouchReleaseEvent.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.pageInitialization);
			this.tabControl.Controls.Add(this.pageTouchPressEvent);
			this.tabControl.Controls.Add(this.pageTouchReleaseEvent);
			this.tabControl.Controls.Add(this.pageTouchMove);
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Location = new System.Drawing.Point(0, 0);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(440, 358);
			this.tabControl.TabIndex = 1;
			this.tabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.cbTPPressCompId_Selected);
			// 
			// tbUserCode
			// 
			this.tbUserCode.AcceptsTab = true;
			this.tbUserCode.DetectUrls = false;
			//!!! font this.tbUserCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.tbUserCode.Location = new System.Drawing.Point(3, 42);
			this.tbUserCode.Name = "tbUserCode";
			this.tbUserCode.Size = new System.Drawing.Size(129, 90);
			this.tbUserCode.TabIndex = 5;
			this.tbUserCode.Text = "";
			this.tbUserCode.WordWrap = false;
			this.tbUserCode.TextChanged += new System.EventHandler(this.tbUserCode_TextChanged);
			this.tbUserCode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbUserCode_KeyDown);
			this.tbUserCode.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbUserCode_KeyUp);
			// 
			// rbLoadCommands
			// 
			this.rbLoadCommands.AutoSize = true;
			this.rbLoadCommands.Location = new System.Drawing.Point(93, 6);
			this.rbLoadCommands.Name = "rbLoadCommands";
			this.rbLoadCommands.Size = new System.Drawing.Size(104, 17);
			this.rbLoadCommands.TabIndex = 4;
			this.rbLoadCommands.TabStop = true;
			this.rbLoadCommands.Text = "Load Commands";
			this.rbLoadCommands.UseVisualStyleBackColor = true;
			this.rbLoadCommands.Click += new System.EventHandler(this.rbLoadCommands_Click);
			// 
			// rbAutoLoad
			// 
			this.rbAutoLoad.AutoSize = true;
			this.rbAutoLoad.Location = new System.Drawing.Point(6, 6);
			this.rbAutoLoad.Name = "rbAutoLoad";
			this.rbAutoLoad.Size = new System.Drawing.Size(81, 17);
			this.rbAutoLoad.TabIndex = 3;
			this.rbAutoLoad.TabStop = true;
			this.rbAutoLoad.Text = "Autoloading";
			this.rbAutoLoad.UseVisualStyleBackColor = true;
			this.rbAutoLoad.Click += new System.EventHandler(this.rbAutoLoad_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 26);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "User Code";
			// 
			// cbTPPressCompId
			// 
			this.cbTPPressCompId.AutoSize = true;
			this.cbTPPressCompId.Location = new System.Drawing.Point(6, 6);
			this.cbTPPressCompId.Name = "cbTPPressCompId";
			this.cbTPPressCompId.Size = new System.Drawing.Size(122, 17);
			this.cbTPPressCompId.TabIndex = 0;
			this.cbTPPressCompId.Text = "Send Component ID";
			this.cbTPPressCompId.UseVisualStyleBackColor = true;
			this.cbTPPressCompId.Click += new System.EventHandler(this.cbTPPressCompId_Click);
			// 
			// cbTPReleaseCompId
			// 
			this.cbTPReleaseCompId.AutoSize = true;
			this.cbTPReleaseCompId.Location = new System.Drawing.Point(6, 6);
			this.cbTPReleaseCompId.Name = "cbTPReleaseCompId";
			this.cbTPReleaseCompId.Size = new System.Drawing.Size(122, 17);
			this.cbTPReleaseCompId.TabIndex = 1;
			this.cbTPReleaseCompId.Text = "Send Component ID";
			this.cbTPReleaseCompId.UseVisualStyleBackColor = true;
			this.cbTPReleaseCompId.Click += new System.EventHandler(this.cbTPReleaseCompId_Click);
			// 
			// pageInitialization
			// 
			this.pageInitialization.BackColor = System.Drawing.Color.Transparent;
			this.pageInitialization.Controls.Add(this.tbUserCode);
			this.pageInitialization.Controls.Add(this.rbLoadCommands);
			this.pageInitialization.Controls.Add(this.rbAutoLoad);
			this.pageInitialization.Controls.Add(this.label1);
			this.pageInitialization.Location = new System.Drawing.Point(4, 22);
			this.pageInitialization.Name = "pageInitialization";
			this.pageInitialization.Padding = new System.Windows.Forms.Padding(3);
			this.pageInitialization.Size = new System.Drawing.Size(432, 332);
			this.pageInitialization.TabIndex = 0;
			this.pageInitialization.Text = "Initialization";
			this.pageInitialization.UseVisualStyleBackColor = true;
			this.pageInitialization.Resize += new System.EventHandler(this.pageInitialization_Resize);
			// 
			// pageTouchPressEvent
			// 
			this.pageTouchPressEvent.Controls.Add(this.cbTPPressCompId);
			this.pageTouchPressEvent.Location = new System.Drawing.Point(4, 22);
			this.pageTouchPressEvent.Name = "pageTouchPressEvent";
			this.pageTouchPressEvent.Padding = new System.Windows.Forms.Padding(3);
			this.pageTouchPressEvent.Size = new System.Drawing.Size(432, 332);
			this.pageTouchPressEvent.TabIndex = 1;
			this.pageTouchPressEvent.Text = "Touch Press Event";
			this.pageTouchPressEvent.UseVisualStyleBackColor = true;
			this.pageTouchPressEvent.Resize += new System.EventHandler(this.pageTouchPressEvent_Resize);
			// 
			// pageTouchReleaseEvent
			// 
			this.pageTouchReleaseEvent.Controls.Add(this.cbTPReleaseCompId);
			this.pageTouchReleaseEvent.Location = new System.Drawing.Point(4, 22);
			this.pageTouchReleaseEvent.Name = "pageTouchReleaseEvent";
			this.pageTouchReleaseEvent.Padding = new System.Windows.Forms.Padding(3);
			this.pageTouchReleaseEvent.Size = new System.Drawing.Size(432, 332);
			this.pageTouchReleaseEvent.TabIndex = 2;
			this.pageTouchReleaseEvent.Text = "Touch Release Event";
			this.pageTouchReleaseEvent.UseVisualStyleBackColor = true;
			this.pageTouchReleaseEvent.Resize += new System.EventHandler(this.pageTouchReleaseEvent_Resize);
			// 
			// pageTouchMove
			// 
			this.pageTouchMove.Location = new System.Drawing.Point(4, 22);
			this.pageTouchMove.Name = "pageTouchMove";
			this.pageTouchMove.Padding = new System.Windows.Forms.Padding(3);
			this.pageTouchMove.Size = new System.Drawing.Size(432, 332);
			this.pageTouchMove.TabIndex = 3;
			this.pageTouchMove.Text = "Touch Move";
			this.pageTouchMove.UseVisualStyleBackColor = true;
			this.pageTouchMove.Resize += new System.EventHandler(this.pageTouchMove_Resize);
			// 
			// ObjAttCompiler
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.tabControl);
			this.Name = "ObjAttCompiler";
			this.Size = new System.Drawing.Size(440, 358);
			this.Resize += new System.EventHandler(this.objattoo_Resize);
			this.tabControl.ResumeLayout(false);
			this.pageInitialization.ResumeLayout(false);
			this.pageInitialization.PerformLayout();
			this.pageTouchPressEvent.ResumeLayout(false);
			this.pageTouchPressEvent.PerformLayout();
			this.pageTouchReleaseEvent.ResumeLayout(false);
			this.pageTouchReleaseEvent.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion

		private void objattoo_Resize(object sender, EventArgs e)
		{
			try
			{
				tabControl.Top = 0;
				tabControl.Left = 0;
				tabControl.Width = base.Width;
				tabControl.Height = base.Height;
			}
			catch { }
		}

		private void rbAutoLoad_Click(object sender, EventArgs e)
		{
			if (rbAutoLoad.Checked && m_Object.ObjInfo.Panel.loadlei != 1)
			{
				m_Object.ObjInfo.Panel.loadlei = 1;
				ChangeAttribute(this, null);
				RefreshHead();
			}
		}

		private void rbLoadCommands_Click(object sender, EventArgs e)
		{
			if (rbLoadCommands.Checked && m_Object.ObjInfo.Panel.loadlei != 0)
			{
				m_Object.ObjInfo.Panel.loadlei = 0;
				ChangeAttribute(this, null);
				RefreshHead();
			}
		}

		private void RefreshHead()
		{
			int count = 0;
			if (tabControl.TabPages.Count > 0)
			{
				count = m_Object.Codes[0].Count;
				if (rbLoadCommands.Checked)
					count++;
				
				if (count > 0)
					pageInitialization.Text = string.Concat(pageInitialization.Tag, "(", count, ")");
				else
					pageInitialization.Text = pageInitialization.Tag.ToString();
			}
			if (tabControl.TabPages.Count > 1)
			{
				count = m_Object.Codes[1].Count;
				if (cbTPPressCompId.Checked)
					count++;

				if (count > 0)
					pageTouchPressEvent.Text = string.Concat(pageTouchPressEvent.Tag, "(", count, ")");
				else
					pageTouchPressEvent.Text = pageTouchPressEvent.Tag.ToString();
			}
			if (tabControl.TabPages.Count > 2)
			{
				count = m_Object.Codes[2].Count;
				if (cbTPReleaseCompId.Checked)
					count++;

				if (count > 0)
					pageTouchReleaseEvent.Text = string.Concat(pageTouchReleaseEvent.Tag, "(", count, ")");
				else
					pageTouchReleaseEvent.Text = pageTouchReleaseEvent.Tag.ToString();
			}
			if (tabControl.TabPages.Count > 3)
			{
				count = m_Object.Codes[3].Count;
				if (count > 0)
					pageTouchMove.Text = string.Concat(pageTouchMove.Tag, "(", count, ")");
				else
					pageTouchMove.Text = pageTouchMove.Tag.ToString();
			}
		}

		public void RefreshObject(HmiApplication app, HmiPage page, HmiObject obj)
		{
			try
			{
				SaveCodes();

				tabControl.Tag = "stop";
				if (app == null || page == null || obj == null)
				{
					Clear();
					return;
				}

				m_AppInf = app;
				m_Page = page;
				m_Object = obj;

				if (obj.ObjId == 0)
					rbAutoLoad.Visible = rbLoadCommands.Visible = false;
				else
					rbAutoLoad.Visible = rbLoadCommands.Visible = true;

				tabControl.Enabled = tbUserCode.Enabled = true;
				rbAutoLoad.Enabled = rbLoadCommands.Enabled = true;
				cbTPPressCompId.Enabled = cbTPReleaseCompId.Enabled = true;

				if (obj.ObjInfo.Panel.loadlei == 1)
					rbAutoLoad.Checked = true;
				else
					rbLoadCommands.Checked = true;

				cbTPPressCompId.Checked = ((obj.ObjInfo.Panel.SendKey & 2) > 0);
				cbTPReleaseCompId.Checked = ((obj.ObjInfo.Panel.SendKey & 1) > 0);

				int selectedIndex = tabControl.SelectedIndex;
				if (obj.Attributes[0].Data[0] == HmiObjType.OBJECT_TYPE_SLIDER)
				{
					if (!tabControl.TabPages.Contains(pageTouchPressEvent))
						tabControl.TabPages.Add(pageTouchPressEvent);
					if (!tabControl.TabPages.Contains(pageTouchReleaseEvent))
						tabControl.TabPages.Add(pageTouchReleaseEvent);
					if (!tabControl.TabPages.Contains(pageTouchMove))
						tabControl.TabPages.Add(pageTouchMove);

					pageInitialization.Tag = "Initialization".Translate();
					pageTouchPressEvent.Tag = "Touch Press Event".Translate();
					pageTouchReleaseEvent.Tag = "Touch Release Event".Translate();
					pageTouchMove.Tag = "Touch Move".Translate();
				}
				else if (obj.Attributes[0].Data[0] == HmiObjType.TIMER)
				{
					if (tabControl.TabPages.Contains(pageTouchPressEvent))
						tabControl.TabPages.Remove(pageTouchPressEvent);
					if (tabControl.TabPages.Contains(pageTouchReleaseEvent))
						tabControl.TabPages.Remove(pageTouchReleaseEvent);
					if (tabControl.TabPages.Contains(pageTouchMove))
						tabControl.TabPages.Remove(pageTouchMove);
					pageInitialization.Tag = "Timer Event".Translate();
					rbAutoLoad.Visible = false;
					rbLoadCommands.Visible = false;
				}
				else
				{
					if (!tabControl.TabPages.Contains(pageTouchPressEvent))
						tabControl.TabPages.Add(pageTouchPressEvent);
					if (!tabControl.TabPages.Contains(pageTouchReleaseEvent))
						tabControl.TabPages.Add(pageTouchReleaseEvent);
					if (tabControl.TabPages.Contains(pageTouchMove))
						tabControl.TabPages.Remove(pageTouchMove);
					pageInitialization.Tag = "Initialization".Translate();
					pageTouchPressEvent.Tag = "Touch Press Event".Translate();
					pageTouchReleaseEvent.Tag = "Touch Release Event".Translate();
				}
				if (selectedIndex >= tabControl.TabPages.Count)
					selectedIndex = 0;

				tabControl.SelectedIndex = selectedIndex;
				attrLoad(selectedIndex);
				RefreshHead();
				tabControl.Tag = "save";
			}
			catch (Exception ex)
			{
				MessageBox.Show("Load Component Error".Translate() + " " + ex.Message);
			}
		}

		private void tbUserCode_KeyDown(object sender, KeyEventArgs e)
		{
			if (!m_AppInf.ChangeApp)
				ChangeAttribute(this, null);
		}

		public void SaveCodes()
		{
			if (m_AppInf != null
			 && m_AppInf.ChangeApp
			 && tabControl.Tag.ToString().Trim() != "stop"
			 && m_Page != null
			 && m_Object != null
			 && m_codeIndex != 0xffff
				)
			{
				m_Object.Codes[m_codeIndex] = tbUserCode.Text.GetListBytes();
			}
		}

		private void cbTPPressCompId_Selected(object sender, TabControlEventArgs e)
		{
			SaveCodes();
			tbUserCode.Parent = e.TabPage;
			label1.Parent = e.TabPage;
			attrLoad(tabControl.SelectedIndex);
			RefreshHead();
		}

		private void pageInitialization_Resize(object sender, EventArgs e)
		{
			try
			{
				tbUserCode.Width = (pageInitialization.Width - tbUserCode.Left) - 1;
				tbUserCode.Height = (pageInitialization.Height - tbUserCode.Top) - 1;
			}
			catch { }
		}

		private void pageTouchPressEvent_Resize(object sender, EventArgs e)
		{
			try
			{
				tbUserCode.Width = (pageTouchPressEvent.Width - tbUserCode.Left) - 1;
				tbUserCode.Height = (pageTouchPressEvent.Height - tbUserCode.Top) - 1;
			}
			catch { }
		}

		private void pageTouchReleaseEvent_Resize(object sender, EventArgs e)
		{
			try
			{
				tbUserCode.Width = (pageTouchReleaseEvent.Width - tbUserCode.Left) - 1;
				tbUserCode.Height = (pageTouchReleaseEvent.Height - tbUserCode.Top) - 1;
			}
			catch { }
		}

		private void pageTouchMove_Resize(object sender, EventArgs e)
		{
			try
			{
				tbUserCode.Width = (pageTouchMove.Width - tbUserCode.Left) - 1;
				tbUserCode.Height = (pageTouchMove.Height - tbUserCode.Top) - 1;
			}
			catch { }
		}

		private void tbUserCode_KeyUp(object sender, KeyEventArgs e)
		{
			if (m_strStart != 0xffff)
			{
				tbUserCode.SetLineSelect(m_strStart);
				m_strStart = 0xffff;
			}
		}

		private void tbUserCode_TextChanged(object sender, EventArgs e)
		{
			m_strStart = tbUserCode.SelectionStart;
		}
	}
}