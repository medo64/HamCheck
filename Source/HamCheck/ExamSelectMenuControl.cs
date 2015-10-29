using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
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


        private readonly ToolTip tip = new ToolTip();

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
                        var sbTip = new StringBuilder();
                        sbTip.AppendFormat(CultureInfo.CurrentCulture, "Element {0}: {1}", element.Number, element.Title);
                        if (DateTime.Now.Date < element.ValidFrom) {
                            sbTip.AppendFormat(CultureInfo.CurrentCulture, "\nValid from {0:d}", element.ValidFrom.Date);
                        } else if (DateTime.Now.Date <= element.ValidTo) {
                            sbTip.AppendFormat(CultureInfo.CurrentCulture, "\nValid to {0:d}", element.ValidTo.Date);
                        } else {
                            sbTip.AppendFormat(CultureInfo.CurrentCulture, "\nInvalid since {0:d}", element.ValidTo.Date);
                        }
                        tip.SetToolTip(btn, sbTip.ToString());
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
            using (var scaledFont = new Font(this.Font.FontFamily, this.Font.Size * Settings.FontScale))
            using (var g = this.CreateGraphics()) {
                var emSize = g.MeasureString("M", scaledFont).ToSize();

                if (this.Controls.Count > 0) {
                    var width = emSize.Width * 18;
                    var height = (int)(emSize.Height * 2.5);

                    var left = (this.Width - width) / 2;
                    var top = (this.Height - this.Controls.Count * height - (this.Controls.Count - 1) * height / 4) / 2;

                    for (int i = 0; i < this.Controls.Count; i++) {
                        this.Controls[i].Font = scaledFont;
                        this.Controls[i].Location = new Point(left, top);
                        this.Controls[i].Size = new Size(width, height);
                        top += height + height / 4;
                    }
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


                case Keys.Control | Keys.D0:
                case Keys.Control | Keys.NumPad0:
                case Keys.D0:
                case Keys.NumPad0:
                    Settings.FontScale = Settings.DefaultFontScale;
                    this.OnResize(null);
                    break;

                case Keys.Control | Keys.Add:
                case Keys.Control | Keys.Oemplus:
                case Keys.Add:
                case Keys.Oemplus:
                    Settings.FontScale += 0.1F;
                    this.OnResize(null);
                    break;

                case Keys.Control | Keys.Subtract:
                case Keys.Control | Keys.OemMinus:
                case Keys.Subtract:
                case Keys.OemMinus:
                    Settings.FontScale -= 0.1F;
                    this.OnResize(null);
                    break;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
            if (Control.ModifierKeys == Keys.Control) {
                var detents = e.Delta / SystemInformation.MouseWheelScrollDelta;
                Settings.FontScale += detents / 10.0F;
                this.OnResize(null);
            } else {
                base.OnMouseWheel(e);
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
