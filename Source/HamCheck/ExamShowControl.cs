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
                this.ShowingResults = false;

                this.Invalidate();
            }
        }


        public bool ShowAnswerAfterEveryQuestion { get; set; }
        public bool ShowResultsAfterQuestions { get; set; }


        #region Events

        protected override bool IsInputKey(Keys keyData) {
            switch (keyData) {
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                case Keys.Tab:
                    return true;
                default: return base.IsInputKey(keyData);
            }
        }

        private bool wasEscape = false;

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);

            if (e.KeyData == Keys.Escape) {
                if (wasEscape) {
                    var eh = this.GoBack;
                    if (eh != null) { eh.Invoke(this, new EventArgs()); }
                } else {
                    wasEscape = true;
                    return;
                }
            }
            wasEscape = false;

            switch (e.KeyData) {
                case Keys.Right:
                case Keys.Space:
                case Keys.Enter:
                    if (this.ShowAnswerAfterEveryQuestion && (this.LastAnswerIndex > this.ItemIndex)) { //show only answers once it has been shown
                        this.ShowingAnswer = true;
                        if (this.ItemIndex < this.Items.Count - 1) {
                            this.ItemIndex += 1;
                            this.Invalidate();
                        } else if (this.ShowResultsAfterQuestions) { //show results
                            this.ShowingResults = true;
                            this.ItemIndex = this.Items.Count;
                            this.Invalidate();
                        }
                    } else if (this.ShowAnswerAfterEveryQuestion && !this.ShowingAnswer) { //after question switch to showing its answer
                        var item = this.Items[this.ItemIndex];
                        if ((item.SelectedAnswerIndex == null) && (e.KeyData == Keys.Right)) { break; } //don't advance with Right key if question is not answered
                        this.LastAnswerIndex = Math.Max(this.LastAnswerIndex, this.ItemIndex);
                        this.ShowingAnswer = true;
                        this.Invalidate();
                    } else if (this.ItemIndex < this.Items.Count - 1) { //go to next question
                        this.ShowingAnswer = false;
                        this.ItemIndex += 1;
                        this.Invalidate();
                    } else if (!this.ShowAnswerAfterEveryQuestion && (this.ItemIndex == this.Items.Count - 1)) { //show results
                        this.ShowingResults = true;
                        this.ItemIndex = this.Items.Count;
                        this.LastAnswerIndex = this.Items.Count;
                        this.Invalidate();
                    } else if (this.ShowAnswerAfterEveryQuestion && this.ShowResultsAfterQuestions && this.ShowingAnswer && (this.ItemIndex == this.Items.Count - 1)) { //show results
                        this.ShowingResults = true;
                        this.ItemIndex = this.Items.Count;
                        this.LastAnswerIndex = this.Items.Count;
                        this.Invalidate();
                    }
                    break;

                case Keys.Left:
                    if (this.ShowAnswerAfterEveryQuestion) { //show only answers for already answered questions
                        this.ShowingAnswer = true;
                        if (this.ItemIndex > 0) {
                            this.ItemIndex -= 1;
                            this.Invalidate();
                        }
                    } else if (this.ItemIndex > 0) { //go to previous question
                        this.ShowingAnswer = false;
                        this.ItemIndex -= 1;
                        this.Invalidate();
                    }
                    break;

                case Keys.Home:
                    if (this.ItemIndex > 0) {
                        if (this.ShowAnswerAfterEveryQuestion) { this.ShowingAnswer = true; }
                        this.ItemIndex = 0;
                        this.Invalidate();
                    }
                    break;

                case Keys.End:
                    if (this.ShowingResults) { //go to result page but only in exam mode and if questions have been answered
                        this.ItemIndex = this.Items.Count;
                        this.Invalidate();
                    } else if (this.ShowAnswerAfterEveryQuestion && (this.LastAnswerIndex >= 0)) {
                        this.ItemIndex = this.LastAnswerIndex + 1;
                        this.ShowingAnswer = false;
                        this.Invalidate();
                    }
                    break;

                case Keys.Tab:
                    if (this.ShowingResults) { //go to next failed question; only in exam mode and if questions have been answered
                        var foundWrongAnswer = false;
                        var startAt = (this.ItemIndex == this.Items.Count) ? 0 : this.ItemIndex + 1;
                        for (int i = startAt; i < this.Items.Count; i++) {
                            var item = this.Items[i];
                            if ((item.SelectedAnswerIndex == null) || (!item.Answers[item.SelectedAnswerIndex.Value].IsCorrect)) {
                                this.ItemIndex = i;
                                foundWrongAnswer = true;
                                break;
                            }
                        }
                        if (!foundWrongAnswer) { this.ItemIndex = this.Items.Count; }
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
                        if (this.ShowAnswerAfterEveryQuestion && Settings.InstantAnswer) {
                            this.ShowingAnswer = true;
                        }
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
                        if (this.ShowAnswerAfterEveryQuestion && Settings.InstantAnswer) {
                            this.ShowingAnswer = true;
                        }
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
                        if (this.ShowAnswerAfterEveryQuestion && Settings.InstantAnswer) {
                            this.ShowingAnswer = true;
                        }
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
                        if (this.ShowAnswerAfterEveryQuestion && Settings.InstantAnswer) {
                            this.ShowingAnswer = true;
                        }
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
                    if (this.ShowingAnswer || (this.ItemIndex <= this.LastAnswerIndex)) { break; }
                    if ((this.Items != null) && (this.ItemIndex < this.Items.Count)) {
                        var item = this.Items[this.ItemIndex];
                        item.SelectedAnswerIndex = null;
                        this.Invalidate();
                    }
                    break;


                case Keys.Control | Keys.D0:
                case Keys.Control | Keys.NumPad0:
                case Keys.D0:
                case Keys.NumPad0:
                    Settings.ExamFontScale = Settings.DefaultFontScale;
                    this.Invalidate();
                    break;

                case Keys.Control | Keys.Add:
                case Keys.Control | Keys.Oemplus:
                case Keys.Add:
                case Keys.Oemplus:
                    Settings.ExamFontScale += 0.1F;
                    this.OnResize(null);
                    break;

                case Keys.Control | Keys.Subtract:
                case Keys.Control | Keys.OemMinus:
                case Keys.Subtract:
                case Keys.OemMinus:
                    Settings.ExamFontScale -= 0.1F;
                    this.OnResize(null);
                    break;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            var location = e.Location;
            location.Offset(-this.AutoScrollPosition.X, -this.AutoScrollPosition.Y);

            if (this.ShowingAnswer || (this.ItemIndex <= this.LastAnswerIndex)) { return; }
            if ((this.Items != null) && (this.ItemIndex < this.Items.Count)) {
                var item = this.Items[this.ItemIndex];
                for (int i = 0; i < this.AnswerHitRectangles.Count; i++) {
                    if (this.AnswerHitRectangles[i].Contains(location)) {
                        item.SelectedAnswerIndex = i;
                        this.Invalidate();
                        break;
                    }
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
            if (Control.ModifierKeys == Keys.Control) {
                var detents = e.Delta / SystemInformation.MouseWheelScrollDelta;
                Settings.ExamFontScale += detents / 10.0F;
                this.Invalidate();
            } else {
                base.OnMouseWheel(e);
            }
        }


        protected override void OnDoubleClick(EventArgs e) {
            base.OnDoubleClick(e);

            if (this.Items != null) {
                if (this.ShowingResults || this.ShowingAnswer) { //always move if there is no need to answer question
                    OnKeyDown(new KeyEventArgs(Keys.Enter));
                } else if (this.ItemIndex < this.Items.Count) {
                    var item = this.Items[this.ItemIndex];
                    if (item.SelectedAnswerIndex != null) { //move to next page if anything is selected
                        OnKeyDown(new KeyEventArgs(Keys.Enter));
                    }
                }
            }
        }


        private int ItemIndex;
        private int LastAnswerIndex;
        private bool ShowingAnswer;
        private bool ShowingResults;
        private List<Rectangle> AnswerHitRectangles = new List<Rectangle>();


        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            if (this.Items == null) { return; }

            using (var scaledFont = new Font(this.Font.FontFamily, this.Font.Size * Settings.ExamFontScale))
            using (var boldFont = new Font(scaledFont, FontStyle.Bold)) {
                this.AnswerHitRectangles.Clear();
                e.Graphics.TranslateTransform(this.AutoScrollPosition.X, this.AutoScrollPosition.Y);

                var emSize = e.Graphics.MeasureString("M", scaledFont).ToSize();

                var width = emSize.Width * 36;
                var height = emSize.Height * 20;

                var margin = Math.Max((SystemInformation.Border3DSize.Width + SystemInformation.Border3DSize.Height) / 2, emSize.Width / 4);

                var maxWidth = this.Width - margin * 2 - SystemInformation.VerticalScrollBarWidth;
                var maxHeight = this.Height - margin * 2;
                if (width > maxWidth) { width = maxWidth; }
                if (height > maxHeight) { height = maxHeight; }

                var left = (this.Width - (width + SystemInformation.VerticalScrollBarWidth)) / 2;
                var right = left + width;
                var top = (this.Height - height) / 2;
                var bottom = top + height;
                if (top < margin) { top = margin; }
                if (Settings.DebugShowHitBoxes) { e.Graphics.DrawRectangle(Pens.Gray, left, top, width, height); }


                if (this.ShowingResults && (this.ItemIndex == this.Items.Count)) { //to show results
                    int statTotal = this.Items.Count, statE = (int)Math.Ceiling(statTotal * 0.74), statD = (int)Math.Ceiling(statTotal * 0.8), statC = (int)Math.Ceiling(statTotal * 0.85), statB = (int)Math.Ceiling(statTotal * 0.9), statA = (int)Math.Ceiling(statTotal * 0.95);
                    int statCorrect = 0, statIncorrect = 0, statUnanswered = 0;

                    foreach (var itemStat in this.Items) {
                        if (itemStat.SelectedAnswerIndex == null) {
                            statUnanswered++;
                        } else if (itemStat.Answers[itemStat.SelectedAnswerIndex.Value].IsCorrect) {
                            statCorrect++;
                        } else {
                            statIncorrect++;
                        }
                    }

                    var maxStatTitlesWidth = 0;
                    var statTop = top;
                    foreach (var statTitle in new string[] { "Total questions:", "Needed to pass:", "", "Correct:", "Incorrect:", "Unanswered:" }) {
                        if (!string.IsNullOrEmpty(statTitle)) {
                            int maxTitleWidth = width / 2;
                            var statTitleSize = e.Graphics.MeasureString(statTitle, scaledFont, maxTitleWidth, new StringFormat(StringFormat.GenericDefault) { Trimming = StringTrimming.EllipsisCharacter }).ToSize();
                            var statTitleRectange = new Rectangle(left, statTop, statTitleSize.Width, emSize.Height);
                            if (Settings.DebugShowHitBoxes) { e.Graphics.DrawRectangle(Pens.Magenta, statTitleRectange); }
                            e.Graphics.DrawString(statTitle, scaledFont, SystemBrushes.WindowText, statTitleRectange, new StringFormat(StringFormat.GenericTypographic) { Trimming = StringTrimming.EllipsisCharacter });
                            if (maxStatTitlesWidth < statTitleRectange.Width) { maxStatTitlesWidth = statTitleRectange.Width; }
                        }
                        statTop += emSize.Height;
                    }

                    statTop = top;
                    foreach (var statResult in new string[] { statTotal.ToString(CultureInfo.CurrentCulture), statE.ToString(CultureInfo.CurrentCulture), "", statCorrect.ToString(CultureInfo.CurrentCulture), statIncorrect.ToString(CultureInfo.CurrentCulture), statUnanswered.ToString(CultureInfo.CurrentCulture) }) {
                        if (!string.IsNullOrEmpty(statResult)) {
                            int maxTitleWidth = width / 2;
                            var statResultRectangle = new Rectangle(left + maxStatTitlesWidth, statTop, emSize.Width * 2, emSize.Height);
                            if (Settings.DebugShowHitBoxes) { e.Graphics.DrawRectangle(Pens.Cyan, statResultRectangle); }
                            e.Graphics.DrawString(statResult, boldFont, SystemBrushes.WindowText, statResultRectangle, new StringFormat(StringFormat.GenericTypographic) { Alignment = StringAlignment.Far });
                        }
                        statTop += emSize.Height;
                    }

                    using (var verdictFont = new Font(scaledFont.FontFamily, scaledFont.Size * 4F, FontStyle.Bold)) {
                        var verdictFormat = new StringFormat(StringFormat.GenericTypographic) { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        var verdictRectangle = new Rectangle(left + maxStatTitlesWidth + emSize.Width * 2, top, width - maxStatTitlesWidth - emSize.Width * 2, statTop - top);
                        if (Settings.DebugShowHitBoxes) { e.Graphics.DrawRectangle(Pens.Yellow, verdictRectangle); }

                        if (Settings.ShowGrade) {
                            if (statCorrect >= statA) {
                                e.Graphics.DrawString("A", verdictFont, SystemBrushes.WindowText, verdictRectangle, verdictFormat);
                            } else if (statCorrect >= statB) {
                                e.Graphics.DrawString("B", verdictFont, SystemBrushes.WindowText, verdictRectangle, verdictFormat);
                            } else if (statCorrect >= statC) {
                                e.Graphics.DrawString("C", verdictFont, SystemBrushes.WindowText, verdictRectangle, verdictFormat);
                            } else if (statCorrect >= statD) {
                                e.Graphics.DrawString("D", verdictFont, SystemBrushes.WindowText, verdictRectangle, verdictFormat);
                            } else if (statCorrect >= statE) {
                                e.Graphics.DrawString("E", verdictFont, SystemBrushes.WindowText, verdictRectangle, verdictFormat);
                            } else {
                                e.Graphics.DrawString("F", verdictFont, SystemBrushes.GrayText, verdictRectangle, verdictFormat);
                            }
                        } else if (statCorrect >= statE) {
                            e.Graphics.DrawString("Pass", verdictFont, SystemBrushes.WindowText, verdictRectangle, verdictFormat);
                        } else {
                            e.Graphics.DrawString("Fail", verdictFont, SystemBrushes.GrayText, verdictRectangle, verdictFormat);
                        }
                    }

                    statTop += margin;

                    var newSizeResults = new Size(width, statTop);
                    if (this.AutoScrollMinSize != newSizeResults) { this.AutoScrollMinSize = newSizeResults; }
                    return;
                }


                var item = this.Items[this.ItemIndex];

                var questionCodeRectange = new Rectangle(left, top, emSize.Width * 3, emSize.Height);
                if (Settings.DebugShowHitBoxes) { e.Graphics.DrawRectangle(Pens.Pink, questionCodeRectange); }
                e.Graphics.DrawString(item.Question.Code, scaledFont, SystemBrushes.GrayText, questionCodeRectange, StringFormat.GenericTypographic);

                var questionNumberRectange = new Rectangle(left + width - emSize.Width * 3, top, emSize.Width * 3, emSize.Height);
                if (Settings.DebugShowHitBoxes) { e.Graphics.DrawRectangle(Pens.Pink, questionNumberRectange); }
                var questionNumberText = string.Format(CultureInfo.CurrentCulture, "{0}/{1}", this.ItemIndex + 1, this.Items.Count);
                e.Graphics.DrawString(questionNumberText, scaledFont, SystemBrushes.GrayText, questionNumberRectange, new StringFormat(StringFormat.GenericTypographic) { Alignment = StringAlignment.Far });

                if (this.ShowingAnswer || this.ShowingResults) {
                    var titleRectange = new Rectangle(left + emSize.Width * 4, top, width - emSize.Width * 8, emSize.Height);
                    if (Settings.DebugShowHitBoxes) { e.Graphics.DrawRectangle(Pens.Pink, titleRectange); }
                    string titleText;
                    if (item.SelectedAnswerIndex == null) {
                        titleText = "Not answered";
                    } else if (item.Answers[item.SelectedAnswerIndex.Value].IsCorrect) {
                        titleText = "Correct answer";
                    } else {
                        titleText = "Incorrect answer";
                    }
                    e.Graphics.DrawString(titleText, scaledFont, SystemBrushes.GrayText, titleRectange, new StringFormat(StringFormat.GenericTypographic) { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisWord });
                }

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
                    var imageLeft = left + width - imageWidth;
                    var imageTop = top;
                    var imageRectange = new Rectangle(imageLeft, imageTop, imageWidth, imageHeight);

                    e.Graphics.DrawImage(item.Question.Illustration.Picture, imageRectange);
                    if (Settings.DebugShowHitBoxes) { e.Graphics.DrawRectangle(Pens.Red, imageRectange); }

                    illustrationBottom = imageRectange.Bottom + emSize.Height / 4;
                    illustrationWidth = imageRectange.Width + emSize.Height / 4;
                }

                var explanationFont = new Font(scaledFont.FontFamily, scaledFont.Size * 1.2F);
                int maxQuestionWidth = width - ((top < illustrationBottom) ? illustrationWidth : 0);
                var questionTextSize = e.Graphics.MeasureString(item.Question.Text, explanationFont, maxQuestionWidth, StringFormat.GenericDefault).ToSize();
                var questionRectange = new Rectangle(left, top, questionTextSize.Width, questionTextSize.Height);
                e.Graphics.DrawString(item.Question.Text, explanationFont, SystemBrushes.WindowText, questionRectange, StringFormat.GenericTypographic);
                if (Settings.DebugShowHitBoxes) { e.Graphics.DrawRectangle(Pens.Blue, questionRectange); }

                top += questionTextSize.Height + emSize.Height;

                for (int i = 0; i < item.Answers.Count; i++) {
                    var answer = item.Answers[i];

                    var letterFont = boldFont;
                    var answerFont = (item.SelectedAnswerIndex == i) || ((this.ShowingAnswer || this.ShowingResults) && answer.IsCorrect) ? boldFont : scaledFont;
                    Brush answerBrush = SystemBrushes.WindowText;
                    if (this.ShowingAnswer || this.ShowingResults) {
                        letterFont = (answer.IsCorrect) ? boldFont : scaledFont;
                        answerBrush = (answer.IsCorrect) ? SystemBrushes.WindowText : SystemBrushes.GrayText;
                    }

                    e.Graphics.DrawString(((char)('A' + i)).ToString(), letterFont, answerBrush, left, top, StringFormat.GenericTypographic);

                    int maxAnswerWidth = width - ((top < illustrationBottom) ? illustrationWidth : 0) - emSize.Width;
                    var answerTextSize = e.Graphics.MeasureString(answer.Text, answerFont, maxAnswerWidth, StringFormat.GenericDefault).ToSize();
                    var answerRectangle = new Rectangle(left + emSize.Width, top, answerTextSize.Width, answerTextSize.Height);
                    e.Graphics.DrawString(answer.Text, answerFont, answerBrush, answerRectangle, StringFormat.GenericTypographic);
                    if (Settings.DebugShowHitBoxes) { e.Graphics.DrawRectangle(Pens.Green, answerRectangle); }
                    if (!this.ShowingAnswer && !this.ShowingResults) {
                        var answerHitRectange = new Rectangle(answerRectangle.Left - emSize.Width, answerRectangle.Top, answerRectangle.Width + emSize.Width, answerRectangle.Height);
                        this.AnswerHitRectangles.Add(answerHitRectange);
                    }

                    top += answerTextSize.Height + emSize.Height;
                }

                var explanationText = item.Question.Explanation?.Text;
                if (this.ShowingAnswer || this.ShowingResults) {
                    using (explanationFont = new Font(scaledFont.FontFamily, scaledFont.Size * 1.2F, FontStyle.Italic)) {
                        int maxExplanationWidth = width - ((top < illustrationBottom) ? illustrationWidth : 0);
                        var explanationTextSize = e.Graphics.MeasureString(explanationText, explanationFont, maxExplanationWidth, StringFormat.GenericDefault).ToSize();
                        var explanationRectangle = new Rectangle(left, top, explanationTextSize.Width, explanationTextSize.Height);
                        e.Graphics.DrawString(explanationText, explanationFont, SystemBrushes.WindowText, explanationRectangle, StringFormat.GenericTypographic);
                        if (Settings.DebugShowHitBoxes) { e.Graphics.DrawRectangle(Pens.Beige, explanationRectangle); }
                        top += explanationTextSize.Height + emSize.Height;
                    }
                }

                top -= emSize.Height;
                top += margin;

                var newSize = new Size(width, top);
                if (this.AutoScrollMinSize != newSize) { this.AutoScrollMinSize = newSize; }
            }
        }

        #endregion

        public event EventHandler<EventArgs> GoBack;

    }
}
