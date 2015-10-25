namespace HamCheck {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.bwEnumerateExams = new System.ComponentModel.BackgroundWorker();
            this.lblLoading = new System.Windows.Forms.Label();
            this.hamSelect = new HamCheck.ExamSelectMenuControl();
            this.hamSetup = new HamCheck.ExamSetupMenuControl();
            this.SuspendLayout();
            // 
            // bwEnumerateExams
            // 
            this.bwEnumerateExams.WorkerSupportsCancellation = true;
            this.bwEnumerateExams.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwEnumerateExams_DoWork);
            this.bwEnumerateExams.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwEnumerateExams_RunWorkerCompleted);
            // 
            // lblLoading
            // 
            this.lblLoading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLoading.Location = new System.Drawing.Point(0, 0);
            this.lblLoading.Name = "lblLoading";
            this.lblLoading.Size = new System.Drawing.Size(582, 353);
            this.lblLoading.TabIndex = 0;
            this.lblLoading.Text = "Loading...";
            this.lblLoading.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // hamSelect
            // 
            this.hamSelect.BackColor = System.Drawing.SystemColors.Window;
            this.hamSelect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hamSelect.Elements = null;
            this.hamSelect.ForeColor = System.Drawing.SystemColors.WindowText;
            this.hamSelect.Location = new System.Drawing.Point(0, 0);
            this.hamSelect.Name = "hamSelect";
            this.hamSelect.Size = new System.Drawing.Size(582, 353);
            this.hamSelect.TabIndex = 0;
            this.hamSelect.Visible = false;
            this.hamSelect.Selected += new System.EventHandler<HamCheck.ExamElementEventArgs>(this.hamSelect_Selected);
            // 
            // hamSetup
            // 
            this.hamSetup.BackColor = System.Drawing.SystemColors.Window;
            this.hamSetup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hamSetup.Element = null;
            this.hamSetup.ForeColor = System.Drawing.SystemColors.WindowText;
            this.hamSetup.Location = new System.Drawing.Point(0, 0);
            this.hamSetup.Name = "hamSetup";
            this.hamSetup.Size = new System.Drawing.Size(582, 353);
            this.hamSetup.TabIndex = 1;
            this.hamSetup.Visible = false;
            this.hamSetup.Selected += new System.EventHandler<ExamElementEventArgs>(this.hamSetup_Selected);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 353);
            this.Controls.Add(this.hamSetup);
            this.Controls.Add(this.lblLoading);
            this.Controls.Add(this.hamSelect);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "MainForm";
            this.Text = "Ham check";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker bwEnumerateExams;
        private System.Windows.Forms.Label lblLoading;
        private ExamSelectMenuControl hamSelect;
        private ExamSetupMenuControl hamSetup;
    }
}

