using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NextionEditor
{
    public class SaveOther : Form
    {
        private Button btnOK;
        private ComboBox cbVersion;
        private IContainer components = null;
        private FormParams m_params;
        private Label label1;

        public SaveOther(FormParams formParams)
        {
            m_params = formParams;
            InitializeComponent();
			Utility.Translate(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (cbVersion.Text.Length == 0)
				MessageBox.Show("Select the version".Translate(), "Select the version".Translate());
            else
            {
                m_params.Strings[0] = cbVersion.Text;
                base.DialogResult = DialogResult.OK;
            }
        }

		private void SaveOther_Load(object sender, EventArgs e)
		{
			cbVersion.Items.Clear();
			cbVersion.Items.Add("XML");
			cbVersion.Items.Add("0.29");
			cbVersion.SelectedIndex = 0;
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
			this.cbVersion = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnOK = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// cbVersion
			// 
			this.cbVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbVersion.FormattingEnabled = true;
			this.cbVersion.Location = new System.Drawing.Point(146, 31);
			this.cbVersion.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.cbVersion.Name = "cbVersion";
			this.cbVersion.Size = new System.Drawing.Size(103, 25);
			this.cbVersion.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(52, 34);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(88, 17);
			this.label1.TabIndex = 1;
			this.label1.Text = "Select version";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// btnOK
			// 
			this.btnOK.Location = new System.Drawing.Point(257, 29);
			this.btnOK.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(90, 30);
			this.btnOK.TabIndex = 2;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.button1_Click);
			// 
			// SaveOther
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(407, 112);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cbVersion);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.Name = "SaveOther";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Save Other version";
			this.Load += new System.EventHandler(this.SaveOther_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
	}
}

