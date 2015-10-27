using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace HamCheck {
    internal partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
            this.Font = SystemFonts.MessageBoxFont;
            hamSelect.Font = new Font(this.Font.FontFamily, this.Font.Size * 1.5F);
            hamSetup.Font = new Font(this.Font.FontFamily, this.Font.Size * 1.5F);
            hamShow.Font = new Font(this.Font.FontFamily, this.Font.Size * 1.5F);
        }


        private void Form_Load(object sender, System.EventArgs e) {
            bwEnumerateExams.RunWorkerAsync();
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e) {
            if (hamShow.Visible) {
                hamShow_GoBack(null, null);
                e.Cancel = true;
            } else if (hamSetup.Visible) {
                hamSetup_GoBack(null, null);
                e.Cancel = true;
            }
        }


        #region Load all

        private void bwEnumerateExams_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e) {
            var sw = Stopwatch.StartNew();
            foreach (var exam in ExamElements.All) {
                Trace.WriteLine(string.Format("{0}: {1}", exam.Number, exam.Title));
            }
            Trace.WriteLine(string.Format("Loaded all exams in {0} ms.", sw.ElapsedMilliseconds));
            e.Result = ExamElements.All;
        }

        private void bwEnumerateExams_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e) {
            if (!e.Cancelled) {
                hamSelect.Elements = (IEnumerable<ExamElement>)e.Result;
                hamSelect.Visible = true;
                lblLoading.Visible = false;
                hamSelect.Focus();

            }
        }

        #endregion


        private void hamSelect_Selected(object sender, ExamElementEventArgs e) {
            hamSetup.Element = e.Element;
            hamSetup.Visible = true;
            hamSetup.Focus();

            hamSelect.Visible = false;
        }

        private void hamSetup_GoBack(object sender, EventArgs e) {
            hamSelect.Visible = true;
            hamSelect.Focus();

            hamSetup.Visible = false;
            hamSetup.Element = null;
        }

        private void hamSetup_Selected(object sender, ExamElementEventArgs e) {
            switch (e.Type) {
                case ExamType.Practice:
                    hamShow.ShowAnswerAfterEveryQuestion = false;
                    //hamShow.Items =
                    break;

                case ExamType.Randomize:
                    hamShow.ShowAnswerAfterEveryQuestion = true;
                    //hamShow.Items =
                    break;

                case ExamType.All:
                    hamShow.ShowAnswerAfterEveryQuestion = true;
                    hamShow.Items = hamSetup.Element.GetAllQuestions();
                    break;
            }

            hamShow.BackColor = Helper.GetColor(hamSetup.Element);
            hamShow.Visible = true;
            hamShow.Focus();
            hamSetup.Visible = false;
        }

        private void hamShow_GoBack(object sender, EventArgs e) {
            hamSetup.Visible = true;
            hamSetup.Focus();

            hamShow.Visible = false;
            hamShow.Items = null;
        }

    }
}
