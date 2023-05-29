using System;

namespace HamCheck {

    /// <summary>
    /// Exam answer.
    /// </summary>
    public class ExamAnswer {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="text">Answer text.</param>
        /// <param name="isCorrect">True if this is correct answer.</param>
        /// <exception cref="System.ArgumentNullException">Text cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Text cannot be empty.</exception>
        internal ExamAnswer(string text, bool isCorrect) {
            if (text == null) { throw new ArgumentNullException(nameof(text), "Text cannot be null."); }
            text = text.Trim(); if (string.IsNullOrEmpty(text)) { throw new ArgumentNullException(nameof(text), "Text cannot be empty."); }

            this.Text = text;
            this.IsCorrect = isCorrect;
        }


        /// <summary>
        /// Gets answer text.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Gets if answer is correct.
        /// </summary>
        public bool IsCorrect { get; private set; }


        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString() {
            return this.Text;
        }

        #endregion

    }
}
