using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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
            if ((number < 2) || (number > 4)) { throw new ArgumentOutOfRangeException("number", "Element number must be between 2 and 4."); }
            if (title == null) { throw new ArgumentNullException("title", "Title cannot be null."); }
            title = title.Trim(); if (string.IsNullOrEmpty(title)) { throw new ArgumentNullException("title", "Title cannot be empty."); }
            if (this.ValidFrom.Date > this.ValidTo.Date) { throw new ArgumentOutOfRangeException("validFrom", "Exam must be valid for at least a day."); }

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

        #region Load/Save

        /// <summary>
        /// Loads exam from stream.
        /// </summary>
        /// <param name="stream">Stream.</param>
        public static ExamElement Load(Stream stream) {
            if (stream == null) { throw new ArgumentNullException(nameof(stream), "Stream cannot be null."); }

            var xml = new XmlDocument();
            xml.Load(stream);

            var rootElement = xml.SelectSingleNode("/HamExam");
            if (rootElement == null) { throw new FormatException("Invalid root element."); }
            var elementNumber = int.Parse(GetValue(rootElement.Attributes["elementNumber"]), NumberStyles.Integer, CultureInfo.InvariantCulture);
            var elementTitle = GetValue(rootElement.Attributes["title"]);
            var elementValidFrom = DateTime.ParseExact(GetValue(rootElement.Attributes["validFrom"]), "yyyy-MM-dd", CultureInfo.InvariantCulture).Date;
            var elementValidTo = DateTime.ParseExact(GetValue(rootElement.Attributes["validTo"]), "yyyy-MM-dd", CultureInfo.InvariantCulture).Date;

            var exam = new ExamElement(elementNumber, elementTitle, elementValidFrom, elementValidTo);

            foreach (XmlElement xmlIllustration in xml.SelectNodes("HamExam/Illustrations/Illustration")) {
                var illustrationName = GetValue(xmlIllustration.Attributes["name"]);
                var illustrationPictureBytes = Convert.FromBase64String(GetValue(xmlIllustration.Attributes["picture"]));
                using (var illustrationPictureStream = new MemoryStream(illustrationPictureBytes)) {
                    var illustrationPicture = new Bitmap(illustrationPictureStream);
                    var illustration = new ExamIllustration(illustrationName, illustrationPicture);
                    exam.Illustrations.Add(illustration);
                }
            }

            foreach (XmlElement xmlSubelement in xml.SelectNodes("HamExam/Subelement")) {
                var subelementCode = GetValue(xmlSubelement.Attributes["code"]);
                var subelementTitle = GetValue(xmlSubelement.Attributes["title"]);
                var subelement = new ExamSubelement(subelementCode, subelementTitle);
                exam.Subelements.Add(subelement);

                foreach (XmlElement xmlGroup in xmlSubelement.SelectNodes("Group")) {
                    var groupCode = GetValue(xmlGroup.Attributes["code"]);
                    var groupTitle = GetValue(xmlGroup.Attributes["title"]);
                    var group = new ExamGroup(groupCode, groupTitle);
                    subelement.Groups.Add(group);

                    foreach (XmlElement xmlQuestion in xmlGroup.SelectNodes("Question")) {
                        var questionCode = GetValue(xmlQuestion.Attributes["code"]);
                        var questionText = GetValue(xmlQuestion.Attributes["text"]);
                        var questionFccReference = GetValue(xmlQuestion.Attributes["fccReference"]);
                        var questionIllustration = exam.Illustrations[GetValue(xmlQuestion.Attributes["illustration"])];
                        var question = new ExamQuestion(questionCode, questionText, questionIllustration, null, questionFccReference, null);
                        group.Questions.Add(question);

                        foreach (XmlElement xmlAnswer in xmlQuestion.SelectNodes("Answer")) {
                            var answerText = GetValue(xmlAnswer.Attributes["text"]);
                            bool answerIsCorrect = false; bool.TryParse(GetValue(xmlAnswer.Attributes["isCorrect"]), out answerIsCorrect);
                            var answer = new ExamAnswer(answerText, answerIsCorrect);
                            question.Answers.Add(answer);
                        }

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

        /// <summary>
        /// Saves exam to a stream.
        /// </summary>
        /// <param name="stream">Stream.</param>
        public void Save(Stream stream) {
            using (var xml = new XmlTextWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)) { Formatting = Formatting.Indented }) {
                xml.WriteStartDocument();
                xml.WriteStartElement("HamExam");
                xml.WriteAttributeString("elementNumber", this.Number.ToString(CultureInfo.InvariantCulture));
                xml.WriteAttributeString("title", this.Title.ToString(CultureInfo.InvariantCulture));
                xml.WriteAttributeString("validFrom", this.ValidFrom.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                xml.WriteAttributeString("validTo", this.ValidTo.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

                xml.WriteStartElement("Illustrations");
                foreach (var illustration in this.Illustrations) {
                    xml.WriteStartElement("Illustration");
                    xml.WriteAttributeString("name", illustration.Name);
                    using (var ms = new MemoryStream(65536)) {
                        illustration.Picture.Save(ms, ImageFormat.Png);
                        xml.WriteAttributeString("picture", Convert.ToBase64String(ms.ToArray()));
                    }
                    xml.WriteEndElement();
                }
                xml.WriteEndElement();

                foreach (var subelement in this.Subelements) {
                    xml.WriteStartElement("Subelement");
                    xml.WriteAttributeString("code", subelement.Code);
                    xml.WriteAttributeString("title", subelement.Title);

                    foreach (var group in subelement.Groups) {
                        xml.WriteStartElement("Group");
                        xml.WriteAttributeString("code", group.Code);
                        xml.WriteAttributeString("title", group.Title);

                        foreach (var question in group.Questions) {
                            xml.WriteStartElement("Question");
                            xml.WriteAttributeString("code", question.Code);
                            xml.WriteAttributeString("text", question.Text);
                            if (question.Illustration != null) {
                                xml.WriteAttributeString("illustration", question.Illustration.Name);
                            }
                            if (question.FccReference != null) {
                                xml.WriteAttributeString("fccReference", question.FccReference);
                            }

                            foreach (var answer in question.Answers) {
                                xml.WriteStartElement("Answer");
                                xml.WriteAttributeString("text", answer.Text);
                                if (answer.IsCorrect) { xml.WriteAttributeString("isCorrect", "true"); }
                                xml.WriteEndElement();
                            }

                            if (question.Explanation != null) {
                                xml.WriteStartElement("Explanation");
                                xml.WriteAttributeString("text", question.Explanation.Text);
                                if (question.Explanation.Illustration != null) {
                                    using (var ms = new MemoryStream(65536)) {
                                        question.Explanation.Illustration.Save(ms, ImageFormat.Png);
                                        xml.WriteAttributeString("illustration", Convert.ToBase64String(ms.ToArray()));
                                    }
                                }
                                xml.WriteEndElement();
                            }

                            xml.WriteEndElement();
                        }

                        xml.WriteEndElement();
                    }

                    xml.WriteEndElement();
                }

                xml.WriteEndElement();
            }
        }

        #endregion


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


        #region ImportFromText

        /// <summary>
        /// Imports element from text document.
        /// </summary>
        /// <param name="number">Element number.</param>
        /// <param name="title">Element title.</param>
        /// <param name="validFrom">Date exam is valid from. Only date portion is used.</param>
        /// <param name="validTo">Date exam stops being valid on. Only date portion is used.</param>
        /// <param name="path">Path to the directory where text and figures (if any) are stored.</param>
        /// <exception cref="System.FormatException"></exception>
        public static ExamElement ImportFromText(int number, string title, DateTime validFrom, DateTime validTo, string path) {
            var element = new ExamElement(number, title, validFrom, validTo);

            var txtFiles = Directory.GetFiles(path, "*.txt");
            if (txtFiles.Length != 1) { throw new FormatException(string.Format(CultureInfo.InvariantCulture, "Cannot determine which text file to use (out of {0}).", txtFiles.Length)); }

            var state = State.Default;

            string parsedQuestionCode = null;
            string parsedQuestionCorrectAnswer = null;
            string parsedQuestionFccReference = null;
            string parsedQuestionText = null;
            ExamAnswers parsedAnswers = null;

            var rawLines = File.ReadAllLines(txtFiles[0], Encoding.Default);
            for (int i = 0; i < rawLines.Length; i++) {
                var rawLine = rawLines[i];
                var line = rawLine.Replace(" ", " ").Replace("–", "-").Trim();
                if (string.IsNullOrEmpty(line)) { continue; }

                switch (state) {
                    case State.Default:
                        {
                            if (line.StartsWith("SUBELEMENT ", StringComparison.OrdinalIgnoreCase)) {
                                var parsedCode = line.Substring(11, 2);
                                var parsedTitle = ExtractTitle(line);
                                element.Subelements.Add(new ExamSubelement(parsedCode, parsedTitle));
                            } else if (GroupRegex.IsMatch(line)) {
                                if (element.Subelements.Count == 0) { throw new FormatException("Cannot find subelement for group near \"" + line + "\" (line " + (i + 1) + ")."); }
                                var subelement = element.Subelements[element.Subelements.Count - 1];
                                var parsedCode = line.Substring(0, 3);
                                var parsedTitle = line.Substring(6);
                                subelement.Groups.Add(new ExamGroup(parsedCode, parsedTitle));
                            } else if (QuestionRegex.IsMatch(line)) {
                                parsedQuestionCode = line.Substring(0, 5);
                                parsedQuestionCorrectAnswer = line.Substring(7, 1);
                                parsedQuestionFccReference = line.Substring(9).Trim(new char[] { ' ', '[', ']' });
                                if (parsedQuestionFccReference.Length == 0) { parsedQuestionFccReference = null; }
                                parsedQuestionText = null;
                                state = State.Question;
                            } else {
                                throw new FormatException("Unknown line format near \"" + line + "\" (line " + (i + 1) + ").");
                            }
                        }
                        break;

                    case State.Question:
                        {
                            parsedQuestionText = line;
                            parsedAnswers = new ExamAnswers();
                            state = State.Answers;
                        }
                        break;

                    case State.Answers:
                        {
                            if (AnswerRegex.IsMatch(line)) {
                                var parsedText = line.Substring(3);
                                var parsedIsCorrect = line.Substring(0, 1).Equals(parsedQuestionCorrectAnswer, StringComparison.OrdinalIgnoreCase);
                                parsedAnswers.Add(new ExamAnswer(parsedText, parsedIsCorrect));
                            } else if (line.StartsWith("~", StringComparison.OrdinalIgnoreCase)) {
                                if (element.Subelements.Count == 0) { throw new FormatException("Cannot find subelement for group near \"" + line + "\" (line " + (i + 1) + ")."); }
                                var subelement = element.Subelements[element.Subelements.Count - 1];
                                if (subelement.Groups.Count == 0) { throw new FormatException("Cannot find group for question near \"" + line + "\" (line " + (i + 1) + ")."); }
                                var group = subelement.Groups[subelement.Groups.Count - 1];
                                var illustrationIndex = parsedQuestionText.IndexOf(" figure ", StringComparison.OrdinalIgnoreCase);
                                ExamIllustration parsedIllustration = null;
                                if (illustrationIndex >= 0) {
                                    var firstSpaceAfterIllustrationIndex = parsedQuestionText.IndexOfAny(new char[] { ' ', ',', '?' }, illustrationIndex + 8);
                                    var figureName = parsedQuestionText.Substring(illustrationIndex + 8, firstSpaceAfterIllustrationIndex - illustrationIndex - 8);
                                    if (figureName.StartsWith(parsedQuestionCode.Substring(0, 1), StringComparison.Ordinal)) {
                                        parsedIllustration = ExtractAndTrimIllustration(path, figureName, i + 1);
                                        if (!element.Illustrations.Contains(parsedIllustration.Name)) {
                                            element.Illustrations.Add(parsedIllustration);
                                        }
                                    }
                                }
                                var parsedQuestion = new ExamQuestion(parsedQuestionCode, parsedQuestionText, parsedIllustration, parsedAnswers, parsedQuestionFccReference, null);
                                group.Questions.Add(parsedQuestion);
                                state = State.Default;
                            } else {
                                throw new FormatException("Unknown answer format near \"" + line + "\" (line " + (i + 1) + ").");
                            }
                        }
                        break;

                    default:
                        throw new FormatException("Unknown state " + state.ToString() + " near \"" + line + "\" (line " + (i + 1) + ").");
                }
            }


            return element;
        }


        private static Regex GroupRegex = new Regex(@"^[A-Z][0-9][A-Z] - ", RegexOptions.Compiled);
        private static Regex QuestionRegex = new Regex(@"^[A-Z][0-9][A-Z][0-9][0-9] \([A-D]\)", RegexOptions.Compiled);
        private static Regex AnswerRegex = new Regex(@"^[A-D]\. ", RegexOptions.Compiled);

        private enum State {
            Default,
            Question,
            Answers
        }


        private static Dictionary<string, ExamIllustration> ImportBitmapCache = new Dictionary<string, ExamIllustration>();
        private static readonly object ImportBitmapCacheSync = new object();

        private static ExamIllustration ExtractAndTrimIllustration(string path, string figureName, int lineNumber) {
            var cacheKey = path + "\0" + figureName;
            lock (ImportBitmapCacheSync) {
                if (ImportBitmapCache.ContainsKey(cacheKey)) {
                    return ImportBitmapCache[cacheKey];
                }
            }

            var imageFiles = Directory.GetFiles(path, figureName + ".*");
            if (imageFiles.Length != 1) { throw new FormatException(string.Format(CultureInfo.InvariantCulture, "Cannot determine which image file to use (searching {0} at line {1}).", figureName, lineNumber)); }

            var bitmap = (Bitmap)Bitmap.FromFile(imageFiles[0]);

            //find where content is in bitmap
            //this part is slow but I see no need to optimize as it gets used only for import
            int? left = null, right = null, top = null, bottom = null;
            for (int x = 0; x < bitmap.Width; x++) {
                for (int y = 0; y < bitmap.Height; y++) {
                    var isTransparent = bitmap.GetPixel(x, y).GetBrightness() > 0.98;
                    if (!isTransparent) {
                        if ((left == null) || (x < left)) { left = x; }
                        if ((top == null) || (y < top)) { top = y; }
                        if ((right == null) || (x > right)) { right = x; }
                        if ((bottom == null) || (y > bottom)) { bottom = y; }
                    }
                }
            }

            //make new bitmap with content only and transparent background
            //this part is slow but I see no need to optimize as it gets used only for import
            int width = right.Value - left.Value + 1, height = bottom.Value - top.Value + 1;
            var newBitmap = new Bitmap(width, height);
            for (int x = left.Value; x <= right.Value; x++) {
                for (int y = top.Value; y <= bottom.Value; y++) {
                    var color = bitmap.GetPixel(x, y);
                    if (color.GetHue() == 0) {
                        var brightness = color.GetBrightness();
                        if (brightness <= 0.98) {
                            var alpha = (int)(255 - Math.Floor(brightness * 255));
                            newBitmap.SetPixel(x - left.Value, y - top.Value, Color.FromArgb(alpha, Color.Black));
                        }
                    }
                }
            }

            var illustration = new ExamIllustration(figureName, newBitmap);
            lock (ImportBitmapCacheSync) {
                ImportBitmapCache.Add(cacheKey, illustration);
            }
            return illustration;
        }

        private static string ExtractTitle(string line) {
            var args = line.Split(new char[] { '-' }, 2);
            var text = args[args.Length - 1].Trim();

            var indexOf = text.IndexOf(" - [", StringComparison.Ordinal);
            return (indexOf >= 0) ? text.Substring(0, indexOf) : text;
        }

        private static string GetValue(XmlAttribute attribute, string defaultValue = null) {
            return (attribute != null) ? attribute.Value : defaultValue;
        }

        #endregion

    }
}
