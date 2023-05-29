using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace HamCheck {

    /// <summary>
    /// Exam element.
    /// </summary>
    [DebuggerDisplay("{Number.ToString() + \": \" + Title}")]
    public class ExamElement {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="number">Element number.</param>
        /// <param name="title">Element title.</param>
        /// <param name="validFrom">Date exam is valid from. Only date portion is used.</param>
        /// <param name="validTo">Date exam stops being valid on. Only date portion is used.</param>
        /// <exception cref="System.ArgumentNullException">Title cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Element number must be between 2 and 4. -or- Title cannot be empty. -or- Exam must be valid for at least a day.</exception>
        internal ExamElement(int number, string title, DateTime validFrom, DateTime validTo) {
            if ((number < 2) || (number > 4)) { throw new ArgumentOutOfRangeException(nameof(number), "Element number must be between 2 and 4."); }
            if (title == null) { throw new ArgumentNullException(nameof(title), "Title cannot be null."); }
            title = title.Trim();
            if (string.IsNullOrEmpty(title)) { throw new ArgumentNullException(nameof(title), "Title cannot be empty."); }
            if (this.ValidFrom.Date > this.ValidTo.Date) { throw new ArgumentOutOfRangeException(nameof(validFrom), "Exam must be valid for at least a day."); }

            this.Number = number;
            this.Title = title;
            this.ValidFrom = validFrom.Date;
            this.ValidTo = validTo.Date.AddDays(1).AddTicks(-1);
            this.Illustrations = new ExamIllustrations();
            this.Subelements = new ExamSubelements();
        }


        /// <summary>
        /// Gets element number.
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Gets element title.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets date when exam starts.
        /// </summary>
        public DateTime ValidFrom { get; private set; }

        /// <summary>
        /// Gets date when exam ends.
        /// </summary>
        public DateTime ValidTo { get; private set; }

        /// <summary>
        /// Gets if exam is still valid.
        /// </summary>
        public bool IsValid {
            get { return ((DateTime.Now >= this.ValidFrom) && (DateTime.Now <= this.ValidTo)); }
        }

        /// <summary>
        /// Gets collection of illustrations.
        /// </summary>
        public ExamIllustrations Illustrations { get; private set; }

        /// <summary>
        /// Gets collection of subelements.
        /// </summary>
        public ExamSubelements Subelements { get; private set; }


        /// <summary>
        /// Gets pool size.
        /// </summary>
        public int PoolSize {
            get {
                var questionCount = 0;
                foreach (var subelement in this.Subelements) {
                    foreach (var group in subelement.Groups) {
                        questionCount += group.Questions.Count;
                    }
                }
                return questionCount;
            }
        }

        /// <summary>
        /// Gets a number of questions that will appear on FCC exam.
        /// </summary>
        public int DefaultQuestionsCount {
            get {
                var questionCount = 0;
                foreach (var subelement in this.Subelements) {
                    questionCount += subelement.Groups.Count;
                }
                return questionCount;
            }
        }

        /// <summary>
        /// Gets a number of correct answers needed for pass of FCC exam.
        /// </summary>
        public int DefaultMinimumCorrect {
            get {
                return (int)Math.Round(0.74 * this.DefaultQuestionsCount);
            }
        }


        #region Questions

        /// <summary>
        /// Returns exam questions.
        /// </summary>
        public ReadOnlyCollection<ExamItem> GetExamQuestions() {
            var items = new List<ExamItem>();

            foreach (var subelement in this.Subelements) {
                foreach (var group in subelement.Groups) {
#pragma warning disable CA5394 // Do not use insecure randomness
                    var question = group.Questions[Random.Next(group.Questions.Count)]; //one question from each group
#pragma warning restore CA5394 // Do not use insecure randomness
                    var answers = new List<ExamAnswer>(question.Answers);
                    RandomizeList(answers);
                    items.Add(new ExamItem(question, answers.AsReadOnly(), group));
                }
            }

            return items.AsReadOnly();
        }

        /// <summary>
        /// Returns randomized collection of questions.
        /// </summary>
        public ReadOnlyCollection<ExamItem> GetRandomizedQuestions() {
            var items = new List<ExamItem>();

            foreach (var subelement in this.Subelements) {
                foreach (var group in subelement.Groups) {
                    foreach (var question in group.Questions) {
                        var answers = new List<ExamAnswer>(question.Answers);
                        RandomizeList(answers);
                        items.Add(new ExamItem(question, answers.AsReadOnly(), group));
                    }
                }
            }

            RandomizeList(items);

            return items.AsReadOnly();
        }

        /// <summary>
        /// Returns all questions.
        /// </summary>
        public ReadOnlyCollection<ExamItem> GetAllQuestions() {
            var items = new List<ExamItem>();

            foreach (var subelement in this.Subelements) {
                foreach (var group in subelement.Groups) {
                    foreach (var question in group.Questions) {
                        var answers = new List<ExamAnswer>(question.Answers);
                        items.Add(new ExamItem(question, answers.AsReadOnly(), group));
                    }
                }
            }

            return items.AsReadOnly();
        }

        private static readonly Random Random = new();
        private static void RandomizeList<T>(IList<T> items) {
            for (var i = 0; i < items.Count; i++) {
#pragma warning disable CA5394 // Do not use insecure randomness
                var j = Random.Next(items.Count);
#pragma warning restore CA5394 // Do not use insecure randomness
                (items[j], items[i]) = (items[i], items[j]);
            }
        }

        #endregion


        #region Load

        /// <summary>
        /// Loads exam from stream.
        /// </summary>
        /// <param name="stream">Stream.</param>
        public static ExamElement Load(Stream stream) {
            if (stream == null) { throw new ArgumentNullException(nameof(stream), "Stream cannot be null."); }

            var xml = new XmlDocument();
            xml.Load(stream);

            var rootElement = xml.SelectSingleNode("/HamExam") ?? throw new FormatException("Invalid root element.");
            var elementNumber = int.Parse(GetValue(rootElement.Attributes["elementNumber"]), NumberStyles.Integer, CultureInfo.InvariantCulture);

            var elementTitle = GetValue(rootElement.Attributes["title"]) ?? throw new FormatException("Element doesn't have a title.");

            var elementValidFrom = DateTime.ParseExact(GetValue(rootElement.Attributes["validFrom"]), "yyyy-MM-dd", CultureInfo.InvariantCulture).Date;
            var elementValidTo = DateTime.ParseExact(GetValue(rootElement.Attributes["validTo"]), "yyyy-MM-dd", CultureInfo.InvariantCulture).Date;

            var exam = new ExamElement(elementNumber, elementTitle, elementValidFrom, elementValidTo);

            foreach (XmlElement xmlIllustration in xml.SelectNodes("HamExam/Illustrations/Illustration")) {
                var illustrationName = GetValue(xmlIllustration.Attributes["name"]) ?? throw new FormatException("Illustration name cannot be null.");
                var illustrationPictureBytes = Convert.FromBase64String(GetValue(xmlIllustration.Attributes["picture"]));
                var illustration = new ExamIllustration(illustrationName, illustrationPictureBytes);
                exam.Illustrations.Add(illustration);
            }

            foreach (XmlElement xmlSubelement in xml.SelectNodes("HamExam/Subelement")) {
                var subelementCode = GetValue(xmlSubelement.Attributes["code"]) ?? throw new FormatException("Subelement code cannot be null.");
                var subelementTitle = GetValue(xmlSubelement.Attributes["title"]) ?? throw new FormatException("Subelement title cannot be null.");
                var subelement = new ExamSubelement(subelementCode, subelementTitle);
                exam.Subelements.Add(subelement);

                foreach (XmlElement xmlGroup in xmlSubelement.SelectNodes("Group")) {
                    var groupCode = GetValue(xmlGroup.Attributes["code"]) ?? throw new FormatException("Group code cannot be null.");
                    var groupTitle = GetValue(xmlGroup.Attributes["title"]) ?? throw new FormatException("Group title cannot be null.");
                    var group = new ExamGroup(groupCode, groupTitle);
                    subelement.Groups.Add(group);

                    foreach (XmlElement xmlQuestion in xmlGroup.SelectNodes("Question")) {
                        var questionCode = GetValue(xmlQuestion.Attributes["code"]) ?? throw new FormatException("Question code cannot be null.");
                        var questionText = GetValue(xmlQuestion.Attributes["text"]) ?? throw new FormatException("Question text cannot be null.");
                        var questionFccReference = GetValue(xmlQuestion.Attributes["fccReference"]);
                        var questionIllustration = exam.Illustrations[GetValue(xmlQuestion.Attributes["illustration"])];

                        var explanationText = xmlQuestion.SelectSingleNode("Explanation")?.InnerText;
                        var explanation = (explanationText != null) ? new ExamExplanation(explanationText, null) : null;

                        var answers = new ExamAnswers();
                        foreach (XmlElement xmlAnswer in xmlQuestion.SelectNodes("Answer")) {
                            var answerText = GetValue(xmlAnswer.Attributes["text"]) ?? throw new FormatException("Answer text cannot be null.");
                            _ = bool.TryParse(GetValue(xmlAnswer.Attributes["isCorrect"]), out var answerIsCorrect);
                            var answer = new ExamAnswer(answerText, answerIsCorrect);
                            answers.Add(answer);
                        }

                        var question = new ExamQuestion(questionCode, questionText, questionIllustration, answers, questionFccReference, explanation);
                        group.Questions.Add(question);

                        if (question.Answers.Count != 4) { throw new ArgumentOutOfRangeException("answers", "There must be exactly four answers."); }
                        var correctCount = 0;
                        foreach (var answer in question.Answers) {
                            if (answer.IsCorrect) { correctCount++; }
                        }
                        if (correctCount != 1) { throw new ArgumentOutOfRangeException("answers", "There must be exactly one correct answer."); }
                    }
                }
            }

            return exam;
        }


        #endregion Load


        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object obj) {
            var other = obj as ExamElement;
            return (other != null) && (this.Number == other.Number);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        public override int GetHashCode() {
            return this.Number.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString() {
            return this.Title;
        }

        #endregion

        #region Private

        private static string? GetValue(XmlAttribute attribute, string? defaultValue = null) {
            return (attribute != null) ? attribute.Value : defaultValue;
        }

        #endregion Private

    }
}
