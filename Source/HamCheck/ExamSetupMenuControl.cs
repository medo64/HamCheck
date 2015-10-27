using System;
using System.Drawing;
using System.Windows.Forms;

namespace HamCheck {
    internal class ExamSetupMenuControl : ScrollableControl {

        public ExamSetupMenuControl() {
            this.BackColor = SystemColors.Window;
            this.ForeColor = SystemColors.WindowText;

            this.Paint += Control_Paint;
            this.Resize += Control_Resize;
            this.KeyDown += Control_KeyDown;

            this.Control_Resize(null, null);
        }


        private Size EmSize { get; set; }


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
                            if (eh != null) { eh.Invoke(sender, new ExamElementEventArgs(value, ExamType.Practice)); }
                        };
                        this.Controls.Add(btn);
                    }
                    {
                        var btn = new Button() { Text = "Randomize", BackColor = buttonColor, FlatStyle = FlatStyle.Flat, Tag = Keys.R };
                        btn.KeyDown += Control_KeyDown;
                        btn.Click += delegate (object sender, EventArgs e) {
                            var eh = this.Selected;
                            if (eh != null) { eh.Invoke(sender, new ExamElementEventArgs(value, ExamType.Randomize)); }
                        };
                        this.Controls.Add(btn);
                    }
                    {
                        var btn = new Button() { Text = "All", BackColor = buttonColor, FlatStyle = FlatStyle.Flat, Tag = Keys.A };
                        btn.KeyDown += Control_KeyDown;
                        btn.Click += delegate (object sender, EventArgs e) {
                            var eh = this.Selected;
                            if (eh != null) { eh.Invoke(sender, new ExamElementEventArgs(value, ExamType.All)); }
                        };
                        this.Controls.Add(btn);
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
                var top = (this.Height - this.Controls.Count * height - this.Controls.Count * height / 4) / 2;

                for (int i = 0; i < this.Controls.Count; i++) {
                    this.Controls[i].Location = new Point(left, top);
                    this.Controls[i].Size = new Size(width, height);
                    top += height + height / 4;
                    if (i == this.Controls.Count - 2) { top += height / 4; }
                }
            }
        }


        private void Control_KeyDown(object sender, KeyEventArgs e) {
            foreach (Button button in this.Controls) {
                if ((button.Tag != null) && ((Keys)(button.Tag) == e.KeyData)) {
                    button.PerformClick();
                }
            }
        }

        #endregion


        public event EventHandler<ExamElementEventArgs> Selected;
        public event EventHandler<EventArgs> GoBack;

    }
}
