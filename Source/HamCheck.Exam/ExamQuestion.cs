using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

namespace HamCheck {

    /// <summary>
    /// Exam question.
    /// </summary>
    [DebuggerDisplay("{Code + \": \" + Text}")]
    public class ExamQuestion {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="code">Question code.</param>
        /// <param name="text">Question text.</param>
        /// <param name="illustration">Question illustration.</param>
        /// <param name="answers">Answers to a question.</param>
        /// <param name="fccReference">FCC reference text.</param>
        /// <param name="explanation">Explanation for the question.</param>
        /// <exception cref="ArgumentNullException">Code cannot be null. -or- Text cannot be null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Code must be exactly five characters in length. -or- Text cannot be empty. -or- There must be exactly four answers. -or- There must be exactly one correct answer. -or- FCC reference cannot be empty.</exception>
        internal ExamQuestion(string code, string text, ExamIllustration? illustration, ExamAnswers answers, string? fccReference, ExamExplanation? explanation) {
            if (code == null) { throw new ArgumentNullException(nameof(code), "Code cannot be null."); }
            code = code.Trim(); if (code.Length != 5) { throw new ArgumentOutOfRangeException(nameof(code), "Code must be exactly five characters in length."); }
            if (text == null) { throw new ArgumentNullException(nameof(text), "Text cannot be null."); }
            text = text.Trim(); if (string.IsNullOrEmpty(text)) { throw new ArgumentOutOfRangeException(nameof(text), "Text cannot be empty."); }
            if (answers == null) { throw new ArgumentNullException(nameof(answers), "Answers cannot be null."); }

            if (answers.Count != 4) { throw new ArgumentOutOfRangeException(nameof(answers), "There must be exactly four answers."); }
            var correctCount = 0;
            foreach (var answer in answers) {
                if (answer.IsCorrect) { correctCount++; }
            }
            if (correctCount != 1) { throw new ArgumentOutOfRangeException(nameof(answers), "There must be exactly one correct answer."); }

            if (fccReference != null) {
                fccReference = fccReference.Trim(); if (string.IsNullOrEmpty(fccReference)) { throw new ArgumentOutOfRangeException(nameof(fccReference), "FCC reference cannot be empty."); }
            }

            this.Code = code;
            this.Text = text;
            this.Illustration = illustration;
            this.Answers = answers;
            this.FccReference = fccReference;
            this.Explanation = explanation;
        }


        /// <summary>
        /// Gets question code.
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Gets question text.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Gets question illustration.
        /// </summary>
        public ExamIllustration? Illustration { get; private set; }

        /// <summary>
        /// Gets answers to a question.
        /// </summary>
        public ExamAnswers Answers { get; private set; }

        /// <summary>
        /// Gets question FCC reference text.
        /// </summary>
        public string? FccReference { get; private set; }


        /// <summary>
        /// Gets explanation for question.
        /// </summary>
        public ExamExplanation? Explanation { get; private set; }


        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object obj) {
            var other = obj as ExamQuestion;
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
            return this.Text;
        }

        #endregion

    }
}
