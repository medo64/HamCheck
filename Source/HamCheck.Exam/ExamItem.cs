using System.Collections.ObjectModel;
using System.Diagnostics;

namespace HamCheck {
    /// <summary>
    /// Question with answer.
    /// </summary>
    [DebuggerDisplay("{Question.Text}")]
    public class ExamItem {

        internal ExamItem(ExamQuestion question, ReadOnlyCollection<ExamAnswer> answers, ExamGroup group) {
            this.Question = question;
            this.Answers = answers;
            this.Group = group;
        }

        /// <summary>
        /// Gets question.
        /// </summary>
        public ExamQuestion Question { get; }

        /// <summary>
        /// Gets list of answers.
        /// </summary>
        public ReadOnlyCollection<ExamAnswer> Answers { get; }

        /// <summary>
        /// Gets group question belongs to.
        /// </summary>
        public ExamGroup Group { get; }

        /// <summary>
        /// Gets user-selected answer.
        /// </summary>
        public int SelectedAnswerIndex { get; set; }

    }
}
