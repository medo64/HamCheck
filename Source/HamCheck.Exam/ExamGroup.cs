using System;
using System.Diagnostics;
using System.Globalization;

namespace HamCheck {

    /// <summary>
    /// Exam group.
    /// </summary>
    [DebuggerDisplay("{Code + \" - \" + Title}")]
    public class ExamGroup {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="code">Group code.</param>
        /// <param name="title">Group title.</param>
        /// <exception cref="System.ArgumentNullException">Code cannot be null. -or- Title cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Code must be exactly three characters in length. -or- Title cannot be empty.</exception>
        internal ExamGroup(string code, string title) {
            if (code == null) { throw new ArgumentNullException(nameof(code), "Code cannot be null."); }
            code = code.Trim(); if (code.Length != 3) { throw new ArgumentNullException(nameof(code), "Code must be exactly three characters in length."); }
            if (title == null) { throw new ArgumentNullException(nameof(title), "Title cannot be null."); }
            title = title.Trim(); if (string.IsNullOrEmpty(title)) { throw new ArgumentNullException(nameof(title), "Title cannot be empty."); }

            this.Code = code;
            this.Title = title;
            this.Questions = new ExamQuestions();
        }


        /// <summary>
        /// Gets group code.
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Gets group title.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets collection of questions.
        /// </summary>
        public ExamQuestions Questions { get; private set; }


        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object obj) {
            var other = obj as ExamGroup;
            return (other != null) && (this.Code.Equals(other.Code, StringComparison.Ordinal));
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        public override int GetHashCode() {
            return this.Code.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString() {
            return this.Title;
        }

        #endregion

    }
}
