using System;
using System.Drawing;
using System.Windows.Forms;

namespace HamCheck {
    internal class ExamSetupMenuControl : ScrollableControl {

        public ExamSetupMenuControl() {
            this.BackColor = SystemColors.Window;
            this.ForeColor = SystemColors.WindowText;
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            this.KeyDown += Control_KeyDown;

            this.OnResize(null);
        }


        private readonly ToolTip tip = new ToolTip();

        private ExamElement _element;
        public ExamElement Element {
            get { return this._element; }
            set {
                this._element = value;
                this.Controls.Clear();

                if (this._element != null) {
                    var buttonColor = Helper.GetColor(this._element);

                    {
                        var btn = new Button() { Text = "Practice exam", BackColor = buttonColor, FlatStyle = FlatStyle.Flat, Tag = Keys.P };
                        btn.KeyDown += Control_KeyDown;
                        btn.Click += delegate (object sender, EventArgs e) {
                            var eh = this.Selected;
                            if (eh != null) { eh.Invoke(sender, new ExamElementEventArgs(value, ExamType.Exam)); }
                        };
                        this.Controls.Add(btn);
                        tip.SetToolTip(btn, "Answer questions using the same selection process as for the real exam:\n• Randomly select one question from each group.\n• Randomize the answers.\n• Show answers only after all questions are answered.\n• Score the result.");
                    }
                    {
                        var btn = new Button() { Text = "Flash exam", BackColor = buttonColor, FlatStyle = FlatStyle.Flat, Tag = Keys.F };
                        btn.KeyDown += Control_KeyDown;
                        btn.Click += delegate (object sender, EventArgs e) {
                            var eh = this.Selected;
                            if (eh != null) { eh.Invoke(sender, new ExamElementEventArgs(value, ExamType.FlashExam)); }
                        };
                        this.Controls.Add(btn);
                        tip.SetToolTip(btn, "Answer questions using the same selection process as for the real exam:\n• Randomly select one question from each group.\n• Randomize the answers.\n• Show answers after each question.\n• Score the result.");
                    }
                    {
                        var btn = new Button() { Text = "Randomize", BackColor = buttonColor, FlatStyle = FlatStyle.Flat, Tag = Keys.R };
                        btn.KeyDown += Control_KeyDown;
                        btn.Click += delegate (object sender, EventArgs e) {
                            var eh = this.Selected;
                            if (eh != null) { eh.Invoke(sender, new ExamElementEventArgs(value, ExamType.Randomize)); }
                        };
                        this.Controls.Add(btn);
                        tip.SetToolTip(btn, "Answer questions in random order:\n• Randomize all questions.\n• Randomize the answers.\n• Show correct answer immediately after question.");
                    }
                    {
                        var btn = new Button() { Text = "All", BackColor = buttonColor, FlatStyle = FlatStyle.Flat, Tag = Keys.A };
                        btn.KeyDown += Control_KeyDown;
                        btn.Click += delegate (object sender, EventArgs e) {
                            var eh = this.Selected;
                            if (eh != null) { eh.Invoke(sender, new ExamElementEventArgs(value, ExamType.All)); }
                        };
                        this.Controls.Add(btn);
                        tip.SetToolTip(btn, "Go through whole question pool:\n• Questions are not randomized.\n• Answers are not randomized.\n• Show correct answer immediately after question.");
                    }

                    {
                        var btn = new Button() { Text = "Go back", FlatStyle = FlatStyle.Flat, Tag = Keys.Escape };
                        btn.KeyDown += Control_KeyDown;
                        btn.Click += delegate (object sender, EventArgs e) {
                            var eh = this.GoBack;
                            if (eh != null) { eh.Invoke(sender, new EventArgs()); }
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
            using (var scaledFont = new Font(this.Font.FontFamily, this.Font.Size * Settings.MenuFontScale))
            using (var g = this.CreateGraphics()) {
                var emSize = g.MeasureString("M", scaledFont).ToSize();

                if (this.Controls.Count > 0) {
                    var width = emSize.Width * 18;
                    var height = (int)(emSize.Height * 2.5);

                    var left = (this.Width - width) / 2;
                    var top = (this.Height - this.Controls.Count * height - this.Controls.Count * height / 4) / 2;

                    for (int i = 0; i < this.Controls.Count; i++) {
                        this.Controls[i].Font = scaledFont;
                        this.Controls[i].Location = new Point(left, top);
                        this.Controls[i].Size = new Size(width, height);
                        top += height + height / 4;
                        if (i == this.Controls.Count - 2) { top += height / 4; }
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


                case Keys.Control | Keys.D0:
                case Keys.Control | Keys.NumPad0:
                case Keys.D0:
                case Keys.NumPad0:
                    Settings.MenuFontScale = Settings.DefaultFontScale;
                    this.OnResize(null);
                    break;

                case Keys.Control | Keys.Add:
                case Keys.Control | Keys.Oemplus:
                case Keys.Add:
                case Keys.Oemplus:
                    Settings.MenuFontScale += 0.1F;
                    this.OnResize(null);
                    break;

                case Keys.Control | Keys.Subtract:
                case Keys.Control | Keys.OemMinus:
                case Keys.Subtract:
                case Keys.OemMinus:
                    Settings.MenuFontScale -= 0.1F;
                    this.OnResize(null);
                    break;


                default:
                    foreach (Button button in this.Controls) {
                        if ((button.Tag != null) && ((Keys)(button.Tag) == e.KeyData)) {
                            button.PerformClick();
                        }
                    }
                    break;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
            if (Control.ModifierKeys == Keys.Control) {
                var detents = e.Delta / SystemInformation.MouseWheelScrollDelta;
                Settings.MenuFontScale += detents / 10.0F;
                this.OnResize(null);
            } else {
                base.OnMouseWheel(e);
            }
        }

        #endregion


        public event EventHandler<ExamElementEventArgs> Selected;
        public event EventHandler<EventArgs> GoBack;

    }
}
