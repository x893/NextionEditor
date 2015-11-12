using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NextionEditor
{
    public class MemberAdd : Form
    {
		private HmiApplication m_app;
		private HmiPage m_page;
		private HmiObject m_obj;
		
		private Button btnOK;
        private ComboBox cbAttrType;
        private IContainer components = null;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private TextBox tbAttrName;
		private TextBox tbComment;
        private TextBox tbValueSize;

        public MemberAdd(HmiApplication app, HmiPage page, HmiObject obj)
        {
            m_app = app;
            m_page = page;
            m_obj = obj;

            InitializeComponent();
			Utility.Translate(this);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            int length = 0;
            if (tbAttrName.Text.Trim().Length == 0)
				MessageBox.Show("Please Input the name.".Translate());
            else if (tbAttrName.Text.Trim().Length > 4)
				MessageBox.Show("Max Allow Length : 4 bytes".Translate());
            else if (cbAttrType.Text == "")
				MessageBox.Show("Please select type".Translate());
            else
            {
                length = Utility.GetInt(tbValueSize.Text.Trim());
                if (length == 0 || length > 255)
					MessageBox.Show("Length beyond allowed scope".Translate());
                else
                    base.DialogResult = DialogResult.OK;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbAttrType.SelectedIndex)
            {
                case 0:
                    tbValueSize.Text = "1";
                    tbValueSize.Enabled = false;
                    break;

                case 1:
                    tbValueSize.Text = "2";
                    tbValueSize.Enabled = false;
                    break;

                case 2:
                    tbValueSize.Text = "4";
                    tbValueSize.Enabled = false;
                    break;

                case 3:
                    tbValueSize.Text = "";
                    tbValueSize.Enabled = true;
                    break;
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
			this.label1 = new System.Windows.Forms.Label();
			this.tbAttrName = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.tbValueSize = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.btnOK = new System.Windows.Forms.Button();
			this.cbAttrType = new System.Windows.Forms.ComboBox();
			this.tbComment = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(3, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(62, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "Name";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// tbAttrName
			// 
			this.tbAttrName.Location = new System.Drawing.Point(72, 17);
			this.tbAttrName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.tbAttrName.Name = "tbAttrName";
			this.tbAttrName.Size = new System.Drawing.Size(96, 25);
			this.tbAttrName.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(3, 66);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(62, 18);
			this.label2.TabIndex = 2;
			this.label2.Text = "Type";
			this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// tbValueSize
			// 
			this.tbValueSize.Location = new System.Drawing.Point(72, 152);
			this.tbValueSize.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.tbValueSize.Name = "tbValueSize";
			this.tbValueSize.Size = new System.Drawing.Size(96, 25);
			this.tbValueSize.TabIndex = 5;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(3, 108);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(62, 18);
			this.label3.TabIndex = 4;
			this.label3.Text = "Comment";
			this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// btnOK
			// 
			this.btnOK.Location = new System.Drawing.Point(259, 141);
			this.btnOK.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(79, 44);
			this.btnOK.TabIndex = 6;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// cbAttrType
			// 
			this.cbAttrType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbAttrType.FormattingEnabled = true;
			this.cbAttrType.Location = new System.Drawing.Point(72, 63);
			this.cbAttrType.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.cbAttrType.Name = "cbAttrType";
			this.cbAttrType.Size = new System.Drawing.Size(96, 25);
			this.cbAttrType.TabIndex = 9;
			this.cbAttrType.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// tbComment
			// 
			this.tbComment.Location = new System.Drawing.Point(72, 105);
			this.tbComment.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.tbComment.Name = "tbComment";
			this.tbComment.Size = new System.Drawing.Size(265, 25);
			this.tbComment.TabIndex = 11;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(3, 155);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(62, 17);
			this.label4.TabIndex = 10;
			this.label4.Text = "Length";
			this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// MemberAdd
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(352, 197);
			this.Controls.Add(this.tbComment);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.cbAttrType);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.tbValueSize);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.tbAttrName);
			this.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.Name = "MemberAdd";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Add Members";
			this.Load += new System.EventHandler(this.ObjAttAdd_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

        }
		#endregion
		private void ObjAttAdd_Load(object sender, EventArgs e)
        {
            cbAttrType.Items.Clear();
            cbAttrType.Items.Add("unsigned char");
            cbAttrType.Items.Add("unsigned short");
            cbAttrType.Items.Add("unsigned int");
            cbAttrType.Items.Add("string");
        }
    }
}

