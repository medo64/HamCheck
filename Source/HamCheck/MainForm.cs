using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace HamCheck {
    internal partial class MainForm : Form {
        public MainForm() {
            this.InitializeComponent();
            this.Font = SystemFonts.MessageBoxFont;
            hamSelect.Font = this.Font;
            hamSetup.Font = this.Font;
            hamShow.Font = this.Font;
        }


        private void Form_Shown(object sender, EventArgs e) {
            bwEnumerateExams.RunWorkerAsync();
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e) {
            if (hamShow.Visible) {
                this.hamShow_GoBack(null, null);
                e.Cancel = true;
            } else if (hamSetup.Visible) {
                this.hamSetup_GoBack(null, null);
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
                case ExamType.Exam:
                    hamShow.ShowAnswerAfterEveryQuestion = false;
                    hamShow.ShowResultsAfterQuestions = true;
                    hamShow.Items = hamSetup.Element.GetExamQuestions();
                    break;

                case ExamType.FlashExam:
                    hamShow.ShowAnswerAfterEveryQuestion = true;
                    hamShow.ShowResultsAfterQuestions = true;
                    hamShow.Items = hamSetup.Element.GetExamQuestions();
                    break;

                case ExamType.Randomize:
                    hamShow.ShowAnswerAfterEveryQuestion = true;
                    hamShow.ShowResultsAfterQuestions = false;
                    hamShow.Items = hamSetup.Element.GetRandomizedQuestions();
                    break;

                case ExamType.All:
                    hamShow.ShowAnswerAfterEveryQuestion = true;
                    hamShow.ShowResultsAfterQuestions = false;
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
