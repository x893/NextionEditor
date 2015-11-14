using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using NextionEditor.Properties;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace NextionEditor
{
	public class main : Form
	{
		#region Variables
		private IContainer components = null;
		private Button btnSaveAsComponent;
		private GroupBox groupBox1;
		private MenuStrip menuStrip;
		private System.Windows.Forms.ToolStripMenuItem mi_DualButton;
		private System.Windows.Forms.ToolStripMenuItem mi_Checkbox;
		private System.Windows.Forms.ToolStripMenuItem mi_Timer;
		private System.Windows.Forms.ToolStripMenuItem mi_RadioBox;
		private System.Windows.Forms.ToolStripMenuItem miFontGenerator;
		private System.Windows.Forms.ToolStripMenuItem mi_Slider;
		private System.Windows.Forms.ToolStripMenuItem mi_Variable;
		private System.Windows.Forms.ToolStripMenuItem mi_Waveform;
		private System.Windows.Forms.ToolStripMenuItem miImportProject;
		private System.Windows.Forms.ToolStripMenuItem mi_Button;
		private System.Windows.Forms.ToolStripMenuItem mi_Chinese;
		private System.Windows.Forms.ToolStripMenuItem mi_DeleteAll;
		private System.Windows.Forms.ToolStripMenuItem mi_DeleteSelected;
		private System.Windows.Forms.ToolStripMenuItem mi_English;
		private System.Windows.Forms.ToolStripMenuItem mi_ProgressBar;
		private System.Windows.Forms.ToolStripMenuItem mi_Language;
		private System.Windows.Forms.ToolStripMenuItem mi_Picture;
		private System.Windows.Forms.ToolStripMenuItem mi_CropImage;
		private System.Windows.Forms.ToolStripMenuItem miEyeDropper;
		private System.Windows.Forms.ToolStripMenuItem mi_Text;
		private System.Windows.Forms.ToolStripMenuItem miTools;
		private System.Windows.Forms.ToolStripMenuItem mi_Hotspot;
		private System.Windows.Forms.ToolStripMenuItem mi_Gauges;
		private System.Windows.Forms.ToolStripMenuItem miSaveAs;
		private System.Windows.Forms.ToolStripMenuItem miOpenBuildFolder;
		private System.Windows.Forms.ToolStripMenuItem mi_Number;
		private System.Windows.Forms.ToolStripMenuItem miOpen2;
		private System.Windows.Forms.ToolStripMenuItem miNew2;
		private System.Windows.Forms.ToolStripMenuItem miSave2;
		private System.Windows.Forms.ToolStripMenuItem miSaveOther;
		private System.Windows.Forms.ToolStripMenuItem mi_Help;
		private System.Windows.Forms.ToolStripMenuItem miCloseProject;
		private System.Windows.Forms.ToolStripMenuItem miExit;
		private System.Windows.Forms.ToolStripMenuItem mi_File;
		private StatusStrip statusStrip;
		private ToolStripStatusLabel statusLabel;
		private ToolStrip toolBar;
		private ToolStripButton mi_Resolution;
		private ToolStripButton mi_ID;
		private ToolStripButton mi_New;
		private ToolStripButton mi_Open;
		private ToolStripButton mi_Compile;
		private ToolStripButton mi_Copy;
		private ToolStripButton mi_Save;
		private ToolStripButton mi_Debug;
		private ToolStripButton mi_Upload;
		private ToolStripButton mi_XY;
		private ToolStripButton mi_Paste;
		private ToolStripDropDownButton mi_AddComponent;
		private ToolStripDropDownButton mi_DeleteComponent;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripSeparator toolStripSeparator3;
		private ToolStripSeparator toolStripSeparator4;
		private ToolStripSeparator toolStripSeparator5;
		private ToolStripSeparator toolStripSeparator6;
		private ToolStripSeparator toolStripSeparator7;
		private ToolStripSeparator toolStripSeparator8;
		private Label label1;
		private Label label2;
		private Label label3;
		private Label label4;
		private Label label5;
		private ListBox listBox1;
		private ListBox listBox2;
		private ListBox listBox4;
		private Splitter splitter1;
		private Splitter splitter2;
		private Splitter splitter3;
		private Splitter splitter4;
		private Splitter splitter6;
		private Panel panelLeft;
		private Panel panelBottom;
		private Panel panelView;
		private Panel panelRight;
		private Panel panel9;
		private Panel panelPageAdmin;
		private Panel panelFontAdmin;
		private Panel panelPicAdmin;
		private Panel panelObjAttrs;
		private RichTextBox tbCompilerOutput;
		private TextBox textBox1;
		private TextBox textBox2;
		private TextBox textBox3;
		private PageAdmin pageAdmin;
		private PicAdmin picAdmin;
		private FontAdmin fontAdmin;
		private HmiSimulator runScreen;
		private ToolStripSeparator separator10;
		private ToolStripSeparator separator11;
		private ToolStripSeparator separator9;

		private int m_objXPos;
		private bool m_showXY = true;
		private string m_binpath = "";
		private string m_comData;
		private HmiApplication m_app = null;
		private HmiObject m_copyObj = null;
		private HmiObject m_obj = null;
		private HmiPage m_page = null;
		private HmiObjectEdit m_objEdit = null;
		private ObjCompiler m_compiler;
		private ObjEdit m_attributeEdit;
		private string m_openFile;

		#endregion

		public static main Form;
		#region Constructor
		public main()
		{
			InitializeComponent();
			Utility.Translate(this);
			Form = this;
		}
		#endregion

		#region addComponent
		private void addComponent(byte lei, byte mark)
		{
			try
			{
				if (m_page != null)
				{
					HmiObject item = new HmiObject(m_app, m_page);
					if (lei == 2)
					{
						if (m_copyObj == null)
						{
							MessageBox.Show("Please copy component".Translate());
							return;
						}
						item = m_copyObj.CopyObject(m_app, m_page);
					}
					else if (lei == 0)
					{
						item.ObjInfo.Panel.X = 0;
						item.ObjInfo.Panel.Y = 0;
						item.ObjInfo.Panel.EndX = 100;
						item.ObjInfo.Panel.EndY = 100;
						item.ObjInfo.Panel.loadlei = 1;
					}
					else if (lei > 2)
						item.Attributes = ObjAttOperation.CreateAttrByType(mark, ref item.ObjInfo.Panel);

					item.ObjInfo.Panel.EndX = (ushort)(item.ObjInfo.Panel.EndX - item.ObjInfo.Panel.X);
					item.ObjInfo.Panel.EndY = (ushort)(item.ObjInfo.Panel.EndY - item.ObjInfo.Panel.Y);
					item.ObjInfo.Panel.X = 0;
					item.ObjInfo.Panel.Y = 0;
					item.ObjInfo.ObjType = mark;

					m_page.HmiObjects.Add(item);
					m_app.MakeObjName(m_page, item, mark);
					m_app.RefreshObjId(m_page);
					m_app.ChangeApplication(true);

					if (item.Attributes[0].Data[0] == HmiObjType.TIMER || item.Attributes[0].Data[0] == HmiObjType.VAR)
						loadObj(item);
					else
						runScreen.LoadObj(item);
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}
		}
		#endregion

		private void m_app_ChangeEvent(bool change)
		{
			if (m_app != null)
			{
				m_app.ChangeApp = change;
				mi_Save.Enabled = change;
			}
			else
				mi_Save.Enabled = false;
		}

		private void btnSaveAsComponent_Click(object sender, EventArgs e)
		{
			SaveFileDialog dialog = new SaveFileDialog
			{
				Filter = "tft File|*.tft|txt File|*.txt|All File|*.*".Translate()
			};
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				StreamWriter writer = new StreamWriter(dialog.FileName);
				byte[] bytesssASCII = textBox3.Text.ToBytes();
				writer.BaseStream.Write(bytesssASCII, 0, bytesssASCII.Length);
				writer.Close();
				writer.Dispose();
			}
		}

		private void closeHMI()
		{
			m_app = null;
			m_page = null;

			m_compiler.RefreshObject(null, null, null);
			m_attributeEdit.RefreshObject(null, null, null);

			pageAdmin.SetAppInfo(m_app);
			fontAdmin.SetAppInfo(m_app);
			picAdmin.SetAppInfo(m_app);

			runScreen.RunStop();
			runScreen.Visible = false;

			m_obj = null;
			m_objEdit = null;

			m_binpath = "";

			mi_Save.Enabled = false;
			mi_Copy.Enabled = false;
			mi_Paste.Enabled = false;
			mi_DeleteComponent.Enabled = false;
			mi_Resolution.Enabled = false;
			mi_ID.Enabled = false;
			mi_XY.Enabled = false;
			mi_Compile.Enabled = false;

			mi_AddComponent.Enabled = false;

			if (m_app != null)
				m_app.ChangeApplication(false);

			refreshTimerVar();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="delete_all">1 = all components, 0 = current conponent</param>
		private void deleteObj(int delete_all)
		{
			bool flag = false;
			if (m_page == null)
			{
				MessageBox.Show("Please select page".Translate());
				return;
			}
			if (delete_all == 1)
			{
				m_page.DeleteAllObj();
				refreshPage();
			}
			else if (delete_all != 2)
			{
				if (m_obj == null)
				{
					MessageBox.Show("Please select component".Translate());
					return;
				}

				if (m_obj.Attributes[0].Data[0] == HmiObjType.PAGE)
				{
					MessageBox.Show("Please select component".Translate());
					return;
				}
				m_obj.Page.delobj(m_obj);
				if (m_objEdit.HmiObject.Attributes[0].Data[0] == HmiObjType.TIMER
				 || m_objEdit.HmiObject.Attributes[0].Data[0] == HmiObjType.VAR
					)
					flag = true;
				m_objEdit.Dispose();
				runScreen.HmiObjectEdit.Dispose();
			}

			m_obj = null;
			m_objEdit = null;
			runScreen.HmiObjectEdit = null;
			m_compiler.RefreshObject(m_app, m_page, m_obj);
			m_attributeEdit.RefreshObject(m_app, m_page, m_obj);
			m_app.ChangeApplication(true);
			if (flag)
				refreshTimerVar();
		}

		#region fileOperation
		private void mi_Save_Click(object sender, EventArgs e)
		{
			fileOperation("save", "");
		}
		private void mi_Compile_Click(object sender, EventArgs e)
		{
			fileOperation("compile", "");
		}
		private void mi_SaveAs_Click(object sender, EventArgs e)
		{
			fileOperation("saveAs", "");
		}
		private void mi_Open_Click(object sender, EventArgs e)
		{
			fileOperation("open", "");
		}
		private void mi_New_Click(object sender, EventArgs e)
		{
			fileOperation("add", "");
		}

		private bool fileOperation(string cmd, string filename)
		{
			if (cmd != null)
			{
				SaveFileDialog dialog;

				#region add
				if (cmd == "add")
				{
					if (m_app != null && m_app.ChangeApp)
					{
						switch (MessageBox.Show(" Do you want to save changes made? ".Translate(), "Confirm".Translate(), MessageBoxButtons.YesNoCancel))
						{
							case DialogResult.Yes:
								if (!fileOperation("save", ""))
									return false;
								break;
							case DialogResult.Cancel:
								return false;
						}
					}

					dialog = new SaveFileDialog();
					dialog.Filter = "HMI File|*.HMI|All File|*.*".Translate();
					Utility.SetInitialPath(dialog, "file");
					if (dialog.ShowDialog() != DialogResult.OK)
						return false;
					closeHMI();

					m_app = new HmiApplication();
					m_app.ChangeApplication = new HmiApplication.AppChangEvent(m_app_ChangeEvent);
					m_app.FileSave = new HmiApplication.AppFileSave(m_app_FileSaveEvent);
					m_app.ChangeApplication(false);

					m_app.AddPage();

					if (new DeviceParameters(m_app).ShowDialog() != DialogResult.OK)
						return false;
					
					runScreen.RunStop();

					m_openFile = dialog.FileName;
					Utility.SavePath(dialog, "file");
					m_app.SaveFile(m_openFile, false, null);
					closeHMI();

					m_app = new HmiApplication();
					m_app.ChangeApplication = new HmiApplication.AppChangEvent(m_app_ChangeEvent);
					m_app.FileSave = new HmiApplication.AppFileSave(m_app_FileSaveEvent);
					m_app.ChangeApplication(false);

					if (!m_app.Open(m_openFile))
						return false;
					if (!Utility.DeleteFileWait(HmiOptions.RunFilePath))
						return false;

					File.Copy(m_openFile, HmiOptions.RunFilePath);

					runScreen.GuiInit(HmiOptions.RunFilePath, m_app, true);
					runScreen.Visible = true;

					pageAdmin.SetAppInfo(m_app);
					picAdmin.SetAppInfo(m_app);
					fontAdmin.SetAppInfo(m_app);
					pageAdmin.RefreshObject(0);
					picAdmin.RefreshPictures();
					fontAdmin.RefreshFonts();

					mi_Compile.Enabled = true;
					mi_AddComponent.Enabled = true;
					mi_DeleteComponent.Enabled = true;
					mi_Resolution.Enabled = true;
					mi_ID.Enabled = true;
					mi_XY.Enabled = true;
				}
				#endregion

				#region open
				else if (cmd == "open")
				{
					if (m_app != null && m_app.ChangeApp)
					{
						switch (MessageBox.Show(
								" Do you want to save changes made? ".Translate(),
								"Confirm".Translate(),
								MessageBoxButtons.YesNoCancel
								))
						{
							case DialogResult.Yes:
								if (!fileOperation("save", ""))
									return false;
								break;

							case DialogResult.Cancel:
								return false;
						}
					}

					OpenFileDialog op = new OpenFileDialog();
					if (filename == "")
					{
						op.Filter = "HMI File|*.HMI|All File|*.*".Translate();
						Utility.SetInitialPath(op, "file");
						if (op.ShowDialog() != DialogResult.OK)
							return false;
						Utility.SavePath(op, "file");
						m_openFile = op.FileName;
					}
					else
						m_openFile = filename;

					closeHMI();

					m_app = new HmiApplication();
					m_app.ChangeApplication = new HmiApplication.AppChangEvent(m_app_ChangeEvent);
					m_app.FileSave = new HmiApplication.AppFileSave(m_app_FileSaveEvent);
					m_app.ChangeApplication(false);

					if (!m_app.Open(m_openFile))
						return false;

					if (!Utility.DeleteFileWait(HmiOptions.RunFilePath))
						return false;

					File.Copy(m_openFile, HmiOptions.RunFilePath);
					runScreen.GuiInit(HmiOptions.RunFilePath, m_app, true);
					runScreen.Visible = true;

					pageAdmin.SetAppInfo(m_app);
					picAdmin.SetAppInfo(m_app);
					fontAdmin.SetAppInfo(m_app);

					pageAdmin.RefreshObject(0);
					picAdmin.RefreshPictures();
					fontAdmin.RefreshFonts();

					mi_Compile.Enabled = true;
					mi_AddComponent.Enabled = true;
					mi_DeleteComponent.Enabled = true;
					mi_Resolution.Enabled = true;
					mi_ID.Enabled = true;
					mi_XY.Enabled = true;
				}
				#endregion

				#region save / savexml
				else if (cmd == "save" || cmd == "savexml")
				{
					if (m_app == null
					 || m_openFile == null
					 || HmiOptions.RunFilePath == null
						)
						return false;

					runScreen.PauseScreen();
					if (cmd == "save")
					{
						m_compiler.SaveCodes();
						m_binpath = "";
						if (!m_app.SaveFile(HmiOptions.RunFilePath, false, null))
						{
							runScreen.StartFile();
							return false;
						}

						if (!Utility.DeleteFileWait(m_openFile))
						{
							runScreen.StartFile();
							return false;
						}
						File.Copy(HmiOptions.RunFilePath, m_openFile);
					}
					else
					{
						dialog = new SaveFileDialog();
						dialog.Filter = "XML File|*.xml|All File|*.*".Translate();
						dialog.FileName = Path.GetFileNameWithoutExtension(m_openFile) + ".xml";
						Utility.SetInitialPath(dialog, "file");
						if (dialog.ShowDialog() != DialogResult.OK)
							return false;

						try
						{
							if (File.Exists(dialog.FileName))
								File.Delete(dialog.FileName);
							using (TextWriter writer = new StreamWriter(dialog.FileName, false, Encoding.UTF8))
							{
								XmlSerializer xs = new XmlSerializer(runScreen.GuiApp.GetType());
								xs.Serialize(writer, runScreen.GuiApp);
								writer.Close();
							}
							// xdoc.Save(dialog.FileName);
						}
						catch(Exception ex)
						{
							MessageBox.Show("Export XML:".Translate() + "\n" + ex.Message);

						}
					}
					runScreen.StartFile();
					m_app.ChangeApplication(false);
				}
				#endregion

				#region compile
				else if (cmd == "compile")
				{
					if (m_app == null || m_openFile == null)
						return false;

					if (m_app.ChangeApp)
					{
						runScreen.PauseScreen();
						m_compiler.SaveCodes();
						if (!m_app.SaveFile(HmiOptions.RunFilePath, false, null))
						{
							runScreen.StartFile();
							return false;
						}
						if (!Utility.DeleteFileWait(m_openFile))
						{
							runScreen.StartFile();
							return false;
						}
						File.Copy(HmiOptions.RunFilePath, m_openFile);
						runScreen.StartFile();
						m_app.ChangeApplication(false);
					}
					if (!Directory.Exists(HmiOptions.AppDataBinPath))
						Directory.CreateDirectory(HmiOptions.AppDataBinPath);

					m_binpath = Path.Combine(HmiOptions.AppDataBinPath, Path.GetFileNameWithoutExtension(m_openFile) + ".tft");
					label2.ForeColor = Color.Black;

					if (m_app.SaveFile(m_binpath, true, tbCompilerOutput))
						m_app.ChangeApplication(false);
					else
					{
						m_binpath = "";
						label2.ForeColor = Color.Red;
						return false;
					}
				}
				#endregion

				#region saveAs
				else if (cmd == "saveAs" && m_app != null)
				{
					dialog = new SaveFileDialog
					{
						Filter = "HMI File|*.HMI|All File|*.*".Translate()
					};
					Utility.SetInitialPath(dialog, "file");
					if (dialog.ShowDialog() != DialogResult.OK)
						return false;

					m_openFile = dialog.FileName;
					Utility.SavePath(dialog, "file");
					runScreen.PauseScreen();
					if (m_app.SaveFile(HmiOptions.RunFilePath, false, null))
					{
						if (!Utility.DeleteFileWait(m_openFile))
						{
							runScreen.StartFile();
							return false;
						}
						File.Copy(HmiOptions.RunFilePath, m_openFile);
					}
					runScreen.StartFile();
					refreshPage();
					m_app.ChangeApplication(false);
					return true;
				}
				#endregion
			}

			if (m_app != null)
				showUsageSpace();

			panelView.Refresh();
			return true;
		}
		#endregion

		#region ucRunScreen_MouseWheel
		private void setZoomFactor(int delta)
		{
			if (runScreen.InvokeRequired)
				runScreen.Invoke(new Action<int>(setZoomFactor), delta);
			else if (runScreen.SetZoom(delta))
			{
				resizeForm();
			}
		}
		private void runScreen_MouseWheel(object sender, MouseEventArgs e)
		{
			if (runScreen.Visible)
			{
				setZoomFactor(e.Delta);
				((HandledMouseEventArgs)e).Handled = true;
			}
		}
		#endregion

		#region main_Load
		private void main_Load(object sender, EventArgs e)
		{
			// runScreen.MouseWheel += new MouseEventHandler(runScreen_MouseWheel);

			panel9.HorizontalScroll.Value = panel9.HorizontalScroll.Maximum;
			m_app_ChangeEvent(false);
			closeHMI();
			Text = HmiOptions.SoftName;
			base.Icon = HmiOptions.Icon;
			m_page = null;
			m_compiler.RefreshObject(null, null, null);
			m_attributeEdit.RefreshObject(null, null, null);
			string[] commandLineArgs = Environment.GetCommandLineArgs();

			foreach (string filename in commandLineArgs)
				if (Path.GetExtension(filename).ToLowerInvariant() == ".hmi"
				 && File.Exists(Path.Combine(Application.StartupPath, filename))
					)
				{
					fileOperation("open", Path.Combine(Application.StartupPath, filename));
					break;
				}

			picAdmin.SetSizeToParent();
			fontAdmin.SetSizeToParent();
			pageAdmin.SetSizeToParent();
			m_attributeEdit.SetSizeToParent();
		}
		#endregion
		#region main_FormClosing
		private void main_FormClosing(object sender, FormClosingEventArgs e)
		{
			if ((m_app != null) && m_app.ChangeApp)
			{
				switch (MessageBox.Show(
					" Do you want to save changes made? ".Translate(),
					"Confirm".Translate(),
					MessageBoxButtons.YesNoCancel)
					)
				{
					case DialogResult.Yes:
						if (!fileOperation("save", ""))
						{
							e.Cancel = true;
							return;
						}
						break;

					case DialogResult.Cancel:
						e.Cancel = true;
						return;
				}
			}
			Environment.Exit(0);
		}
		#endregion

		#region  InitializeComponent
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(main));
			this.tbCompilerOutput = new System.Windows.Forms.RichTextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.listBox2 = new System.Windows.Forms.ListBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.btnSaveAsComponent = new System.Windows.Forms.Button();
			this.listBox4 = new System.Windows.Forms.ListBox();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.mi_File = new System.Windows.Forms.ToolStripMenuItem();
			this.miNew2 = new System.Windows.Forms.ToolStripMenuItem();
			this.miOpen2 = new System.Windows.Forms.ToolStripMenuItem();
			this.miOpenBuildFolder = new System.Windows.Forms.ToolStripMenuItem();
			this.separator10 = new System.Windows.Forms.ToolStripSeparator();
			this.miSave2 = new System.Windows.Forms.ToolStripMenuItem();
			this.miSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.miSaveOther = new System.Windows.Forms.ToolStripMenuItem();
			this.separator9 = new System.Windows.Forms.ToolStripSeparator();
			this.miImportProject = new System.Windows.Forms.ToolStripMenuItem();
			this.miCloseProject = new System.Windows.Forms.ToolStripMenuItem();
			this.separator11 = new System.Windows.Forms.ToolStripSeparator();
			this.miExit = new System.Windows.Forms.ToolStripMenuItem();
			this.miTools = new System.Windows.Forms.ToolStripMenuItem();
			this.miFontGenerator = new System.Windows.Forms.ToolStripMenuItem();
			this.miEyeDropper = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_Help = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_Language = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_English = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_Chinese = new System.Windows.Forms.ToolStripMenuItem();
			this.toolBar = new System.Windows.Forms.ToolStrip();
			this.mi_Open = new System.Windows.Forms.ToolStripButton();
			this.mi_New = new System.Windows.Forms.ToolStripButton();
			this.mi_Save = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.mi_Compile = new System.Windows.Forms.ToolStripButton();
			this.mi_Debug = new System.Windows.Forms.ToolStripButton();
			this.mi_Upload = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.mi_AddComponent = new System.Windows.Forms.ToolStripDropDownButton();
			this.mi_Text = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_Number = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_Button = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_ProgressBar = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_Picture = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_CropImage = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_Hotspot = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_Gauges = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_Waveform = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_Slider = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_Timer = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_Variable = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_DualButton = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_RadioBox = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_Checkbox = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_DeleteComponent = new System.Windows.Forms.ToolStripDropDownButton();
			this.mi_DeleteSelected = new System.Windows.Forms.ToolStripMenuItem();
			this.mi_DeleteAll = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.mi_Copy = new System.Windows.Forms.ToolStripButton();
			this.mi_Paste = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
			this.mi_Resolution = new System.Windows.Forms.ToolStripButton();
			this.mi_ID = new System.Windows.Forms.ToolStripButton();
			this.mi_XY = new System.Windows.Forms.ToolStripButton();
			this.panelLeft = new System.Windows.Forms.Panel();
			this.panelPicAdmin = new System.Windows.Forms.Panel();
			this.picAdmin = new NextionEditor.PicAdmin();
			this.splitter4 = new System.Windows.Forms.Splitter();
			this.panelFontAdmin = new System.Windows.Forms.Panel();
			this.fontAdmin = new NextionEditor.FontAdmin();
			this.panelRight = new System.Windows.Forms.Panel();
			this.panelPageAdmin = new System.Windows.Forms.Panel();
			this.pageAdmin = new NextionEditor.PageAdmin();
			this.splitter6 = new System.Windows.Forms.Splitter();
			this.panelObjAttrs = new System.Windows.Forms.Panel();
			this.m_attributeEdit = new NextionEditor.ObjEdit();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.splitter2 = new System.Windows.Forms.Splitter();
			this.panelBottom = new System.Windows.Forms.Panel();
			this.m_compiler = new NextionEditor.ObjCompiler();
			this.panel9 = new System.Windows.Forms.Panel();
			this.splitter3 = new System.Windows.Forms.Splitter();
			this.panelView = new System.Windows.Forms.Panel();
			this.runScreen = new NextionEditor.HmiSimulator();
			this.groupBox1.SuspendLayout();
			this.statusStrip.SuspendLayout();
			this.menuStrip.SuspendLayout();
			this.toolBar.SuspendLayout();
			this.panelLeft.SuspendLayout();
			this.panelPicAdmin.SuspendLayout();
			this.panelFontAdmin.SuspendLayout();
			this.panelRight.SuspendLayout();
			this.panelPageAdmin.SuspendLayout();
			this.panelObjAttrs.SuspendLayout();
			this.panelBottom.SuspendLayout();
			this.panelView.SuspendLayout();
			this.SuspendLayout();
			// 
			// tbCompilerOutput
			// 
			this.tbCompilerOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbCompilerOutput.Location = new System.Drawing.Point(4, 30);
			this.tbCompilerOutput.Margin = new System.Windows.Forms.Padding(4);
			this.tbCompilerOutput.Name = "tbCompilerOutput";
			this.tbCompilerOutput.ReadOnly = true;
			this.tbCompilerOutput.Size = new System.Drawing.Size(235, 150);
			this.tbCompilerOutput.TabIndex = 180;
			this.tbCompilerOutput.Text = "";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label2.Location = new System.Drawing.Point(4, 6);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(105, 17);
			this.label2.TabIndex = 177;
			this.label2.Text = "Compiler Output";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.textBox2);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.textBox1);
			this.groupBox1.Controls.Add(this.listBox2);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.listBox1);
			this.groupBox1.Controls.Add(this.textBox3);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.btnSaveAsComponent);
			this.groupBox1.Controls.Add(this.listBox4);
			this.groupBox1.Location = new System.Drawing.Point(113, 4);
			this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
			this.groupBox1.Size = new System.Drawing.Size(623, 412);
			this.groupBox1.TabIndex = 134;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "groupBox1";
			this.groupBox1.Visible = false;
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(221, 202);
			this.textBox2.Margin = new System.Windows.Forms.Padding(4);
			this.textBox2.Multiline = true;
			this.textBox2.Name = "textBox2";
			this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBox2.Size = new System.Drawing.Size(259, 140);
			this.textBox2.TabIndex = 145;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label5.Location = new System.Drawing.Point(8, 31);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(54, 17);
			this.label5.TabIndex = 133;
			this.label5.Text = "codes_L";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(11, 52);
			this.textBox1.Margin = new System.Windows.Forms.Padding(4);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBox1.Size = new System.Drawing.Size(202, 144);
			this.textBox1.TabIndex = 132;
			// 
			// listBox2
			// 
			this.listBox2.FormattingEnabled = true;
			this.listBox2.ItemHeight = 17;
			this.listBox2.Location = new System.Drawing.Point(11, 236);
			this.listBox2.Margin = new System.Windows.Forms.Padding(4);
			this.listBox2.Name = "listBox2";
			this.listBox2.Size = new System.Drawing.Size(202, 106);
			this.listBox2.TabIndex = 115;
			this.listBox2.SelectedIndexChanged += new System.EventHandler(this.listBox2_SelectedIndexChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label4.Location = new System.Drawing.Point(8, 215);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(105, 17);
			this.label4.TabIndex = 116;
			this.label4.Text = "Components List";
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label1.Location = new System.Drawing.Point(396, -177);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(77, 17);
			this.label1.TabIndex = 144;
			this.label1.Text = "Return Data";
			// 
			// listBox1
			// 
			this.listBox1.BackColor = System.Drawing.Color.White;
			this.listBox1.ForeColor = System.Drawing.Color.Black;
			this.listBox1.FormattingEnabled = true;
			this.listBox1.ItemHeight = 17;
			this.listBox1.Location = new System.Drawing.Point(488, 52);
			this.listBox1.Margin = new System.Windows.Forms.Padding(4);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(127, 140);
			this.listBox1.TabIndex = 142;
			// 
			// textBox3
			// 
			this.textBox3.Location = new System.Drawing.Point(221, 52);
			this.textBox3.Margin = new System.Windows.Forms.Padding(4);
			this.textBox3.Multiline = true;
			this.textBox3.Name = "textBox3";
			this.textBox3.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBox3.Size = new System.Drawing.Size(259, 140);
			this.textBox3.TabIndex = 106;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label3.Location = new System.Drawing.Point(227, 31);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(109, 17);
			this.label3.TabIndex = 107;
			this.label3.Text = "Component code";
			// 
			// btnSaveAsComponent
			// 
			this.btnSaveAsComponent.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.btnSaveAsComponent.Location = new System.Drawing.Point(221, 350);
			this.btnSaveAsComponent.Margin = new System.Windows.Forms.Padding(4);
			this.btnSaveAsComponent.Name = "btnSaveAsComponent";
			this.btnSaveAsComponent.Size = new System.Drawing.Size(102, 36);
			this.btnSaveAsComponent.TabIndex = 117;
			this.btnSaveAsComponent.Text = "Save As Component";
			this.btnSaveAsComponent.UseVisualStyleBackColor = true;
			this.btnSaveAsComponent.Click += new System.EventHandler(this.btnSaveAsComponent_Click);
			// 
			// listBox4
			// 
			this.listBox4.FormattingEnabled = true;
			this.listBox4.ItemHeight = 17;
			this.listBox4.Location = new System.Drawing.Point(488, 202);
			this.listBox4.Margin = new System.Windows.Forms.Padding(4);
			this.listBox4.Name = "listBox4";
			this.listBox4.Size = new System.Drawing.Size(127, 140);
			this.listBox4.TabIndex = 143;
			// 
			// statusStrip
			// 
			this.statusStrip.BackColor = System.Drawing.SystemColors.Control;
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
			this.statusStrip.Location = new System.Drawing.Point(0, 719);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 17, 0);
			this.statusStrip.Size = new System.Drawing.Size(1339, 22);
			this.statusStrip.TabIndex = 55;
			this.statusStrip.Text = "statusStrip1";
			// 
			// statusLabel
			// 
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new System.Drawing.Size(1321, 17);
			this.statusLabel.Spring = true;
			this.statusLabel.Text = "File size:";
			this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// menuStrip
			// 
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mi_File,
            this.miTools,
            this.mi_Help,
            this.mi_Language});
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
			this.menuStrip.Size = new System.Drawing.Size(1339, 25);
			this.menuStrip.TabIndex = 173;
			this.menuStrip.Text = "menuStrip1";
			// 
			// mi_File
			// 
			this.mi_File.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miNew2,
            this.miOpen2,
            this.miOpenBuildFolder,
            this.separator10,
            this.miSave2,
            this.miSaveAs,
            this.miSaveOther,
            this.separator9,
            this.miImportProject,
            this.miCloseProject,
            this.separator11,
            this.miExit});
			this.mi_File.Name = "mi_File";
			this.mi_File.Size = new System.Drawing.Size(39, 21);
			this.mi_File.Text = "File";
			// 
			// miNew2
			// 
			this.miNew2.Name = "miNew2";
			this.miNew2.Size = new System.Drawing.Size(187, 22);
			this.miNew2.Text = "New";
			this.miNew2.Click += new System.EventHandler(this.mi_New_Click);
			// 
			// miOpen2
			// 
			this.miOpen2.Name = "miOpen2";
			this.miOpen2.Size = new System.Drawing.Size(187, 22);
			this.miOpen2.Text = "Open";
			this.miOpen2.Click += new System.EventHandler(this.mi_Open_Click);
			// 
			// miOpenBuildFolder
			// 
			this.miOpenBuildFolder.Name = "miOpenBuildFolder";
			this.miOpenBuildFolder.Size = new System.Drawing.Size(187, 22);
			this.miOpenBuildFolder.Text = "Open build folder";
			this.miOpenBuildFolder.Click += new System.EventHandler(this.mi_Open3_Click);
			// 
			// separator10
			// 
			this.separator10.Name = "separator10";
			this.separator10.Size = new System.Drawing.Size(184, 6);
			// 
			// miSave2
			// 
			this.miSave2.Name = "miSave2";
			this.miSave2.Size = new System.Drawing.Size(187, 22);
			this.miSave2.Text = "Save";
			this.miSave2.Click += new System.EventHandler(this.mi_Save_Click);
			// 
			// miSaveAs
			// 
			this.miSaveAs.Name = "miSaveAs";
			this.miSaveAs.Size = new System.Drawing.Size(187, 22);
			this.miSaveAs.Text = "Save as";
			this.miSaveAs.Click += new System.EventHandler(this.mi_SaveAs_Click);
			// 
			// miSaveOther
			// 
			this.miSaveOther.Name = "miSaveOther";
			this.miSaveOther.Size = new System.Drawing.Size(187, 22);
			this.miSaveOther.Text = "Save Other Version";
			this.miSaveOther.Click += new System.EventHandler(this.mi_SaveOther_Click);
			// 
			// separator9
			// 
			this.separator9.Name = "separator9";
			this.separator9.Size = new System.Drawing.Size(184, 6);
			// 
			// miImportProject
			// 
			this.miImportProject.Name = "miImportProject";
			this.miImportProject.Size = new System.Drawing.Size(187, 22);
			this.miImportProject.Text = "Import ...";
			this.miImportProject.Click += new System.EventHandler(this.miImport_Click);
			// 
			// miCloseProject
			// 
			this.miCloseProject.Name = "miCloseProject";
			this.miCloseProject.Size = new System.Drawing.Size(187, 22);
			this.miCloseProject.Text = "Close Project";
			this.miCloseProject.Click += new System.EventHandler(this.mi_CloseProject_Click);
			// 
			// separator11
			// 
			this.separator11.Name = "separator11";
			this.separator11.Size = new System.Drawing.Size(184, 6);
			// 
			// miExit
			// 
			this.miExit.Name = "miExit";
			this.miExit.Size = new System.Drawing.Size(187, 22);
			this.miExit.Text = "Exit";
			this.miExit.Click += new System.EventHandler(this.miExit_Click);
			// 
			// miTools
			// 
			this.miTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFontGenerator,
            this.miEyeDropper});
			this.miTools.Name = "miTools";
			this.miTools.Size = new System.Drawing.Size(51, 21);
			this.miTools.Text = "Tools";
			// 
			// miFontGenerator
			// 
			this.miFontGenerator.Name = "miFontGenerator";
			this.miFontGenerator.Size = new System.Drawing.Size(164, 22);
			this.miFontGenerator.Text = "Font Generator";
			this.miFontGenerator.Click += new System.EventHandler(this.miFontGenerator_Click);
			// 
			// miEyeDropper
			// 
			this.miEyeDropper.Name = "miEyeDropper";
			this.miEyeDropper.Size = new System.Drawing.Size(164, 22);
			this.miEyeDropper.Text = "EyeDropper";
			this.miEyeDropper.Click += new System.EventHandler(this.miEyeDropper_Click);
			// 
			// mi_Help
			// 
			this.mi_Help.Name = "mi_Help";
			this.mi_Help.Size = new System.Drawing.Size(47, 21);
			this.mi_Help.Text = "Help";
			this.mi_Help.Click += new System.EventHandler(this.mi_Help_Click);
			// 
			// mi_Language
			// 
			this.mi_Language.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mi_English,
            this.mi_Chinese});
			this.mi_Language.Name = "mi_Language";
			this.mi_Language.Size = new System.Drawing.Size(77, 21);
			this.mi_Language.Text = "Language";
			// 
			// mi_English
			// 
			this.mi_English.Name = "mi_English";
			this.mi_English.Size = new System.Drawing.Size(121, 22);
			this.mi_English.Text = "English";
			this.mi_English.Click += new System.EventHandler(this.miEnglish_Click);
			// 
			// mi_Chinese
			// 
			this.mi_Chinese.Name = "mi_Chinese";
			this.mi_Chinese.Size = new System.Drawing.Size(121, 22);
			this.mi_Chinese.Text = "Chinese";
			this.mi_Chinese.Click += new System.EventHandler(this.miChinese_Click);
			// 
			// toolBar
			// 
			this.toolBar.BackColor = System.Drawing.SystemColors.Control;
			this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mi_Open,
            this.mi_New,
            this.mi_Save,
            this.toolStripSeparator1,
            this.toolStripSeparator2,
            this.mi_Compile,
            this.mi_Debug,
            this.mi_Upload,
            this.toolStripSeparator4,
            this.toolStripSeparator3,
            this.mi_AddComponent,
            this.mi_DeleteComponent,
            this.toolStripSeparator5,
            this.toolStripSeparator6,
            this.mi_Copy,
            this.mi_Paste,
            this.toolStripSeparator7,
            this.toolStripSeparator8,
            this.mi_Resolution,
            this.mi_ID,
            this.mi_XY});
			this.toolBar.Location = new System.Drawing.Point(0, 25);
			this.toolBar.Name = "toolBar";
			this.toolBar.Size = new System.Drawing.Size(1339, 25);
			this.toolBar.TabIndex = 174;
			this.toolBar.Text = "toolStrip1";
			// 
			// mi_Open
			// 
			this.mi_Open.Image = ((System.Drawing.Image)(resources.GetObject("mi_Open.Image")));
			this.mi_Open.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.mi_Open.Name = "mi_Open";
			this.mi_Open.Size = new System.Drawing.Size(60, 22);
			this.mi_Open.Text = "Open";
			this.mi_Open.Click += new System.EventHandler(this.mi_Open_Click);
			// 
			// mi_New
			// 
			this.mi_New.Image = ((System.Drawing.Image)(resources.GetObject("mi_New.Image")));
			this.mi_New.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.mi_New.Name = "mi_New";
			this.mi_New.Size = new System.Drawing.Size(54, 22);
			this.mi_New.Text = "New";
			this.mi_New.Click += new System.EventHandler(this.mi_New_Click);
			// 
			// mi_Save
			// 
			this.mi_Save.Image = ((System.Drawing.Image)(resources.GetObject("mi_Save.Image")));
			this.mi_Save.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.mi_Save.Name = "mi_Save";
			this.mi_Save.Size = new System.Drawing.Size(55, 22);
			this.mi_Save.Text = "Save";
			this.mi_Save.Click += new System.EventHandler(this.mi_Save_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// mi_Compile
			// 
			this.mi_Compile.Image = ((System.Drawing.Image)(resources.GetObject("mi_Compile.Image")));
			this.mi_Compile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.mi_Compile.Name = "mi_Compile";
			this.mi_Compile.Size = new System.Drawing.Size(76, 22);
			this.mi_Compile.Text = "Compile";
			this.mi_Compile.Click += new System.EventHandler(this.mi_Compile_Click);
			// 
			// mi_Debug
			// 
			this.mi_Debug.Image = ((System.Drawing.Image)(resources.GetObject("mi_Debug.Image")));
			this.mi_Debug.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.mi_Debug.Name = "mi_Debug";
			this.mi_Debug.Size = new System.Drawing.Size(67, 22);
			this.mi_Debug.Text = "Debug";
			this.mi_Debug.Click += new System.EventHandler(this.mi_Debug_Click);
			// 
			// mi_Upload
			// 
			this.mi_Upload.Image = ((System.Drawing.Image)(resources.GetObject("mi_Upload.Image")));
			this.mi_Upload.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.mi_Upload.Name = "mi_Upload";
			this.mi_Upload.Size = new System.Drawing.Size(71, 22);
			this.mi_Upload.Text = "Upload";
			this.mi_Upload.Click += new System.EventHandler(this.mi_Upload_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// mi_AddComponent
			// 
			this.mi_AddComponent.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mi_Text,
            this.mi_Number,
            this.mi_Button,
            this.mi_ProgressBar,
            this.mi_Picture,
            this.mi_CropImage,
            this.mi_Hotspot,
            this.mi_Gauges,
            this.mi_Waveform,
            this.mi_Slider,
            this.mi_Timer,
            this.mi_Variable,
            this.mi_DualButton,
            this.mi_RadioBox,
            this.mi_Checkbox});
			this.mi_AddComponent.Image = ((System.Drawing.Image)(resources.GetObject("mi_AddComponent.Image")));
			this.mi_AddComponent.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.mi_AddComponent.Name = "mi_AddComponent";
			this.mi_AddComponent.Size = new System.Drawing.Size(133, 22);
			this.mi_AddComponent.Text = "Add Component";
			// 
			// mi_Text
			// 
			this.mi_Text.Name = "mi_Text";
			this.mi_Text.Size = new System.Drawing.Size(224, 22);
			this.mi_Text.Text = "Text";
			this.mi_Text.Click += new System.EventHandler(this.mi_Text_Click);
			// 
			// mi_Number
			// 
			this.mi_Number.Name = "mi_Number";
			this.mi_Number.Size = new System.Drawing.Size(224, 22);
			this.mi_Number.Text = "Number";
			this.mi_Number.Click += new System.EventHandler(this.mi_Number_Click);
			// 
			// mi_Button
			// 
			this.mi_Button.Name = "mi_Button";
			this.mi_Button.Size = new System.Drawing.Size(224, 22);
			this.mi_Button.Text = "Button";
			this.mi_Button.Click += new System.EventHandler(this.mi_Button_Click);
			// 
			// mi_ProgressBar
			// 
			this.mi_ProgressBar.Name = "mi_ProgressBar";
			this.mi_ProgressBar.Size = new System.Drawing.Size(224, 22);
			this.mi_ProgressBar.Text = "Progress bar";
			this.mi_ProgressBar.Click += new System.EventHandler(this.mi_ProgressBar_Click);
			// 
			// mi_Picture
			// 
			this.mi_Picture.Name = "mi_Picture";
			this.mi_Picture.Size = new System.Drawing.Size(224, 22);
			this.mi_Picture.Text = "Picture";
			this.mi_Picture.Click += new System.EventHandler(this.mi_Picture_Click);
			// 
			// mi_CropImage
			// 
			this.mi_CropImage.Name = "mi_CropImage";
			this.mi_CropImage.Size = new System.Drawing.Size(224, 22);
			this.mi_CropImage.Text = "Crop Image";
			this.mi_CropImage.Click += new System.EventHandler(this.mi_CropImage_Click);
			// 
			// mi_Hotspot
			// 
			this.mi_Hotspot.Name = "mi_Hotspot";
			this.mi_Hotspot.Size = new System.Drawing.Size(224, 22);
			this.mi_Hotspot.Text = "Hotspot";
			this.mi_Hotspot.Click += new System.EventHandler(this.mi_Hotspot_Click);
			// 
			// mi_Gauges
			// 
			this.mi_Gauges.Name = "mi_Gauges";
			this.mi_Gauges.Size = new System.Drawing.Size(224, 22);
			this.mi_Gauges.Text = "Gauges";
			this.mi_Gauges.Click += new System.EventHandler(this.mi_Gauges_Click);
			// 
			// mi_Waveform
			// 
			this.mi_Waveform.Name = "mi_Waveform";
			this.mi_Waveform.Size = new System.Drawing.Size(224, 22);
			this.mi_Waveform.Text = "Waveform";
			this.mi_Waveform.Click += new System.EventHandler(this.mi_Waveform_Click);
			// 
			// mi_Slider
			// 
			this.mi_Slider.Name = "mi_Slider";
			this.mi_Slider.Size = new System.Drawing.Size(224, 22);
			this.mi_Slider.Text = "Slider";
			this.mi_Slider.Click += new System.EventHandler(this.mi_Slider_Click);
			// 
			// mi_Timer
			// 
			this.mi_Timer.Name = "mi_Timer";
			this.mi_Timer.Size = new System.Drawing.Size(224, 22);
			this.mi_Timer.Text = "Timer";
			this.mi_Timer.Click += new System.EventHandler(this.mi_Timer_Click);
			// 
			// mi_Variable
			// 
			this.mi_Variable.Name = "mi_Variable";
			this.mi_Variable.Size = new System.Drawing.Size(224, 22);
			this.mi_Variable.Text = "Variable";
			this.mi_Variable.Click += new System.EventHandler(this.mi_Variable_Click);
			// 
			// mi_DualButton
			// 
			this.mi_DualButton.Name = "mi_DualButton";
			this.mi_DualButton.Size = new System.Drawing.Size(224, 22);
			this.mi_DualButton.Text = "Dual-state button";
			this.mi_DualButton.Click += new System.EventHandler(this.mi_DualButton_Click);
			// 
			// mi_RadioBox
			// 
			this.mi_RadioBox.Name = "mi_RadioBox";
			this.mi_RadioBox.Size = new System.Drawing.Size(224, 22);
			this.mi_RadioBox.Text = "Radio box (coming soon)";
			this.mi_RadioBox.Click += new System.EventHandler(this.mi_RadioBox_Click);
			// 
			// mi_Checkbox
			// 
			this.mi_Checkbox.Name = "mi_Checkbox";
			this.mi_Checkbox.Size = new System.Drawing.Size(224, 22);
			this.mi_Checkbox.Text = "Checkbox (coming soon)";
			this.mi_Checkbox.Click += new System.EventHandler(this.mi_Checkbox_Click);
			// 
			// mi_DeleteComponent
			// 
			this.mi_DeleteComponent.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mi_DeleteSelected,
            this.mi_DeleteAll});
			this.mi_DeleteComponent.Image = ((System.Drawing.Image)(resources.GetObject("mi_DeleteComponent.Image")));
			this.mi_DeleteComponent.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.mi_DeleteComponent.Name = "mi_DeleteComponent";
			this.mi_DeleteComponent.Size = new System.Drawing.Size(146, 22);
			this.mi_DeleteComponent.Text = "Delete Component";
			// 
			// mi_DeleteSelected
			// 
			this.mi_DeleteSelected.Name = "mi_DeleteSelected";
			this.mi_DeleteSelected.Size = new System.Drawing.Size(238, 22);
			this.mi_DeleteSelected.Text = "Delete Selected Component";
			this.mi_DeleteSelected.Click += new System.EventHandler(this.mi_DeleteSelected_Click);
			// 
			// mi_DeleteAll
			// 
			this.mi_DeleteAll.Name = "mi_DeleteAll";
			this.mi_DeleteAll.Size = new System.Drawing.Size(238, 22);
			this.mi_DeleteAll.Text = "Delete All Components";
			this.mi_DeleteAll.Click += new System.EventHandler(this.mi_DeleteAll_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripSeparator6
			// 
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
			// 
			// mi_Copy
			// 
			this.mi_Copy.Image = ((System.Drawing.Image)(resources.GetObject("mi_Copy.Image")));
			this.mi_Copy.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.mi_Copy.Name = "mi_Copy";
			this.mi_Copy.Size = new System.Drawing.Size(58, 22);
			this.mi_Copy.Text = "Copy";
			this.mi_Copy.Click += new System.EventHandler(this.mi_Copy_Click);
			// 
			// mi_Paste
			// 
			this.mi_Paste.Image = ((System.Drawing.Image)(resources.GetObject("mi_Paste.Image")));
			this.mi_Paste.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.mi_Paste.Name = "mi_Paste";
			this.mi_Paste.Size = new System.Drawing.Size(59, 22);
			this.mi_Paste.Text = "Paste";
			this.mi_Paste.Click += new System.EventHandler(this.mi_Paste_Click);
			// 
			// toolStripSeparator7
			// 
			this.toolStripSeparator7.Name = "toolStripSeparator7";
			this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripSeparator8
			// 
			this.toolStripSeparator8.Name = "toolStripSeparator8";
			this.toolStripSeparator8.Size = new System.Drawing.Size(6, 25);
			// 
			// mi_Resolution
			// 
			this.mi_Resolution.Image = ((System.Drawing.Image)(resources.GetObject("mi_Resolution.Image")));
			this.mi_Resolution.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.mi_Resolution.Name = "mi_Resolution";
			this.mi_Resolution.Size = new System.Drawing.Size(89, 22);
			this.mi_Resolution.Text = "Resolution";
			this.mi_Resolution.Click += new System.EventHandler(this.mi_Resolution_Click);
			// 
			// mi_ID
			// 
			this.mi_ID.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.mi_ID.Image = ((System.Drawing.Image)(resources.GetObject("mi_ID.Image")));
			this.mi_ID.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.mi_ID.Name = "mi_ID";
			this.mi_ID.Size = new System.Drawing.Size(23, 22);
			this.mi_ID.Text = "ID";
			this.mi_ID.Click += new System.EventHandler(this.mi_ID_Click);
			// 
			// mi_XY
			// 
			this.mi_XY.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.mi_XY.Image = ((System.Drawing.Image)(resources.GetObject("mi_XY.Image")));
			this.mi_XY.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.mi_XY.Name = "mi_XY";
			this.mi_XY.Size = new System.Drawing.Size(27, 22);
			this.mi_XY.Text = "XY";
			this.mi_XY.Click += new System.EventHandler(this.mi_XY_Click);
			// 
			// panelLeft
			// 
			this.panelLeft.Controls.Add(this.panelPicAdmin);
			this.panelLeft.Controls.Add(this.splitter4);
			this.panelLeft.Controls.Add(this.panelFontAdmin);
			this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
			this.panelLeft.Location = new System.Drawing.Point(0, 50);
			this.panelLeft.Margin = new System.Windows.Forms.Padding(4);
			this.panelLeft.Name = "panelLeft";
			this.panelLeft.Size = new System.Drawing.Size(256, 669);
			this.panelLeft.TabIndex = 177;
			// 
			// panelPicAdmin
			// 
			this.panelPicAdmin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelPicAdmin.Controls.Add(this.picAdmin);
			this.panelPicAdmin.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelPicAdmin.Location = new System.Drawing.Point(0, 0);
			this.panelPicAdmin.Margin = new System.Windows.Forms.Padding(4);
			this.panelPicAdmin.Name = "panelPicAdmin";
			this.panelPicAdmin.Size = new System.Drawing.Size(256, 416);
			this.panelPicAdmin.TabIndex = 2;
			this.panelPicAdmin.Resize += new System.EventHandler(this.panelPicAdmin_Resize);
			// 
			// picAdmin
			// 
			this.picAdmin.BackColor = System.Drawing.SystemColors.Control;
			this.picAdmin.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.picAdmin.Location = new System.Drawing.Point(13, 15);
			this.picAdmin.Margin = new System.Windows.Forms.Padding(4);
			this.picAdmin.Name = "picAdmin";
			this.picAdmin.Size = new System.Drawing.Size(228, 353);
			this.picAdmin.TabIndex = 112;
			this.picAdmin.PicSelect += new System.EventHandler(this.picadmin1_picselect);
			this.picAdmin.PicUpdate += new System.EventHandler(this.picadmin1_picupdate);
			// 
			// splitter4
			// 
			this.splitter4.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter4.Location = new System.Drawing.Point(0, 416);
			this.splitter4.Margin = new System.Windows.Forms.Padding(4);
			this.splitter4.Name = "splitter4";
			this.splitter4.Size = new System.Drawing.Size(256, 3);
			this.splitter4.TabIndex = 1;
			this.splitter4.TabStop = false;
			// 
			// panelFontAdmin
			// 
			this.panelFontAdmin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelFontAdmin.Controls.Add(this.fontAdmin);
			this.panelFontAdmin.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelFontAdmin.Location = new System.Drawing.Point(0, 419);
			this.panelFontAdmin.Margin = new System.Windows.Forms.Padding(4);
			this.panelFontAdmin.Name = "panelFontAdmin";
			this.panelFontAdmin.Size = new System.Drawing.Size(256, 250);
			this.panelFontAdmin.TabIndex = 0;
			this.panelFontAdmin.Resize += new System.EventHandler(this.panelFontAdmin_Resize);
			// 
			// fontAdmin
			// 
			this.fontAdmin.BackColor = System.Drawing.SystemColors.Control;
			this.fontAdmin.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.fontAdmin.Location = new System.Drawing.Point(13, 8);
			this.fontAdmin.Margin = new System.Windows.Forms.Padding(4);
			this.fontAdmin.Name = "fontAdmin";
			this.fontAdmin.Size = new System.Drawing.Size(228, 193);
			this.fontAdmin.TabIndex = 123;
			this.fontAdmin.FontUpdate += new System.EventHandler(this.fontAdmin_FontUpdate);
			// 
			// panelRight
			// 
			this.panelRight.Controls.Add(this.panelPageAdmin);
			this.panelRight.Controls.Add(this.splitter6);
			this.panelRight.Controls.Add(this.panelObjAttrs);
			this.panelRight.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelRight.Location = new System.Drawing.Point(1083, 50);
			this.panelRight.Margin = new System.Windows.Forms.Padding(4);
			this.panelRight.Name = "panelRight";
			this.panelRight.Size = new System.Drawing.Size(256, 669);
			this.panelRight.TabIndex = 180;
			// 
			// panelPageAdmin
			// 
			this.panelPageAdmin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelPageAdmin.Controls.Add(this.pageAdmin);
			this.panelPageAdmin.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelPageAdmin.Location = new System.Drawing.Point(0, 0);
			this.panelPageAdmin.Margin = new System.Windows.Forms.Padding(4);
			this.panelPageAdmin.Name = "panelPageAdmin";
			this.panelPageAdmin.Size = new System.Drawing.Size(256, 266);
			this.panelPageAdmin.TabIndex = 148;
			this.panelPageAdmin.Resize += new System.EventHandler(this.panelPageAdmin_Resize);
			// 
			// pageAdmin
			// 
			this.pageAdmin.BackColor = System.Drawing.SystemColors.Control;
			this.pageAdmin.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.pageAdmin.Location = new System.Drawing.Point(8, 15);
			this.pageAdmin.Margin = new System.Windows.Forms.Padding(4);
			this.pageAdmin.Name = "pageAdmin";
			this.pageAdmin.Size = new System.Drawing.Size(234, 217);
			this.pageAdmin.TabIndex = 144;
			this.pageAdmin.PageChange += new System.EventHandler(this.pageAdmin_PageChange);
			this.pageAdmin.SelectEnter += new System.EventHandler(this.pageAdmin_SelectEnter);
			// 
			// splitter6
			// 
			this.splitter6.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter6.Location = new System.Drawing.Point(0, 266);
			this.splitter6.Margin = new System.Windows.Forms.Padding(4);
			this.splitter6.Name = "splitter6";
			this.splitter6.Size = new System.Drawing.Size(256, 3);
			this.splitter6.TabIndex = 147;
			this.splitter6.TabStop = false;
			// 
			// panelObjAttrs
			// 
			this.panelObjAttrs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelObjAttrs.Controls.Add(this.m_attributeEdit);
			this.panelObjAttrs.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelObjAttrs.Location = new System.Drawing.Point(0, 269);
			this.panelObjAttrs.Margin = new System.Windows.Forms.Padding(4);
			this.panelObjAttrs.Name = "panelObjAttrs";
			this.panelObjAttrs.Size = new System.Drawing.Size(256, 400);
			this.panelObjAttrs.TabIndex = 146;
			this.panelObjAttrs.Resize += new System.EventHandler(this.panelObjAttrs_Resize);
			// 
			// m_attributeEdit
			// 
			this.m_attributeEdit.BackColor = System.Drawing.SystemColors.Control;
			this.m_attributeEdit.Location = new System.Drawing.Point(8, 8);
			this.m_attributeEdit.Margin = new System.Windows.Forms.Padding(4);
			this.m_attributeEdit.Name = "m_attributeEdit";
			this.m_attributeEdit.Size = new System.Drawing.Size(234, 339);
			this.m_attributeEdit.TabIndex = 113;
			this.m_attributeEdit.ObjectAttach += new System.EventHandler(this.attributeEdit_ObjectAttach);
			this.m_attributeEdit.ObjectPosXY += new System.EventHandler(this.attributeEdit_ObjectPosXY);
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(256, 50);
			this.splitter1.Margin = new System.Windows.Forms.Padding(4);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 669);
			this.splitter1.TabIndex = 181;
			this.splitter1.TabStop = false;
			// 
			// splitter2
			// 
			this.splitter2.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter2.Location = new System.Drawing.Point(1080, 50);
			this.splitter2.Margin = new System.Windows.Forms.Padding(4);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = new System.Drawing.Size(3, 669);
			this.splitter2.TabIndex = 182;
			this.splitter2.TabStop = false;
			// 
			// panelBottom
			// 
			this.panelBottom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelBottom.Controls.Add(this.m_compiler);
			this.panelBottom.Controls.Add(this.tbCompilerOutput);
			this.panelBottom.Controls.Add(this.label2);
			this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelBottom.Location = new System.Drawing.Point(259, 533);
			this.panelBottom.Margin = new System.Windows.Forms.Padding(4);
			this.panelBottom.Name = "panelBottom";
			this.panelBottom.Size = new System.Drawing.Size(821, 186);
			this.panelBottom.TabIndex = 183;
			this.panelBottom.Paint += new System.Windows.Forms.PaintEventHandler(this.panelBottom_Paint);
			// 
			// m_compiler
			// 
			this.m_compiler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_compiler.BackColor = System.Drawing.Color.White;
			this.m_compiler.Location = new System.Drawing.Point(248, 31);
			this.m_compiler.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.m_compiler.Name = "m_compiler";
			this.m_compiler.Size = new System.Drawing.Size(563, 149);
			this.m_compiler.TabIndex = 178;
			this.m_compiler.ChangeAttribute += new System.EventHandler(this.objAttCompiler_ChangeAttribute);
			// 
			// panel9
			// 
			this.panel9.AutoScroll = true;
			this.panel9.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel9.Location = new System.Drawing.Point(259, 480);
			this.panel9.Margin = new System.Windows.Forms.Padding(4);
			this.panel9.Name = "panel9";
			this.panel9.Size = new System.Drawing.Size(821, 50);
			this.panel9.TabIndex = 177;
			this.panel9.Visible = false;
			// 
			// splitter3
			// 
			this.splitter3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter3.Location = new System.Drawing.Point(259, 530);
			this.splitter3.Margin = new System.Windows.Forms.Padding(4);
			this.splitter3.Name = "splitter3";
			this.splitter3.Size = new System.Drawing.Size(821, 3);
			this.splitter3.TabIndex = 184;
			this.splitter3.TabStop = false;
			this.splitter3.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitter3_SplitterMoved);
			// 
			// panelView
			// 
			this.panelView.AllowDrop = true;
			this.panelView.AutoScroll = true;
			this.panelView.BackColor = System.Drawing.Color.Gray;
			this.panelView.Controls.Add(this.groupBox1);
			this.panelView.Controls.Add(this.runScreen);
			this.panelView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelView.Location = new System.Drawing.Point(259, 50);
			this.panelView.Margin = new System.Windows.Forms.Padding(4);
			this.panelView.Name = "panelView";
			this.panelView.Size = new System.Drawing.Size(821, 430);
			this.panelView.TabIndex = 185;
			this.panelView.DragDrop += new System.Windows.Forms.DragEventHandler(this.panelView_DragDrop);
			this.panelView.DragEnter += new System.Windows.Forms.DragEventHandler(this.panelView_DragEnter);
			this.panelView.Paint += new System.Windows.Forms.PaintEventHandler(this.panelView_Paint);
			this.panelView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelView_MouseUp);
			this.panelView.Resize += new System.EventHandler(this.panelView_Resize);
			// 
			// runScreen
			// 
			this.runScreen.BackColor = System.Drawing.Color.Black;
			this.runScreen.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.runScreen.Location = new System.Drawing.Point(5, 4);
			this.runScreen.Margin = new System.Windows.Forms.Padding(4);
			this.runScreen.Name = "runScreen";
			this.runScreen.Size = new System.Drawing.Size(100, 100);
			this.runScreen.TabIndex = 175;
			this.runScreen.Visible = false;
			this.runScreen.ObjChange += new System.EventHandler(this.runScreen_ObjChange);
			this.runScreen.ObjMouseUp += new System.EventHandler(this.runScreen_ObjMouseUp);
			this.runScreen.SendByte += new System.EventHandler(this.runScreen_SendByte);
			this.runScreen.Resize += new System.EventHandler(this.runScreen_Resize);
			// 
			// main
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(1339, 741);
			this.Controls.Add(this.panelView);
			this.Controls.Add(this.panel9);
			this.Controls.Add(this.splitter3);
			this.Controls.Add(this.panelBottom);
			this.Controls.Add(this.splitter2);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.panelLeft);
			this.Controls.Add(this.panelRight);
			this.Controls.Add(this.toolBar);
			this.Controls.Add(this.menuStrip);
			this.Controls.Add(this.statusStrip);
			this.DoubleBuffered = true;
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.MainMenuStrip = this.menuStrip;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "main";
			this.Text = "Nextion Editor";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.main_FormClosing);
			this.Load += new System.EventHandler(this.main_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			this.toolBar.ResumeLayout(false);
			this.toolBar.PerformLayout();
			this.panelLeft.ResumeLayout(false);
			this.panelPicAdmin.ResumeLayout(false);
			this.panelFontAdmin.ResumeLayout(false);
			this.panelRight.ResumeLayout(false);
			this.panelPageAdmin.ResumeLayout(false);
			this.panelObjAttrs.ResumeLayout(false);
			this.panelBottom.ResumeLayout(false);
			this.panelBottom.PerformLayout();
			this.panelView.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		#region setControlsTop
		private void setControlsTop()
		{
			try
			{
				for (int i = 0; i < panel9.Controls.Count; i++)
					if (panel9.Controls[i].Top != 0)
						panel9.Controls[i].Top = 0;
			}
			catch { }
		}
		#endregion

		private bool m_app_FileSaveEvent()
		{
			bool flag = false;
			if (m_app != null)
			{
				runScreen.PauseScreen();
				m_compiler.SaveCodes();
				flag = m_app.SaveFile(HmiOptions.RunFilePath, false, null);
				runScreen.StartFile();
			}
			return flag;
		}

		private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			List<byte[]> bts = new List<byte[]>();
			m_obj = m_page.HmiObjects[int.Parse(listBox2.SelectedItem.ToString())];
			textBox3.Text = Utility.ToStrings(m_obj.GetCodes());
			m_obj.CompileCodes(bts);
			textBox1.Text = Utility.ToStrings(bts);
			m_compiler.RefreshObject(m_app, m_page, m_page.HmiObjects[int.Parse(listBox2.SelectedItem.ToString())]);
		}

		private void loadObj(HmiObject obj)
		{
			HmiObjectEdit objEdit = new HmiObjectEdit();
			try
			{
				objEdit.HmiObject = obj;
				objEdit.Location = new Point(m_objXPos, 5);
				objEdit.Width = 40;
				objEdit.Height = 40;
				objEdit.IsMove = false;
				if (objEdit.Width < 3)
					objEdit.Width = 3;

				if (base.Height < 3)
					objEdit.Height = 3;

				objEdit.BackColor = (obj.Attributes[0].Data[0] == HmiObjType.PAGE)
										? Color.FromArgb(0, 72, 149, 253)
										: Color.FromArgb(50, 72, 149, 253);
				objEdit.IsShowName = m_app.IsShowName;
				objEdit.HmiRunScreen = runScreen;
				objEdit.ObjMouseUp += new EventHandler(runScreen.T_objMouseUp);
				objEdit.ObjChange += new EventHandler(runScreen.T_ObjChange);
				objEdit.SetApp(m_app);
				objEdit.BackgroundImageLayout = ImageLayout.None;

				panel9.Controls.Add(objEdit);

				objEdit.MakeBackground();
				objEdit.BringToFront();
				objEdit.Visible = true;
				objEdit.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;

				if (m_objXPos == 0)
					panel9.Visible = true;

				m_objXPos += 50;
				setControlsTop();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error occurred during loading components ".Translate() + ex.Message);
			}
		}
		private void objAttCompiler_ChangeAttribute(object sender, EventArgs e)
		{
			m_app.ChangeApplication(true);
		}

		private void attributeEdit_ObjectAttach(object sender, EventArgs e)
		{
			if (m_objEdit != null)
			{
				m_objEdit.MakeBackground();
				m_app.ChangeApplication(true);
			}
		}

		private void attributeEdit_ObjectPosXY(object sender, EventArgs e)
		{
			if (m_objEdit != null)
			{
				m_objEdit.SetWidthXY();
				m_app.ChangeApplication(true);
			}
		}

		private void pageAdmin_PageChange(object sender, EventArgs e)
		{
			m_app.ChangeApplication(true);
		}

		private void pageAdmin_SelectEnter(object sender, EventArgs e)
		{
			int num = (int)sender;
			if (num != 0xffff)
				m_page = m_app.HmiPages[num];
			else
				m_page = null;
			m_obj = null;
			refreshPage();
		}

		private void panelFontAdmin_Resize(object sender, EventArgs e)
		{
			fontAdmin.SetSizeToParent();
		}

		private void panelPicAdmin_Resize(object sender, EventArgs e)
		{
			picAdmin.SetSizeToParent();
		}

		private void panelBottom_Paint(object sender, PaintEventArgs e)
		{
			// Utility.DrawThisLine(panelBottom, Color.FromArgb(0x33, 0x99, 0xff), 1);
		}

		private void panelView_Resize(object sender, EventArgs e)
		{
			try
			{
				// panelTabs.Height = (m_objXPos == 0) ? panelTabs.Height : (panelTabs.Height - panel9.Height);
				// tabs.Width = panelTabs.Width;

				// panel9.Location = new Point(0, panelTabs.Height - panel9.Height);
				// panel9.Width = panelTabs.Width;

				// setControlsTop();
				resizeForm();
			}
			catch { }
		}

		private void panelObjAttrs_Resize(object sender, EventArgs e)
		{
			m_attributeEdit.SetSizeToParent();
		}

		private void panelPageAdmin_Resize(object sender, EventArgs e)
		{
			pageAdmin.SetSizeToParent();
		}

		private void panel9_Resize(object sender, EventArgs e)
		{
			setControlsTop();
		}

		private void picadmin1_picselect(object sender, EventArgs e)
		{
		}

		private void picadmin1_picupdate(object sender, EventArgs e)
		{
			m_app.FileSave();
			m_app.ChangeApplication(true);
		}

		private void showUsageSpace()
		{
			statusLabel.Text = "ROM Space Usage:".Translate() + m_app.GetAllDataSize().ToString("0.000" + "K".Translate());
		}

		private void refreshPage()
		{
			m_obj = null;
			if (runScreen.HmiObjectEdit != null)
			{
				runScreen.HmiObjectEdit.Dispose();
				runScreen.HmiObjectEdit = null;
			}
			if (m_page != null)
			{
				textBox2.Text = Utility.ToStrings(m_page.Codes);
				if (!runScreen.Visible)
				{
					runScreen.GuiInit(HmiOptions.RunFilePath, m_app, true);
					runScreen.Visible = true;
				}
				m_objEdit = null;
				listBox2.Items.Clear();
				for (int i = 0; i < m_page.HmiObjects.Count; i++)
					listBox2.Items.Add(i.ToString());
			}

			runScreen.RefreshPageEdit(m_page);
			m_compiler.RefreshObject(m_app, m_page, m_obj);
			m_attributeEdit.RefreshObject(m_app, m_page, m_obj);
			mi_Copy.Enabled = false;

			refreshTimerVar();
		}

		private void refreshTimerVar()
		{
			try
			{
				m_objXPos = 0;
				for (int idx = 0; idx < panel9.Controls.Count; idx++)
					if (panel9.Controls[idx] is HmiObjectEdit)
					{
						panel9.Controls[idx].Dispose();
						--idx;
					}
				panel9.Visible = false;
				if (m_app != null && m_page != null)
				{
					for (int idx = 1; idx < m_page.HmiObjects.Count; idx++)
						if (m_page.HmiObjects[idx].Attributes[0].Data[0] == HmiObjType.TIMER
						 || m_page.HmiObjects[idx].Attributes[0].Data[0] == HmiObjType.VAR
							)
						{
							loadObj(m_page.HmiObjects[idx]);
						}
				}
 			}
			catch (Exception ex)
			{
				MessageBox.Show("load formobj error:" + ex.Message);
			}
		}

		private void resizeForm()
		{
			try
			{
				int left = (panelView.Width - runScreen.Width) / 2;
				int top = (panelView.Height - runScreen.Height) / 2;
				if (left < 25)
					left = 25;
				if (top < 25)
					top = 25;
				if (runScreen.Left != left || runScreen.Top != top)
				{
					runScreen.SuspendLayout();
					runScreen.Left = left;
					runScreen.Top = top;

					runScreen.ResumeLayout(false);
					runScreen.PerformLayout();
				}
			}
			catch { }
		}

		private void runScreen_ObjChange(object sender, EventArgs e)
		{
			m_app.ChangeApplication(true);
		}

		private void runScreen_ObjMouseUp(object sender, EventArgs e)
		{
			List<byte[]> bts = new List<byte[]>();
			HmiObjectEdit objedit = (HmiObjectEdit)sender;
			if ((m_app != null) && (m_page != null))
			{
				if (objedit != null)
				{
					m_objEdit = objedit;
					m_obj = m_objEdit.HmiObject;
					textBox3.Text = Utility.ToStrings(m_obj.GetCodes());
					m_obj.CompileCodes(bts);
					textBox1.Text = Utility.ToStrings(bts);
				}
				else
					m_obj = m_page.HmiObjects[0];
				
				m_compiler.RefreshObject(m_app, m_page, m_obj);
				m_attributeEdit.RefreshObject(m_app, m_page, m_obj);
				mi_Copy.Enabled = true;
			}
		}

		private void runScreen_Resize(object sender, EventArgs e)
		{
			resizeForm();
		}

		private void runScreen_SendByte(object sender, EventArgs e)
		{
			int num = (int)sender;
			string str = Convert.ToString(num, 0x10);
			if (str.Length == 1)
				str = "0" + str;
			m_comData = m_comData + "0x" + str + " ";
			if (str == "0a")
			{
				listBox1.Items.Add(m_comData.Trim());
				listBox1.SelectedIndex = listBox1.Items.Count - 1;
				m_comData = "";
			}
		}

		private void setCompileHeight(int state)
		{
			if (state == 0 && panelBottom.Tag != null && Utility.GetInt(panelBottom.Tag.ToString()) > 0)
			{
				panelBottom.Height = Utility.GetInt(panelBottom.Tag.ToString());
				panelBottom.Tag = null;
				panelRight.Width = Utility.GetInt(panelRight.Tag.ToString());
				panelRight.Tag = null;
				panelLeft.Width = Utility.GetInt(panelLeft.Tag.ToString());
				panelLeft.Tag = null;
				toolBar.Enabled = true;
				panelView.Refresh();
			}
			if (state == 1 && (panelBottom.Tag == null || Utility.GetInt(panelBottom.Tag.ToString()) == 0))
			{
				panelBottom.Tag = panelBottom.Height.ToString();
				panelBottom.Height = 0;
				panelRight.Tag = panelRight.Width.ToString();
				panelRight.Width = 0;
				panelLeft.Tag = panelLeft.Width.ToString();
				panelLeft.Width = 0;
				toolBar.Enabled = false;
				panelView.Refresh();
			}
		}

		private void splitter3_SplitterMoved(object sender, SplitterEventArgs e)
		{
			panelBottom.Tag = null;
		}
		private void panelView_DragDrop(object sender, DragEventArgs e)
		{
			Array data = (Array)e.Data.GetData(DataFormats.FileDrop);
			for (int i = 0; i < data.Length; i++)
			{
				string path = data.GetValue(i).ToString();
				if (File.Exists(path)
				 && Path.GetExtension(path).ToLowerInvariant().EndsWith("hmi")
					)
				{
					fileOperation("open", path);
					break;
				}
			}
		}

		private void panelView_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Move;
			else
				e.Effect = DragDropEffects.None;
		}

		private void panelView_MouseUp(object sender, MouseEventArgs e)
		{
			if (m_app != null)
			{
				if (m_objEdit != null)
					m_objEdit.SetSelected(false);

				m_objEdit = null;
				m_obj = null;
				m_compiler.RefreshObject(m_app, m_page, m_obj);
				m_attributeEdit.RefreshObject(m_app, m_page, m_obj);
				mi_Copy.Enabled = false;
			}
		}

		private void panelView_Paint(object sender, PaintEventArgs e)
		{
			try
			{
				int num = 7;
				Pen pen = new Pen(Color.Yellow, 1f);
				Graphics graphics = panelView.CreateGraphics();
				graphics.Clear(panelView.BackColor);

				if (runScreen.Visible && m_showXY)
				{
					panelView.SuspendLayout();
					graphics.DrawString(
						"(0,0)",
						new Font(Encoding.Default.EncodingName, 9f),
						new SolidBrush(Color.Yellow), (PointF)new Point((runScreen.Left - num) - 17, (runScreen.Top - num) - 15)
						);
					graphics.DrawString(
						"X",
						new Font(Encoding.Default.EncodingName, 9f),
						new SolidBrush(Color.Yellow), (PointF)new Point((runScreen.Left + (runScreen.Width / 2)) - 5, (runScreen.Top - num) - 17)
						);
					graphics.DrawString(
						"Y",
						new Font(Encoding.Default.EncodingName, 9f),
						new SolidBrush(Color.Yellow), (PointF)new Point(runScreen.Left - 20, (runScreen.Top + (runScreen.Height / 2)) - 5)
						);
					graphics.DrawLine(
						pen,
						new Point(runScreen.Left - num, runScreen.Top - num),
						new Point(runScreen.Left + runScreen.Width, runScreen.Top - num)
						);
					graphics.DrawLine(
						pen,
						new Point(runScreen.Left + runScreen.Width - 8, runScreen.Top - num - 4),
						new Point(runScreen.Left + runScreen.Width, runScreen.Top - num)
						);
					graphics.DrawLine(
						pen,
						new Point(runScreen.Left - num, runScreen.Top - num),
						new Point(runScreen.Left - num, runScreen.Top + runScreen.Height)
						);
					graphics.DrawLine
						(pen,
						new Point(runScreen.Left - num - 4, runScreen.Top + runScreen.Height - 8),
						new Point(runScreen.Left - num, runScreen.Top + runScreen.Height)
						);
					panelView.ResumeLayout(false);
					panelView.PerformLayout();
				}
			}
			catch { }
		}

		private void mi_ID_Click(object sender, EventArgs e)
		{
			if (m_app != null)
			{
				m_app.IsShowName = !m_app.IsShowName;
				runScreen.RefreshPageEdit(m_page);
				refreshTimerVar();
			}
		}

		private void mi_Resolution_Click(object sender, EventArgs e)
		{
			if (m_app != null)
			{
				Form form = new DeviceParameters(m_app);
				form.ShowDialog();
				if (form.DialogResult == DialogResult.OK)
				{
					m_app.FileSave();
					m_app.ChangeApplication(true);
					runScreen.RunStop();
					runScreen.GuiInit(HmiOptions.RunFilePath, m_app, true);
					refreshPage();
				}
			}
		}

		private void mi_XY_Click(object sender, EventArgs e)
		{
			m_showXY = !m_showXY;
			panelView.Refresh();
		}

		private void mi_Debug_Click(object sender, EventArgs e)
		{
			MessageBox.Show("Temporary unavailable");
			return;

			if (m_app != null)
			{
				if ((!m_app.ChangeApp && m_binpath != "") || fileOperation("compile", ""))
					new SerialDebug(m_binpath).ShowDialog();
			}
			else
			{
				OpenFileDialog op = new OpenFileDialog();
				Utility.SetInitialPath(op, "filetft");
				op.Filter = "tft File|*.tft|All File|*.*".Translate();
				if (op.ShowDialog() == DialogResult.OK)
				{
					Utility.SavePath(op, "filetft");
					new SerialDebug(op.FileName).ShowDialog();
				}
			}
		}

		private void mi_Upload_Click(object sender, EventArgs e)
		{
			if (m_app == null)
			{
				OpenFileDialog dialog = new OpenFileDialog();
				dialog.Filter = "tft File|*.tft|All File|*.*".Translate();
				if (dialog.ShowDialog() != DialogResult.OK)
					return;

				m_binpath = dialog.FileName;
			}
			else if ((m_app.ChangeApp || m_binpath == "") && !fileOperation("compile", ""))
				return;

			new FirmwareUpload(m_binpath).ShowDialog();
		}

		#region Copy/Paste
		private void mi_Copy_Click(object sender, EventArgs e)
		{
			if (m_page != null)
			{
				if (m_obj != null && m_obj.Attributes[0].Data[0] != HmiObjType.PAGE)
				{
					m_copyObj = m_obj;
					mi_Paste.Enabled = true;
				}
				else
					MessageBox.Show("Please select component".Translate());
			}
			else
				MessageBox.Show("Please select page".Translate());
		}
		private void mi_Paste_Click(object sender, EventArgs e)
		{
			addComponent(2, m_copyObj.Attributes[0].Data[0]);
		}
		#endregion

		#region Add components
		private void mi_Text_Click(object sender, EventArgs e)
		{
			addComponent(0xff, HmiObjType.TEXT);
		}
		private void mi_Number_Click(object sender, EventArgs e)
		{
			addComponent(0xff, HmiObjType.NUMBER);
		}
		private void mi_Button_Click(object sender, EventArgs e)
		{
			addComponent(0xff, HmiObjType.BUTTON);
		}
		private void mi_ProgressBar_Click(object sender, EventArgs e)
		{
			addComponent(0xff, HmiObjType.PROG);
		}
		private void mi_Picture_Click(object sender, EventArgs e)
		{
			addComponent(0xff, HmiObjType.PICTURE);
		}
		private void mi_CropImage_Click(object sender, EventArgs e)
		{
			addComponent(0xff, HmiObjType.PICTUREC);
		}
		private void mi_Hotspot_Click(object sender, EventArgs e)
		{
			addComponent(0xff, HmiObjType.TOUCH);
		}
		private void mi_Gauges_Click(object sender, EventArgs e)
		{
			addComponent(0xff, HmiObjType.POINTER);
		}
		private void mi_Waveform_Click(object sender, EventArgs e)
		{
			addComponent(0xff, HmiObjType.OBJECT_TYPE_CURVE);
		}
		private void mi_Slider_Click(object sender, EventArgs e)
		{
			addComponent(0xff, HmiObjType.OBJECT_TYPE_SLIDER);
		}
		private void mi_Timer_Click(object sender, EventArgs e)
		{
			addComponent(0xff, HmiObjType.TIMER);
		}
		private void mi_Variable_Click(object sender, EventArgs e)
		{
			addComponent(0xff, HmiObjType.VAR);
		}
		private void mi_DualButton_Click(object sender, EventArgs e)
		{
			addComponent(0xff, HmiObjType.BUTTON_T);
		}
		private void mi_RadioBox_Click(object sender, EventArgs e)
		{
			MessageBox.Show("Coming Soon.".Translate());
		}
		private void mi_Checkbox_Click(object sender, EventArgs e)
		{
			MessageBox.Show("Coming Soon.".Translate());
		}
		#endregion

		private void mi_Open3_Click(object sender, EventArgs e)
		{
			if (!Directory.Exists(HmiOptions.AppDataBinPath))
				Directory.CreateDirectory(HmiOptions.AppDataBinPath);
			Utility.OpenWeb(HmiOptions.AppDataBinPath);
		}

		#region Save Other Version
		/// <summary>
		/// Save Other Version
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mi_SaveOther_Click(object sender, EventArgs e)
		{
			if (m_app == null
			 || m_openFile == null
			 || HmiOptions.RunFilePath == null
				)
			{
				MessageBox.Show("Please open or create new project.".Translate());
				return;
			}

			byte savedVerMajor = HmiOptions.VersionMajor;
			byte savedVerMinor = HmiOptions.VersionMinor;
			try
			{
				FormParams formParams = new FormParams();
				formParams.Strings = new string[1];

				Form form = new SaveOther(formParams);
				form.ShowDialog();
				if (form.DialogResult == DialogResult.OK)
				{
					string[] version = formParams.Strings[0].Split(Utility.CHAR_DOT);
					if (version.Length == 2)
					{
						HmiOptions.VersionMajor = (byte)Utility.GetInt(version[0]);
						HmiOptions.VersionMinor = (byte)Utility.GetInt(version[1]);
						fileOperation("saveAs", "");
						HmiOptions.VersionMajor = savedVerMajor;
						HmiOptions.VersionMinor = savedVerMinor;
					}
					else if (version.Length == 1 && version[0] == "XML")
						fileOperation("savexml", "");
				}
			}
			catch (Exception ex)
			{
				HmiOptions.VersionMajor = savedVerMajor;
				HmiOptions.VersionMinor = savedVerMinor;
				MessageBox.Show(ex.Message);
			}
		}
		#endregion

		private void mi_Help_Click(object sender, EventArgs e)
		{
			Utility.OpenWeb("http://wiki.iteadstudio.com/Nextion_HMI_Solution");
		}
		#region mi_FontGenerator_Click
		private void miFontGenerator_Click(object sender, EventArgs e)
		{
			FormParams formParams = new FormParams();
			formParams.Strings = new string[] { "" };

			new FontCreator(formParams).ShowDialog();
			if (m_app != null
			 && !string.IsNullOrEmpty(formParams.Strings[0])
			 && (MessageBox.Show(
					"Add the generated font?".Translate(),
					"Tips".Translate(),
					MessageBoxButtons.YesNo
					) == DialogResult.Yes
				))
			{
				fontAdmin.AddFont(formParams.Strings[0]);
				fontAdmin.RefreshFonts();
			}
		}
		#endregion
		#region 
		private void fontAdmin_FontUpdate(object sender, EventArgs e)
		{
			m_app.FileSave();
			m_app.ChangeApplication(true);
		}
		#endregion
		#region
		private void mi_CloseProject_Click(object sender, EventArgs e)
		{
			closeHMI();
		}
		#endregion
		#region
		private void mi_DeleteAll_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Are you sure to delete all components?".Translate(), "Confirm".Translate(), MessageBoxButtons.OKCancel) == DialogResult.OK)
				deleteObj(1);
		}
		#endregion
		#region
		private void mi_DeleteSelected_Click(object sender, EventArgs e)
		{
			deleteObj(0);
		}
		#endregion
		#region
		private void miEyeDropper_Click(object sender, EventArgs e)
		{
			new EyeDropper().ShowDialog();
		}
		#endregion

		#region
		private void miImport_Click(object sender, EventArgs e)
		{
			if (m_app == null)
			{
				MessageBox.Show("Please open the project".Translate());
			}
			else
			{
				int num = 0;
				HmiApplication _inf = new HmiApplication();
				OpenFileDialog op = new OpenFileDialog
				{
					Filter = "HMI File|*.HMI|All File|*.*".Translate()
				};
				Utility.SetInitialPath(op, "file");
				if (op.ShowDialog() == DialogResult.OK)
				{
					Utility.SavePath(op, "file");
					if (_inf.Open(op.FileName))
					{
						for (num = 0; num < _inf.HmiPages.Count; num++)
						{
							for (int i = 0; i < _inf.HmiPages[num].HmiObjects.Count; i++)
							{
								_inf.HmiPages[num].HmiObjects[i].App = m_app;
							}
							_inf.HmiPages[num].App = m_app;
							m_app.HmiPages.Add(_inf.HmiPages[num]);
							pageAdmin.RefreshObject(0);
						}
						for (num = 0; num < _inf.Fonts.Count; num++)
						{
							m_app.Fonts.Add(_inf.Fonts[num]);
							m_app.FontImages.Add(_inf.FontImages[num]);
							fontAdmin.RefreshFonts();
						}
						for (num = 0; num < _inf.Pictures.Count; num++)
						{
							m_app.Pictures.Add(_inf.Pictures[num]);
							m_app.PictureImages.Add(_inf.PictureImages[num]);
							picAdmin.RefreshPictures();
						}
						_inf = null;
						m_app.ChangeApplication(true);
					}
				}
			}
		}
		#endregion

		#region Languages
		private void miEnglish_Click(object sender, EventArgs e)
		{
			setLanguage(1);
		}
		private void miChinese_Click(object sender, EventArgs e)
		{
			setLanguage(0);
		}
		private void setLanguage(int language)
		{
			if (HmiOptions.Language != language)
			{
				HmiOptions.Language = language;

				Utility.SaveOption(HmiOptions.Language.ToString(), "Lang");
				Utility.Translate(this);
			}
		}
		#endregion
		#region mi_Exit_Click
		private void miExit_Click(object sender, EventArgs e)
		{
			Close();
		}
		#endregion
	}
}
