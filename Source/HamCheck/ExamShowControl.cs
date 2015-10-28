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
        }


        private ReadOnlyCollection<ExamItem> _items;
        public ReadOnlyCollection<ExamItem> Items {
            get { return this._items; }
            set {
                this._items = value;
                this.ItemIndex = 0;
                this.LastAnswerIndex = -1;
                this.ShowingAnswer = false;

                if (this._items != null) {
                }

                this.Invalidate();
            }
        }


        public bool ShowAnswerAfterEveryQuestion { get; set; }


        #region Events

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
                case Keys.Space:
                    if (this.ShowAnswerAfterEveryQuestion && !this.ShowingAnswer) {
                        this.LastAnswerIndex = Math.Max(this.LastAnswerIndex, this.ItemIndex);
                        this.ShowingAnswer = true;
                        this.Invalidate();
                    } else if (this.ItemIndex < this.Items.Count - 1) {
                        this.ShowingAnswer = false;
                        this.ItemIndex += 1;
                        this.Invalidate();
                    }
                    break;

                case Keys.Left:
                    if (this.ShowAnswerAfterEveryQuestion && this.ShowingAnswer) {
                        this.ShowingAnswer = false;
                        this.Invalidate();
                    } else if (this.ShowAnswerAfterEveryQuestion && (this.ItemIndex > 0)) {
                        this.ShowingAnswer = true;
                        this.ItemIndex -= 1;
                        this.Invalidate();
                    } else if ((this.ItemIndex > 0)) {
                        this.ShowingAnswer = false;
                        this.ItemIndex -= 1;
                        this.Invalidate();
                    }
                    break;

                case Keys.PageDown:
                    {
                        var currY = -this.AutoScrollPosition.Y;
                        var maxY = (this.AutoScrollMinSize.Height - this.Height);
                        if (currY < maxY) {
                            this.AutoScrollPosition = new Point(this.AutoScrollPosition.X, Math.Min(-this.AutoScrollPosition.Y + SystemInformation.CursorSize.Height, maxY));
                        }
                    }
                    break;

                case Keys.PageUp:
                    {
                        var currY = -this.AutoScrollPosition.Y;
                        if (currY > 0) {
                            this.AutoScrollPosition = new Point(this.AutoScrollPosition.X, Math.Max(-this.AutoScrollPosition.Y - SystemInformation.CursorSize.Height, 0));
                        }
                    }
                    break;

                case Keys.A:
                case Keys.D1:
                case Keys.NumPad1:
                    if (this.ShowingAnswer || (this.ItemIndex <= this.LastAnswerIndex)) { break; }
                    if ((this.Items != null) && (this.ItemIndex < this.Items.Count)) {
                        var item = this.Items[this.ItemIndex];
                        item.SelectedAnswerIndex = 0;
                        this.Invalidate();
                    }
                    break;

                case Keys.B:
                case Keys.D2:
                case Keys.NumPad2:
                    if (this.ShowingAnswer || (this.ItemIndex <= this.LastAnswerIndex)) { break; }
                    if ((this.Items != null) && (this.ItemIndex < this.Items.Count)) {
                        var item = this.Items[this.ItemIndex];
                        item.SelectedAnswerIndex = 1;
                        this.Invalidate();
                    }
                    break;

                case Keys.C:
                case Keys.D3:
                case Keys.NumPad3:
                    if (this.ShowingAnswer || (this.ItemIndex <= this.LastAnswerIndex)) { break; }
                    if ((this.Items != null) && (this.ItemIndex < this.Items.Count)) {
                        var item = this.Items[this.ItemIndex];
                        item.SelectedAnswerIndex = 2;
                        this.Invalidate();
                    }
                    break;

                case Keys.D:
                case Keys.D4:
                case Keys.NumPad4:
                    if (this.ShowingAnswer || (this.ItemIndex <= this.LastAnswerIndex)) { break; }
                    if ((this.Items != null) && (this.ItemIndex < this.Items.Count)) {
                        var item = this.Items[this.ItemIndex];
                        item.SelectedAnswerIndex = 3;
                        this.Invalidate();
                    }
                    break;

                case Keys.Up:
                    if (this.ShowingAnswer || (this.ItemIndex <= this.LastAnswerIndex)) { break; }
                    if ((this.Items != null) && (this.ItemIndex < this.Items.Count)) {
                        var item = this.Items[this.ItemIndex];
                        if (item.SelectedAnswerIndex == null) {
                            item.SelectedAnswerIndex = 0;
                        } else if (item.SelectedAnswerIndex > 0) {
                            item.SelectedAnswerIndex -= 1;
                        }
                        this.Invalidate();
                    }
                    break;

                case Keys.Down:
                    if (this.ShowingAnswer || (this.ItemIndex <= this.LastAnswerIndex)) { break; }
                    if ((this.Items != null) && (this.ItemIndex < this.Items.Count)) {
                        var item = this.Items[this.ItemIndex];
                        if (item.SelectedAnswerIndex == null) {
                            item.SelectedAnswerIndex = 0;
                        } else if (item.SelectedAnswerIndex < item.Answers.Count - 1) {
                            item.SelectedAnswerIndex += 1;
                        }
                        this.Invalidate();
                    }
                    break;

                case Keys.Back:
                case Keys.D0:
                case Keys.NumPad0:
                    if (this.ShowingAnswer || (this.ItemIndex <= this.LastAnswerIndex)) { break; }
                    if ((this.Items != null) && (this.ItemIndex < this.Items.Count)) {
                        var item = this.Items[this.ItemIndex];
                        item.SelectedAnswerIndex = null;
                        this.Invalidate();
                    }
                    break;

            }
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            if (this.ShowingAnswer || (this.ItemIndex <= this.LastAnswerIndex)) { return; }
            if ((this.Items != null) && (this.ItemIndex < this.Items.Count)) {
                var item = this.Items[this.ItemIndex];
                for (int i = 0; i < this.AnswerRectangles.Count; i++) {
                    if (this.AnswerRectangles[i].Contains(e.Location)) {
                        item.SelectedAnswerIndex = i;
                        this.Invalidate();
                        break;
                    }
                }
            }
        }


        private int ItemIndex;
        private int LastAnswerIndex;
        private bool ShowingAnswer;
        private List<Rectangle> AnswerRectangles = new List<Rectangle>();


        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            this.AnswerRectangles.Clear();

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

            using (var boldFont = new Font(this.Font, FontStyle.Bold)) {
                for (int i = 0; i < item.Answers.Count; i++) {
                    var answer = item.Answers[i];

                    var letterFont = boldFont;
                    var answerFont = (item.SelectedAnswerIndex == i) || (this.ShowingAnswer && answer.IsCorrect) ? boldFont : this.Font;
                    Brush answerBrush = SystemBrushes.WindowText;
                    if (this.ShowingAnswer) {
                        letterFont = (answer.IsCorrect) ? boldFont : this.Font;
                        answerBrush = (answer.IsCorrect) ? SystemBrushes.WindowText : SystemBrushes.GrayText;
                    }

                    e.Graphics.DrawString(((char)('A' + i)).ToString(), letterFont, answerBrush, left, top, StringFormat.GenericTypographic);

                    int maxAnswerWidth = width - ((top < illustrationBottom) ? illustrationWidth : 0) - emSize.Width - SystemInformation.VerticalScrollBarWidth;
                    var answerTextSize = e.Graphics.MeasureString(answer.Text, answerFont, maxAnswerWidth, StringFormat.GenericDefault).ToSize();
                    var answerRectangle = new Rectangle(left + emSize.Width, top, answerTextSize.Width, answerTextSize.Height);
                    e.Graphics.DrawString(answer.Text, answerFont, answerBrush, answerRectangle, StringFormat.GenericTypographic);
                    //e.Graphics.DrawRectangle(Pens.Green, answerRectangle);
                    if (!this.ShowingAnswer) {
                        this.AnswerRectangles.Add(answerRectangle);
                    }

                    top += answerTextSize.Height + emSize.Height;
                }
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
