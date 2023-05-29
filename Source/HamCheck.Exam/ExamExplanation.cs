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
        /// <param name="illustrationBytes">Explanation illustration.</param>
        /// <exception cref="ArgumentNullException">Text cannot be null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Text cannot be empty.</exception>
        internal ExamExplanation(string text, byte[]? illustrationBytes) {
            if (text == null) { throw new ArgumentNullException(nameof(text), "Text cannot be null."); }
            text = text.Trim(); if (string.IsNullOrEmpty(text)) { throw new ArgumentNullException(nameof(text), "Text cannot be empty."); }

            this.Text = text;
            this.IllustrationBytes = illustrationBytes;
        }


        /// <summary>
        /// Gets explanation text.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Gets explanation illustration.
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[]? IllustrationBytes { get; private set; }
#pragma warning restore CA1819 // Properties should not return arrays


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
