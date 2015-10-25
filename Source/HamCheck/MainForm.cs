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
        }

        private void Form_Load(object sender, System.EventArgs e) {
            bwEnumerateExams.RunWorkerAsync();
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

        private void hamSetup_Selected(object sender, ExamElementEventArgs e) {
            switch (e.Type) {
                case ExamType.Practice:
                    break;

                case ExamType.Randomize:
                    break;

                case ExamType.Find:
                    break;

                default:
                    hamSelect.Visible = true;
                    hamSelect.Focus();

                    hamSetup.Visible = false;
                    hamSetup.Element = null;
                    break;
            }
        }
    }
}
