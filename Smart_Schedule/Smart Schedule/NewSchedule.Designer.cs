namespace Smart_Schedule
{
    partial class NewSchedule
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnCancel = new System.Windows.Forms.Button();
            this.cmbImpAls = new System.Windows.Forms.ComboBox();
            this.fdOpen = new System.Windows.Forms.OpenFileDialog();
            this.btnOK = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.chkAlias = new System.Windows.Forms.CheckBox();
            this.btnFileDiag = new System.Windows.Forms.Button();
            this.txtDataFile = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSchName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(358, 151);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 19;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // cmbImpAls
            // 
            this.cmbImpAls.FormattingEnabled = true;
            this.cmbImpAls.Location = new System.Drawing.Point(133, 82);
            this.cmbImpAls.Name = "cmbImpAls";
            this.cmbImpAls.Size = new System.Drawing.Size(121, 21);
            this.cmbImpAls.TabIndex = 17;
            // 
            // fdOpen
            // 
            this.fdOpen.FileName = "*.xls";
            this.fdOpen.Title = "Select Data File";
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(277, 151);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 18;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(16, 109);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(80, 17);
            this.checkBox1.TabIndex = 16;
            this.checkBox1.Text = "checkBox1";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.Visible = false;
            // 
            // chkAlias
            // 
            this.chkAlias.AutoSize = true;
            this.chkAlias.Location = new System.Drawing.Point(16, 82);
            this.chkAlias.Name = "chkAlias";
            this.chkAlias.Size = new System.Drawing.Size(80, 17);
            this.chkAlias.TabIndex = 15;
            this.chkAlias.Text = "Import Alias";
            this.chkAlias.UseVisualStyleBackColor = true;
            // 
            // btnFileDiag
            // 
            this.btnFileDiag.Location = new System.Drawing.Point(391, 49);
            this.btnFileDiag.Name = "btnFileDiag";
            this.btnFileDiag.Size = new System.Drawing.Size(35, 23);
            this.btnFileDiag.TabIndex = 14;
            this.btnFileDiag.Text = "...";
            this.btnFileDiag.UseVisualStyleBackColor = true;
            this.btnFileDiag.Click += new System.EventHandler(this.btnFileDiag_Click);
            // 
            // txtDataFile
            // 
            this.txtDataFile.Location = new System.Drawing.Point(133, 49);
            this.txtDataFile.Margin = new System.Windows.Forms.Padding(4);
            this.txtDataFile.Name = "txtDataFile";
            this.txtDataFile.Size = new System.Drawing.Size(251, 20);
            this.txtDataFile.TabIndex = 13;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 52);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Data File:";
            // 
            // txtSchName
            // 
            this.txtSchName.Location = new System.Drawing.Point(133, 18);
            this.txtSchName.Margin = new System.Windows.Forms.Padding(4);
            this.txtSchName.Name = "txtSchName";
            this.txtSchName.Size = new System.Drawing.Size(251, 20);
            this.txtSchName.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 21);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Schedule Name:";
            // 
            // NewSchedule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(446, 193);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.cmbImpAls);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.chkAlias);
            this.Controls.Add(this.btnFileDiag);
            this.Controls.Add(this.txtDataFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtSchName);
            this.Controls.Add(this.label1);
            this.Name = "NewSchedule";
            this.Text = "NewSchedule";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NewSchedule_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ComboBox cmbImpAls;
        private System.Windows.Forms.OpenFileDialog fdOpen;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox chkAlias;
        private System.Windows.Forms.Button btnFileDiag;
        private System.Windows.Forms.TextBox txtDataFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSchName;
        private System.Windows.Forms.Label label1;

    }
}