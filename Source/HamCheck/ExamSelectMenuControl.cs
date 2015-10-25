using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace HamCheck {
    internal class ExamSelectMenuControl : ScrollableControl {

        public ExamSelectMenuControl() {
            this.BackColor = SystemColors.Window;
            this.ForeColor = SystemColors.WindowText;

            this.Paint += Control_Paint;
            this.Resize += Control_Resize;
            this.KeyDown += Control_KeyDown;

            this.Control_Resize(null, null);
        }


        private Size EmSize { get; set; }


        private IEnumerable<ExamElement> _elements;
        public IEnumerable<ExamElement> Elements {
            get { return this._elements; }
            set {
                this._elements = value;
                this.Controls.Clear();

                if (this._elements != null) {
                    foreach (var element in this._elements) {
                        var btn = new Button() { FlatStyle = FlatStyle.Flat, Tag = element };
                        btn.Text = string.Format(CultureInfo.CurrentCulture, "{1}", element.Number, element.Title, element.ValidFrom.Year, element.ValidTo.Year);
                        btn.KeyDown += Control_KeyDown;
                        btn.BackColor = Helper.GetColor(element);

                        btn.Click += delegate (object sender, EventArgs e) {
                            var exam = (ExamElement)(((Control)sender).Tag);
                            var eh = this.Selected;
                            if (eh != null) {
                                eh.Invoke(this, new ExamElementEventArgs(exam));
                            }
                        };

                        this.Controls.Add(btn);
                    }
                }

                this.Control_Resize(null, null);
            }
        }


        #region Events

        private void Control_Paint(object sender, PaintEventArgs e) {
            Control_Resize(null, null);
        }

        private void Control_Resize(object sender, EventArgs e) {
            using (var g = this.CreateGraphics()) {
                this.EmSize = g.MeasureString("M", this.Font).ToSize();
            }

            if (this.Controls.Count > 0) {
                var width = this.EmSize.Width * 18;
                var height = this.EmSize.Width * 3;

                var left = (this.Width - width) / 2;
                var top = (this.Height - this.Controls.Count * height - (this.Controls.Count - 1) * height / 4) / 2;

                for (int i = 0; i < this.Controls.Count; i++) {
                    this.Controls[i].Location = new Point(left, top);
                    this.Controls[i].Size = new Size(width, height);
                    top += height + height / 4;
                }
            }
        }


        private void Control_KeyDown(object sender, KeyEventArgs e) {
            switch (e.KeyData) {
                case Keys.T:
                case Keys.D2:
                case Keys.NumPad2:
                    PerformClick(2);
                    break;

                case Keys.G:
                case Keys.D3:
                case Keys.NumPad3:
                    PerformClick(3);
                    break;

                case Keys.E:
                case Keys.D4:
                case Keys.NumPad4:
                    PerformClick(4);
                    break;

                default: Debug.WriteLine(e.KeyData); break;
            }
        }

        private void PerformClick(int elementNumber) {
            foreach (Button button in this.Controls) {
                var element = (ExamElement)(button.Tag);
                if (element.Number == elementNumber) {
                    button.PerformClick();
                    break;
                }
            }
        }

        #endregion


        public event EventHandler<ExamElementEventArgs> Selected;

    }
}
