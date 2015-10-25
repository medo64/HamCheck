using System;
using System.Drawing;

namespace HamCheck {

    /// <summary>
    /// Exam explanation.
    /// </summary>
    public class ExamExplanation {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="text">Explanation text.</param>
        /// <param name="illustration">Explanation illustration.</param>
        /// <exception cref="System.ArgumentNullException">Text cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Text cannot be empty.</exception>
        internal ExamExplanation(string text, Bitmap illustration) {
            if (text == null) { throw new ArgumentNullException("text", "Text cannot be null."); }
            text = text.Trim(); if (string.IsNullOrEmpty(text)) { throw new ArgumentNullException("text", "Text cannot be empty."); }

            this.Text = text;
            this.Illustration = illustration;
        }


        /// <summary>
        /// Gets explanation text.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Gets explanation illustration.
        /// </summary>
        public Bitmap Illustration { get; private set; }


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
