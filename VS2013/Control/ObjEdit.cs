using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace NextionEditor
{
	public class ObjEdit : UserControl
	{
		private ComboBox cbAttrValues;
		private IContainer components = null;
		private DataGridView dataGrid;
		private DataGridViewTextBoxColumn colIsBinding;
		private DataGridViewTextBoxColumn colCanModify;
		private DataGridViewTextBoxColumn colType;
		private DataGridViewTextBoxColumn colName;
		private DataGridViewTextBoxColumn colValue;
		private DataGridViewTextBoxColumn colInfo;
		private ContextMenuStrip contextMenu;
		private ToolStripMenuItem mi_DeleteMember;
		private ToolStripMenuItem mi_AddMember;
		private TextBox tbAttrDescription;
		private TextBox tbObjIdType;

		private int m_selectedRow = -1;
		private HmiApplication m_app;
		private HmiPage m_page;
		private HmiObject m_obj;
		private string m_savedValue = string.Empty;

		public event EventHandler ObjectAttach;
		public event EventHandler ObjectPosXY;

		public ObjEdit()
		{
			InitializeComponent();
		}

		private void Clear()
		{
			dataGrid.Rows.Clear();
			tbAttrDescription.Text = "";
			tbObjIdType.Text = "";
		}

		private void cbAttrValues_DropDownClosed(object sender, EventArgs e)
		{
			cbAttrValues.Visible = false;
		}

		private void cbAttrValues_SelectionChangeCommitted(object sender, EventArgs e)
		{
			try
			{
				int iRow = (int)cbAttrValues.Tag;
				cbAttrValues.Visible = false;
				if (iRow < dataGrid.Rows.Count)
				{
					string newValue = cbAttrValues.SelectedIndex.ToString();
					m_obj.SetAttrValue(dataGrid.Rows[iRow].Cells["colName"].Value.ToString(), newValue);
					dataGrid.Rows[iRow].Cells["colValue"].Value = cbAttrValues.Text;
				}
				dataGrid.Rows[iRow].Cells["colValue"].Selected = false;
				RefreshObject(m_app, m_page, m_obj);
				ObjectAttach(null, null);
			}
			catch { }
		}

		private void dataGrid_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (dataGrid.Rows.Count != 0 && e.RowIndex >= 0)
			{
				DataGridViewRow row = dataGrid.Rows[e.RowIndex];

				if (e.ColumnIndex == 1 && row.Cells["colType"].Value.ToString() == HmiAttributeType.Selection.ToString())
				{	// Attribute Selection
					Rectangle rectangle = dataGrid.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
					// 
					string[] strArray = row.Cells["colInfo"].Value.ToString().Split(Utility.CHAR_COLON);
					cbAttrValues.Items.Clear();
					if (strArray.Length > 1)
					{
						strArray = strArray[1].Split(Utility.CHAR_SEMICOLON);
						foreach (string str in strArray)
						{
							string[] strArray2 = str.Split(Utility.CHAR_MINUS);
							if (strArray2.Length == 2)
								cbAttrValues.Items.Add(strArray2[1]);
						}
					}
					cbAttrValues.Left = rectangle.Left;
					cbAttrValues.Top = rectangle.Top + dataGrid.Top;
					cbAttrValues.Width = rectangle.Width;
					cbAttrValues.Height = rectangle.Height;

					int value = Utility.GetInt(row.Cells["colValue"].Value.ToString());
					if (value < cbAttrValues.Items.Count)
						cbAttrValues.SelectedIndex = value;

					cbAttrValues.Tag = e.RowIndex;
					cbAttrValues.Visible = true;
					cbAttrValues.DroppedDown = true;
				}
				else
					cbAttrValues.Visible = false;

				if (e.ColumnIndex == 1 && !row.Cells["colValue"].ReadOnly)
				{
					dataGrid.CurrentCell = row.Cells["colValue"];
					dataGrid.BeginEdit(true);
				}

				if (row.Cells["colInfo"].Value != null)
					tbAttrDescription.Text = row.Cells["colInfo"].Value.ToString().Translate();

				m_selectedRow = e.RowIndex;
			}
		}

		private void dataGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (dataGrid.Rows.Count != 0
			 && e.RowIndex >= 0
			 && dataGrid.Columns[e.ColumnIndex].Name == "colValue"
				)
			{
				string str;
				if (dataGrid.Rows[e.RowIndex].Cells["colType"].Value.ToString() == HmiAttributeType.PicId.ToString())
				{
					FormParams formParams = new FormParams
					{
						Strings = new string[2]
					};
					string[] strArray = ((dataGrid.Rows[e.RowIndex].Cells["colValue"].Value == null)
										? ""
										: dataGrid.Rows[e.RowIndex].Cells["colValue"].Value.ToString()
										).Split(Utility.CHAR_MINUS);
					if (strArray.Length == 2)
						formParams.Strings[1] = strArray[1];

					Form form = new PicSelect(m_app, formParams);
					form.ShowDialog();
					if (form.DialogResult == DialogResult.OK)
					{
						str = formParams.Strings[0];
						dataGrid.Rows[e.RowIndex].Cells["colValue"].Value = str;
						m_obj.SetAttrValue(dataGrid.Rows[e.RowIndex].Cells["colName"].Value.ToString(), str);
						if (m_obj.Attributes[0].Data[0] == HmiObjType.OBJECT_TYPE_SLIDER
						 && dataGrid.Rows[e.RowIndex].Cells["colName"].Value.ToString() == "pic2"
							)
						{
							if (!m_obj.SetAttrValue("wid", m_app.Pictures[Utility.GetInt(str)].W.ToString()))
								m_obj.SetAttrValue("wid", ((m_obj.ObjInfo.Panel.EndX - m_obj.ObjInfo.Panel.X) + 1).ToString());

							if (!m_obj.SetAttrValue("hig", m_app.Pictures[Utility.GetInt(str)].H.ToString()))
								m_obj.SetAttrValue("hig", ((m_obj.ObjInfo.Panel.EndY - m_obj.ObjInfo.Panel.Y) + 1).ToString());

							RefreshObject(m_app, m_page, m_obj);
						}
						if (dataGrid.Rows[e.RowIndex].Cells["colIsBinding"].Value != null
						 && dataGrid.Rows[e.RowIndex].Cells["colIsBinding"].Value.ToString() == "1"
						 && str != ""
						 && Utility.GetInt(str) != 0xffff
						 && Utility.GetInt(str) >= 0
							)
						{
							m_obj.BindingPicXY(Utility.GetInt(str));
							ObjectPosXY(null, null);
						}
						ObjectAttach(null, null);
					}
				}
				else if (dataGrid.Rows[e.RowIndex].Cells["colType"].Value.ToString() == HmiAttributeType.Color.ToString())
				{
					ColorDialog dialog = new ColorDialog();
					if (dialog.ShowDialog() == DialogResult.OK)
					{
						str = Utility.Get16Color(dialog.Color).ToString();
						dataGrid.Rows[e.RowIndex].Cells["colValue"].Value = str;
						m_obj.SetAttrValue(dataGrid.Rows[e.RowIndex].Cells["colName"].Value.ToString(), str);
						ObjectAttach(null, null);
					}
				}
			}
		}

		private void dataGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			int oldValue = 0;
			int newValue;
			string newValueString = "";
			if (dataGrid.Rows.Count != 0
			 && e.RowIndex >= 0
			 && dataGrid.Columns[e.ColumnIndex].Name == "colValue"
				)
			{
				DataGridViewRow row = dataGrid.Rows[e.RowIndex];
				DataGridViewCell cellValue = row.Cells["colValue"];
				newValueString = (cellValue.Value == null)
								? ""
								: cellValue.Value.ToString();

				string colType = row.Cells["colType"].Value.ToString();

				if (colType == "objname")
				{
					m_app.RenameObj(m_page, m_obj, newValueString);
					cellValue.Value = m_obj.ObjName;
				}
				else if (row.Cells["colType"].Value.ToString() == "x")
				{
					oldValue = m_obj.ObjInfo.Panel.EndX - m_obj.ObjInfo.Panel.X + 1;
					newValue = Utility.GetInt(newValueString);
					if (newValue < 0
					 || newValue >= m_app.LcdWidth
					 || newValue + oldValue > m_app.LcdWidth
						)
					{
						MessageBox.Show("Error: Drawing off screen. Cancelled.".Translate());
						cellValue.Value = m_savedValue;
						return;
					}
					m_obj.ObjInfo.Panel.X = (ushort)newValue;
					m_obj.ObjInfo.Panel.EndX = (ushort)(m_obj.ObjInfo.Panel.X + oldValue - 1);
					cellValue.Value = m_obj.ObjInfo.Panel.X.ToString();
					ObjectPosXY(null, null);
				}
				else if (row.Cells["colType"].Value.ToString() == "y")
				{
					oldValue = (m_obj.ObjInfo.Panel.EndY - m_obj.ObjInfo.Panel.Y) + 1;
					newValue = Utility.GetInt(newValueString);
					if (newValue < 0
					 || newValue >= m_app.LcdHeight
					 || newValue + oldValue > m_app.LcdHeight
						)
					{
						MessageBox.Show("Error: Drawing off screen. Cancelled.".Translate());
						cellValue.Value = m_savedValue;
						return;
					}
					m_obj.ObjInfo.Panel.Y = (ushort)newValue;
					m_obj.ObjInfo.Panel.EndY = (ushort)(m_obj.ObjInfo.Panel.Y + oldValue - 1);
					cellValue.Value = m_obj.ObjInfo.Panel.Y.ToString();
					ObjectPosXY(null, null);
				}
				else if (row.Cells["colType"].Value.ToString() == "w")
				{
					newValue = Utility.GetInt(newValueString);
					if (newValue < 1 || m_obj.ObjInfo.Panel.X + newValue > m_app.LcdWidth)
					{
						MessageBox.Show("Error: Drawing off screen. Cancelled.".Translate());
						cellValue.Value = m_savedValue;
						return;
					}
					if (m_obj.IsBinding == 1)
					{
						MessageBox.Show("Shape size has been bound, can not be changed manually".Translate());
						cellValue.Value = m_savedValue;
						return;
					}
					m_obj.ObjInfo.Panel.EndX = (ushort)(m_obj.ObjInfo.Panel.X + newValue - 1);
					cellValue.Value = newValue.ToString();
					ObjectPosXY(null, null);
				}
				else if (row.Cells["colType"].Value.ToString() == "h")
				{
					newValue = Utility.GetInt(newValueString);
					if (newValue < 1 || m_obj.ObjInfo.Panel.Y + newValue > m_app.LcdHeight)
					{
						MessageBox.Show("Error: Drawing off screen. Cancelled.".Translate());
						cellValue.Value = m_savedValue;
						return;
					}
					if (m_obj.IsBinding == 1)
					{
						MessageBox.Show("Shape size has been bound, can not be changed manually".Translate());
						cellValue.Value = m_savedValue;
						return;
					}
					m_obj.ObjInfo.Panel.EndY = (ushort)(m_obj.ObjInfo.Panel.Y + newValue - 1);
					cellValue.Value = newValue.ToString();
					ObjectPosXY(null, null);
				}
				else
				{
					if (m_obj.SetAttrValue(row.Cells["colName"].Value.ToString(), newValueString))
					{
						if (dataGrid.Rows[e.RowIndex].Cells["colIsBinding"].Value != null
						 && dataGrid.Rows[e.RowIndex].Cells["colIsBinding"].Value.ToString() == "1"
						)
						{
							m_obj.BindingPicXY(Utility.GetInt(newValueString));
							ObjectPosXY(null, null);
						}
					}
					else
						cellValue.Value = m_savedValue;
				}
				ObjectAttach(null, null);
			}
		}

		private void dataGrid_CellEnter(object sender, DataGridViewCellEventArgs e)
		{
			if (e != null
			 && e.RowIndex < dataGrid.Rows.Count
			 && dataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null
				)
				m_savedValue = dataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
		}

		private void dataGrid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right
			 && m_obj != null
			 && Utility.GetString(m_obj.Attributes[0].Data) == "z"
				)
				contextMenu.Show(Control.MousePosition);
		}

		private void dataGrid_Paint(object sender, PaintEventArgs e)
		{
			try
			{
				dataGrid.Columns["colValue"].Width = (dataGrid.Width - dataGrid.Columns["colName"].Width) - 20;
			}
			catch { }
		}
		private void dataGrid_Scroll(object sender, ScrollEventArgs e)
		{
			cbAttrValues.DroppedDown = false;
			cbAttrValues.Visible = false;
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
			this.dataGrid = new System.Windows.Forms.DataGridView();
			this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colInfo = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colIsBinding = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colCanModify = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.mi_AddMember = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_DeleteMember = new System.Windows.Forms.ToolStripMenuItem();
			this.tbAttrDescription = new System.Windows.Forms.TextBox();
			this.tbObjIdType = new System.Windows.Forms.TextBox();
			this.cbAttrValues = new System.Windows.Forms.ComboBox();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
			this.contextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// dataGrid
			// 
			this.dataGrid.AllowUserToAddRows = false;
			this.dataGrid.AllowUserToDeleteRows = false;
			this.dataGrid.AllowUserToResizeRows = false;
			this.dataGrid.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.dataGrid.BackgroundColor = System.Drawing.Color.White;
			this.dataGrid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.dataGrid.ColumnHeadersVisible = false;
			this.dataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colName,
            this.colValue,
            this.colType,
            this.colInfo,
            this.colIsBinding,
            this.colCanModify});
			this.dataGrid.Location = new System.Drawing.Point(50, 64);
			this.dataGrid.MultiSelect = false;
			this.dataGrid.Name = "dataGrid";
			this.dataGrid.RowHeadersVisible = false;
			this.dataGrid.RowTemplate.Height = 23;
			this.dataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.dataGrid.ShowCellToolTips = false;
			this.dataGrid.Size = new System.Drawing.Size(228, 272);
			this.dataGrid.TabIndex = 0;
			this.dataGrid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGrid_CellClick);
			this.dataGrid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGrid_CellDoubleClick);
			this.dataGrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGrid_CellEndEdit);
			this.dataGrid.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGrid_CellEnter);
			this.dataGrid.Scroll += new System.Windows.Forms.ScrollEventHandler(this.dataGrid_Scroll);
			this.dataGrid.Paint += new System.Windows.Forms.PaintEventHandler(this.dataGrid_Paint);
			this.dataGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseDown);
			// 
			// colName
			// 
			this.colName.HeaderText = "Attribute";
			this.colName.Name = "colName";
			this.colName.ReadOnly = true;
			this.colName.Width = 80;
			// 
			// colValue
			// 
			this.colValue.HeaderText = "Value";
			this.colValue.Name = "colValue";
			this.colValue.ReadOnly = true;
			// 
			// colType
			// 
			this.colType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.colType.HeaderText = "Type";
			this.colType.MinimumWidth = 2;
			this.colType.Name = "colType";
			this.colType.ReadOnly = true;
			this.colType.Visible = false;
			this.colType.Width = 2;
			// 
			// colInfo
			// 
			this.colInfo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.colInfo.HeaderText = "Info";
			this.colInfo.Name = "colInfo";
			this.colInfo.Visible = false;
			this.colInfo.Width = 5;
			// 
			// colIsBinding
			// 
			this.colIsBinding.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.colIsBinding.HeaderText = "IsBinding";
			this.colIsBinding.Name = "colIsBinding";
			this.colIsBinding.Visible = false;
			this.colIsBinding.Width = 5;
			// 
			// colCanModify
			// 
			this.colCanModify.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.colCanModify.HeaderText = "CanModify";
			this.colCanModify.Name = "colCanModify";
			this.colCanModify.Visible = false;
			this.colCanModify.Width = 5;
			// 
			// contextMenu
			// 
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mi_AddMember,
            this.mi_DeleteMember});
			this.contextMenu.Name = "contextMenu";
			this.contextMenu.Size = new System.Drawing.Size(195, 56);
			// 
			// mi_AddMember
			// 
			this.mi_AddMember.Name = "mi_AddMember";
			this.mi_AddMember.Size = new System.Drawing.Size(194, 26);
			this.mi_AddMember.Text = "Add Members";
			this.mi_AddMember.Click += new System.EventHandler(this.mi_AddMember_Click);
			// 
			// mi_DeleteMember
			// 
			this.mi_DeleteMember.Name = "mi_DeleteMember";
			this.mi_DeleteMember.Size = new System.Drawing.Size(194, 26);
			this.mi_DeleteMember.Text = "Delete Members";
			this.mi_DeleteMember.Click += new System.EventHandler(this.mi_DeleteMember_Click);
			// 
			// tbAttrDescription
			// 
			this.tbAttrDescription.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.tbAttrDescription.Location = new System.Drawing.Point(50, 401);
			this.tbAttrDescription.Multiline = true;
			this.tbAttrDescription.Name = "tbAttrDescription";
			this.tbAttrDescription.ReadOnly = true;
			this.tbAttrDescription.Size = new System.Drawing.Size(228, 53);
			this.tbAttrDescription.TabIndex = 1;
			// 
			// tbObjIdType
			// 
			this.tbObjIdType.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.tbObjIdType.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tbObjIdType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.tbObjIdType.Location = new System.Drawing.Point(51, 1);
			this.tbObjIdType.Multiline = true;
			this.tbObjIdType.Name = "tbObjIdType";
			this.tbObjIdType.ReadOnly = true;
			this.tbObjIdType.Size = new System.Drawing.Size(226, 41);
			this.tbObjIdType.TabIndex = 2;
			// 
			// cbAttrValues
			// 
			this.cbAttrValues.BackColor = System.Drawing.Color.White;
			this.cbAttrValues.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbAttrValues.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbAttrValues.FormattingEnabled = true;
			this.cbAttrValues.Location = new System.Drawing.Point(91, 86);
			this.cbAttrValues.Name = "cbAttrValues";
			this.cbAttrValues.Size = new System.Drawing.Size(124, 21);
			this.cbAttrValues.TabIndex = 3;
			this.cbAttrValues.Visible = false;
			this.cbAttrValues.SelectionChangeCommitted += new System.EventHandler(this.cbAttrValues_SelectionChangeCommitted);
			this.cbAttrValues.DropDownClosed += new System.EventHandler(this.cbAttrValues_DropDownClosed);
			// 
			// ObjAtt
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.Controls.Add(this.cbAttrValues);
			this.Controls.Add(this.tbAttrDescription);
			this.Controls.Add(this.dataGrid);
			this.Controls.Add(this.tbObjIdType);
			this.Name = "ObjAttEdit";
			this.Size = new System.Drawing.Size(329, 454);
			this.Load += new System.EventHandler(this.ObjAttEdit_Load);
			this.Resize += new System.EventHandler(this.ObjAttEdit_Resize);
			((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
			this.contextMenu.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private void ObjAttEdit_Load(object sender, EventArgs e)
		{
			cbAttrValues.Visible = false;
		}

		private void ObjAttEdit_Resize(object sender, EventArgs e)
		{
			try
			{
				tbObjIdType.Top = 1;
				tbObjIdType.Left = 2;
				tbObjIdType.Width = base.Width - 4;
				dataGrid.Left = 2;
				dataGrid.Top = tbObjIdType.Height + 1;
				dataGrid.Width = base.Width - 4;
				dataGrid.Height = ((base.Height - dataGrid.Top) - tbAttrDescription.Height) - 1;
				tbAttrDescription.Left = 2;
				tbAttrDescription.Top = (base.Height - tbAttrDescription.Height) - 1;
				tbAttrDescription.Width = base.Width - 4;
			}
			catch { }
		}

		public void RefreshObject(HmiApplication app, HmiPage page, HmiObject obj)
		{
			if (app == null || page == null || obj == null)
			{
				Clear();
				return;
			}

			m_app = app;
			m_page = page;
			m_obj = obj;

			dataGrid.Rows.Clear();
			cbAttrValues.Visible = false;
			m_selectedRow = -1;

			if (m_obj.Attributes.Count < 1)
			{
				MessageBox.Show("Control property data header error".Translate());
				return;
			}

			byte[] buffer = new byte[1];
			byte[] buffer2 = new byte[1];
			string objType = "";
			int num3, row;

			dataGrid.Rows.Add();
			row = dataGrid.Rows.Count - 1;
			dataGrid.Rows[row].Cells["colValue"].Value = m_obj.ObjName;
			dataGrid.Rows[row].Cells["colName"].Value = "objname";
			dataGrid.Rows[row].Cells["colType"].Value = "objname";
			dataGrid.Rows[row].Cells["colInfo"].Value = "Component name".Translate();
			if (m_obj.Attributes[0].Data[0] == HmiObjType.PAGE)
				dataGrid.Rows[row].Cells["colValue"].ReadOnly = true;
			else
				dataGrid.Rows[row].Cells["colValue"].ReadOnly = false;

			DataGridViewCellStyle style = new DataGridViewCellStyle
			{
				Font = new Font(dataGrid.Font.FontFamily, dataGrid.Font.Size, FontStyle.Regular)
			};
			dataGrid.Rows[row].DefaultCellStyle = style;
			m_obj.IsBinding = 0;
			for (int i = 1; i < m_obj.Attributes.Count; i++)
			{
				HmiAttribute attr = m_obj.Attributes[i];
				if ((m_obj.checkAttribute(attr)
					&& (i != 1
						|| (m_obj.Attributes[0].Data[0] != HmiObjType.OBJECT_TYPE_CURVE
							&& m_obj.Attributes[0].Data[0] != HmiObjType.TIMER
							)
						)
					)
				 && attr.InfoAttribute.AttrType < 15
					)
				{
					dataGrid.Rows.Add();
					row = dataGrid.Rows.Count - 1;
					if (attr.InfoAttribute.AttrType < HmiAttributeType.String)
					{
						if (attr.InfoAttribute.Length == 1)
						{
							if (attr.InfoAttribute.AttrType == 5)
							{
								num3 = m_obj.GetNoteLength(Utility.GetString(attr.Name), true) - 1;
								dataGrid.Rows[row].Cells["colValue"].Value = num3.ToString();
							}
							else if (attr.InfoAttribute.AttrType == HmiAttributeType.Selection)
							{
								style = new DataGridViewCellStyle
								{
									BackColor = Color.FromArgb(0xe0, 0xe0, 0xe0),
									ForeColor = Color.Black
								};
								dataGrid.Rows[row].Cells["colValue"].Value = attr.Data[0].ToString();
								string[] strArray = Utility.GetString(attr.Note).Split(Utility.CHAR_COLON);
								if (strArray.Length > 1)
								{
									strArray = strArray[1].Split(Utility.CHAR_SEMICOLON);
									if (Utility.GetInt(attr.Data[0].ToString()) < strArray.Length)
									{
										strArray = strArray[Utility.GetInt(attr.Data[0].ToString())].Split(Utility.CHAR_MINUS);
										if (strArray.Length == 2)
											dataGrid.Rows[row].Cells["colValue"].Value = strArray[1];
									}
								}
								dataGrid.Rows[row].DefaultCellStyle = style;
							}
							else
								dataGrid.Rows[row].Cells["colValue"].Value = attr.Data[0].ToString();
						}
						else if (attr.InfoAttribute.Length == 2)
						{
							if (attr.InfoAttribute.AttrType == 1 && attr.Data.ToU16() == 0x350b)
								dataGrid.Rows[row].Cells["colValue"].Value = "";
							else if (attr.InfoAttribute.AttrType == 2 && attr.Data.ToU16() == 0xffff)
								dataGrid.Rows[row].Cells["colValue"].Value = "";
							else
								dataGrid.Rows[row].Cells["colValue"].Value = attr.Data.ToU16().ToString();
						}
						else if (attr.InfoAttribute.Length == 4)
							dataGrid.Rows[row].Cells["colValue"].Value = attr.Data.ToU32().ToString();
					}
					else
						dataGrid.Rows[row].Cells["colValue"].Value = Utility.GetString(attr.Data);

					dataGrid.Rows[row].Cells["colName"].Value = Utility.GetString(attr.Name);
					dataGrid.Rows[row].Cells["colType"].Value = attr.InfoAttribute.AttrType.ToString();
					dataGrid.Rows[row].Cells["colInfo"].Value = Utility.GetString(attr.Note, Encoding.ASCII.GetBytes("~")[0]);
					dataGrid.Rows[row].Cells["colIsBinding"].Value = attr.InfoAttribute.IsBinding.ToString();
					dataGrid.Rows[row].Cells["colCanModify"].Value = attr.InfoAttribute.IsBinding.ToString();

					if (attr.InfoAttribute.AttrType == HmiAttributeType.PicId
					 || attr.InfoAttribute.AttrType == HmiAttributeType.Color
					 || attr.InfoAttribute.AttrType == HmiAttributeType.Selection)
						dataGrid.Rows[row].Cells["colValue"].ReadOnly = true;
					else
						dataGrid.Rows[row].Cells["colValue"].ReadOnly = false;

					style = new DataGridViewCellStyle();
					if (attr.InfoAttribute.CanModify == 1)
					{
						style.BackColor = Color.White;
						style.ForeColor = Color.Green;
						dataGrid.Rows[row].DefaultCellStyle = style;
					}

					if (attr.InfoAttribute.IsReturn == 1)
					{
						style.Font = new Font(dataGrid.Font.FontFamily, dataGrid.Font.Size, FontStyle.Bold);
						dataGrid.Rows[row].DefaultCellStyle = style;
					}
				}
			}

			if (m_obj.Attributes[0].Data[0] != HmiObjType.PAGE
			 && m_obj.ObjInfo.ObjType != HmiObjType.TIMER
			 && m_obj.ObjInfo.ObjType != HmiObjType.VAR
				)
			{
				dataGrid.Rows.Add();
				row = dataGrid.Rows.Count - 1;
				dataGrid.Rows[row].Cells["colValue"].Value = m_obj.ObjInfo.Panel.X.ToString();
				dataGrid.Rows[row].Cells["colName"].Value = "x";
				dataGrid.Rows[row].Cells["colType"].Value = "x";
				dataGrid.Rows[row].Cells["colInfo"].Value = "Coordinate X".Translate();
				dataGrid.Rows[row].Cells["colValue"].ReadOnly = false;

				dataGrid.Rows.Add();
				row = dataGrid.Rows.Count - 1;
				dataGrid.Rows[row].Cells["colValue"].Value = m_obj.ObjInfo.Panel.Y.ToString();
				dataGrid.Rows[row].Cells["colName"].Value = "y";
				dataGrid.Rows[row].Cells["colType"].Value = "y";
				dataGrid.Rows[row].Cells["colInfo"].Value = "Coordinate Y".Translate();
				dataGrid.Rows[row].Cells["colValue"].ReadOnly = false;
				dataGrid.Rows.Add();

				row = dataGrid.Rows.Count - 1;
				num3 = (m_obj.ObjInfo.Panel.EndX - m_obj.ObjInfo.Panel.X) + 1;
				dataGrid.Rows[row].Cells["colValue"].Value = num3.ToString();
				dataGrid.Rows[row].Cells["colName"].Value = "w";
				dataGrid.Rows[row].Cells["colType"].Value = "w";
				dataGrid.Rows[row].Cells["colInfo"].Value = "Width".Translate();
				dataGrid.Rows[row].Cells["colValue"].ReadOnly = false;
				dataGrid.Rows.Add();

				row = dataGrid.Rows.Count - 1;
				dataGrid.Rows[row].Cells["colValue"].Value = ((m_obj.ObjInfo.Panel.EndY - m_obj.ObjInfo.Panel.Y) + 1).ToString();
				dataGrid.Rows[row].Cells["colName"].Value = "h";
				dataGrid.Rows[row].Cells["colType"].Value = "h";
				dataGrid.Rows[row].Cells["colInfo"].Value = "Height".Translate();
				dataGrid.Rows[row].Cells["colValue"].ReadOnly = false;
			}

			if (m_obj.Attributes.Count > 0
			 && Utility.GetString(m_obj.Attributes[0].Name) == "lei"
				)
				objType = Utility.GetString(m_obj.Attributes[0].Note, Encoding.ASCII.GetBytes("~")[0]);

			tbAttrDescription.Text = "Click the attribute to display corresponding notes".Translate();
			tbObjIdType.Text = "ID:" + m_obj.ObjId.ToString() + " " + objType;

			if (dataGrid.Rows.Count > 0)
				dataGrid.Rows[0].Cells[0].Selected = false;
		}

		private void mi_DeleteMember_Click(object sender, EventArgs e)
		{
			string name = "";
			if (m_selectedRow == -1)
				MessageBox.Show("Please select members".Translate());
			else
			{
				name = dataGrid.Rows[m_selectedRow].Cells[0].Value.ToString();
				if (MessageBox.Show(
						"Are you sure to delete the member?".Translate() + name,
						"Confirm".Translate(),
						MessageBoxButtons.OKCancel
					) == DialogResult.OK)
				{
					m_obj.DeleteAttribute(name);
					RefreshObject(m_app, m_page, m_obj);
				}
			}
		}

		private void mi_AddMember_Click(object sender, EventArgs e)
		{
			Form form = new MemberAdd(m_app, m_page, m_obj);
			form.ShowDialog();
			if (form.DialogResult == DialogResult.OK)
				RefreshObject(m_app, m_page, m_obj);
		}
	}
}
