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
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            this.KeyDown += Control_KeyDown;

            this.OnResize(null);
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

                this.OnResize(null);
            }
        }


        #region Events

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            this.OnResize(null);
        }

        protected override void OnResize(EventArgs e) {
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


        protected override bool IsInputKey(Keys keyData) {
            switch (keyData) {
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                    return true;
                default: return base.IsInputKey(keyData);
            }
        }

        private void Control_KeyDown(object sender, KeyEventArgs e) {
            switch (e.KeyData) {
                case Keys.Down:
                    {
                        var control = this.GetNextControl(this, true);
                        if (control != null) { control.Select(); }
                    }
                    break;

                case Keys.Up:
                    {
                        var control = this.GetNextControl(this, false);
                        if (control != null) { control.Select(); }
                    }
                    break;

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
