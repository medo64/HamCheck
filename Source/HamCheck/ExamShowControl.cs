using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace HamCheck {
    internal class ExamShowControl : ScrollableControl {

        public ExamShowControl() {
            this.AutoScroll = true;
            this.BackColor = SystemColors.Window;
            this.ForeColor = SystemColors.WindowText;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.StandardClick, true);
            this.SetStyle(ControlStyles.StandardDoubleClick, true);
            this.SetStyle(ControlStyles.UserPaint, true);

            this.Paint += Control_Paint;
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

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);

            switch (e.KeyData) {
                case Keys.Escape:
                    var eh = this.GoBack;
                    if (eh != null) { eh.Invoke(this, new EventArgs()); }
                    break;

                case Keys.Right:
                    this.ItemIndex += 1;
                    this.Invalidate();
                    break;

                case Keys.Left:
                    this.ItemIndex -= 1;
                    this.Invalidate();
                    break;

                case Keys.Down:
                    {
                        var currY = -this.AutoScrollPosition.Y;
                        var maxY = (this.AutoScrollMinSize.Height - this.Height);
                        if (currY < maxY) {
                            this.AutoScrollPosition = new Point(this.AutoScrollPosition.X, Math.Min(-this.AutoScrollPosition.Y + SystemInformation.CursorSize.Height, maxY));
                        }
                    }
                    break;

                case Keys.Up:
                    {
                        var currY = -this.AutoScrollPosition.Y;
                        if (currY > 0) {
                            this.AutoScrollPosition = new Point(this.AutoScrollPosition.X, Math.Max(-this.AutoScrollPosition.Y - SystemInformation.CursorSize.Height, 0));
                        }
                    }
                    break;

            }
        }

        private ReadOnlyCollection<ExamItem> _items;
        public ReadOnlyCollection<ExamItem> Items {
            get { return this._items; }
            set {
                this._items = value;

                if (this._items != null) {
                }

                this.Invalidate();
            }
        }


        public bool ShowAnswerAfterEveryQuestion { get; set; }


        #region Events

        private int ItemIndex;

        private void Control_Paint(object sender, PaintEventArgs e) {
            e.Graphics.TranslateTransform(this.AutoScrollPosition.X, this.AutoScrollPosition.Y);

            if (this.Items == null) { return; }

            var item = this.Items[this.ItemIndex];

            var emSize = e.Graphics.MeasureString("M", this.Font).ToSize();

            var width = emSize.Width * 36;
            var height = emSize.Width * 24;

            var maxWidth = this.Width - SystemInformation.Border3DSize.Width - SystemInformation.VerticalScrollBarWidth;
            var maxHeight = this.Height - SystemInformation.Border3DSize.Height * 2;
            if (width > maxWidth) { width = maxWidth; }
            if (height > maxHeight) { height = maxHeight; }

            var left = (this.Width - width) / 2;
            var right = left + width;
            var top = (this.Height - height) / 2 - emSize.Height;
            var bottom = top + height;
            if (top < SystemInformation.Border3DSize.Width) { top = SystemInformation.Border3DSize.Height; }

            top -= this.AutoScrollOffset.Y;
            //e.Graphics.DrawRectangle(Pens.Gray, left, top, width, height);

            var questionCodeRectange = new Rectangle(left, top, width, emSize.Height);
            e.Graphics.DrawString(item.Question.Code, this.Font, SystemBrushes.GrayText, questionCodeRectange, StringFormat.GenericTypographic);

            var questionNumberRectange = new Rectangle(left, top, width - SystemInformation.VerticalScrollBarWidth - SystemInformation.Border3DSize.Width, emSize.Height);
            var questionNumberText = string.Format(CultureInfo.CurrentCulture, "{0}/{1}", this.ItemIndex + 1, this.Items.Count);
            e.Graphics.DrawString(questionNumberText, this.Font, SystemBrushes.GrayText, questionNumberRectange, new StringFormat(StringFormat.GenericTypographic) { Alignment = StringAlignment.Far });


            top += emSize.Height * 2;

            int illustrationBottom = 0;
            int illustrationWidth = 0;
            if (item.Question.Illustration != null) {
                var imageWidth = item.Question.Illustration.Picture.Width;
                var imageHeight = item.Question.Illustration.Picture.Height;
                while (imageWidth > width / 2) {
                    imageWidth = (int)(imageWidth * 0.75);
                    imageHeight = (int)(imageHeight * 0.75);
                }
                var imageLeft = left + width - imageWidth - SystemInformation.VerticalScrollBarWidth;
                var imageTop = top;
                var imageRectange = new Rectangle(imageLeft, imageTop, imageWidth, imageHeight);

                e.Graphics.DrawImage(item.Question.Illustration.Picture, imageRectange);
                //e.Graphics.DrawRectangle(Pens.Red, imageRectange);

                illustrationBottom = imageRectange.Bottom + emSize.Height / 4;
                illustrationWidth = imageRectange.Width + emSize.Height / 4;
            }

            var questionFont = new Font(this.Font.FontFamily, this.Font.Size * 1.2F);
            int maxQuestionWidth = width - ((top < illustrationBottom) ? illustrationWidth : 0) - SystemInformation.VerticalScrollBarWidth;
            var questionTextSize = e.Graphics.MeasureString(item.Question.Text, questionFont, maxQuestionWidth, StringFormat.GenericDefault).ToSize();
            var questionRectange = new Rectangle(left, top, questionTextSize.Width, questionTextSize.Height);
            e.Graphics.DrawString(item.Question.Text, questionFont, SystemBrushes.WindowText, questionRectange, StringFormat.GenericTypographic);
            //e.Graphics.DrawRectangle(Pens.Blue, questionRectange);

            top += questionTextSize.Height + emSize.Height;

            var answerLetter = 'A';
            foreach (var answer in item.Answers) {
                e.Graphics.DrawString(answerLetter.ToString(), new Font(this.Font, FontStyle.Bold), SystemBrushes.WindowText, left, top, StringFormat.GenericTypographic);

                int maxAnswerWidth = width - ((top < illustrationBottom) ? illustrationWidth : 0) - emSize.Width - SystemInformation.VerticalScrollBarWidth;
                var answerTextSize = e.Graphics.MeasureString(answer.Text, this.Font, maxAnswerWidth, StringFormat.GenericDefault).ToSize();
                var answerRectangle = new Rectangle(left + emSize.Width, top, answerTextSize.Width, answerTextSize.Height);
                e.Graphics.DrawString(answer.Text, this.Font, SystemBrushes.WindowText, answerRectangle, StringFormat.GenericTypographic);
                //e.Graphics.DrawRectangle(Pens.Green, answerRectangle);

                top += answerTextSize.Height + emSize.Height;
                answerLetter++;
            }

            top -= emSize.Height;
            top += SystemInformation.Border3DSize.Height;

            var newSize = new Size(width, top);
            if (this.AutoScrollMinSize != newSize) { this.AutoScrollMinSize = newSize; }
        }

        #endregion

        public event EventHandler<EventArgs> GoBack;

    }
}
