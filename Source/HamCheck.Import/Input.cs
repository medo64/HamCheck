using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace HamCheck.Import {
    internal static class Input {

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
            var filterHeader = true;

            var rawLines = File.ReadAllLines(txtFiles[0], Encoding.UTF8);
            for (var i = 0; i < rawLines.Length; i++) {
                var rawLine = rawLines[i];
                var line = rawLine
                    .Replace(" ", " ")
                    .Replace("\t", " ")
                    .Replace("–", "-")
                    .Replace("’", "'")
                    .Replace("“", "\"")
                    .Replace("”", "\"")
                    .Replace("", "x")
                    .Trim();
                if (string.IsNullOrEmpty(line)) { continue; }

                //skip until the first subelement
                if (filterHeader) {
                    if (line.StartsWith("SUBELEMENT ") && line.EndsWith("]")) {
                        filterHeader = false;
                    } else {
                        continue;
                    }
                }

                switch (state) {
                    case State.Default: {
                            if (line.StartsWith("SUBELEMENT ", StringComparison.OrdinalIgnoreCase)) {
                                var parsedCode = line.Substring(11, 2);
                                var parsedTitle = ExtractTitle(line);
                                element.Subelements.Add(new ExamSubelement(parsedCode, parsedTitle));
                            } else if (GroupRegex1.IsMatch(line)) {
                                if (element.Subelements.Count == 0) { throw new FormatException("Cannot find subelement for group near \"" + line + "\" (line " + (i + 1) + ")."); }
                                var subelement = element.Subelements[element.Subelements.Count - 1];
                                var parsedCode = line.Substring(0, 3);
                                var parsedTitle = line.Substring(6);
                                subelement.Groups.Add(new ExamGroup(parsedCode, parsedTitle));
                            } else if (GroupRegex2.IsMatch(line)) {
                                if (element.Subelements.Count == 0) { throw new FormatException("Cannot find subelement for group near \"" + line + "\" (line " + (i + 1) + ")."); }
                                var subelement = element.Subelements[element.Subelements.Count - 1];
                                var parsedCode = line.Substring(0, 3);
                                var parsedTitle = line.Substring(4);
                                subelement.Groups.Add(new ExamGroup(parsedCode, parsedTitle));
                            } else if (QuestionRegex.IsMatch(line)) {
                                parsedQuestionCode = line.Substring(0, 5);
                                parsedQuestionCorrectAnswer = line.Substring(7, 1);
                                parsedQuestionFccReference = line.Substring(9).Trim(new char[] { ' ', '[', ']' });
                                if (parsedQuestionFccReference.Length == 0) { parsedQuestionFccReference = null; }
                                parsedQuestionText = null;
                                state = State.Question;
                            } else if ((line.IndexOf("DELETED", StringComparison.Ordinal) >= 0) || (line.StartsWith("~", StringComparison.Ordinal))) {
                                //ignore deleted lines
                            } else if ((line.IndexOf("Question Removed", StringComparison.Ordinal) >= 0) || (line.StartsWith("~", StringComparison.Ordinal))) {
                                //ignore deleted lines
                            } else if (line.StartsWith("NOTE:", StringComparison.Ordinal)) {
                                //skip
                            } else if (line.StartsWith("END", StringComparison.InvariantCulture)) {
                                goto Done; //nothing else to do
                            } else if (line.StartsWith("~~~end", StringComparison.InvariantCultureIgnoreCase)) {
                                goto Done; //nothing else to do
                            } else if (line.StartsWith("~~~~end", StringComparison.InvariantCultureIgnoreCase)) {
                                goto Done; //nothing else to do
                            } else {
                                throw new FormatException("Unknown line format near \"" + line + "\" (line " + (i + 1) + ", file " + txtFiles[0] + ").");
                            }
                        }
                        break;

                    case State.Question: {
                            parsedQuestionText = line;
                            parsedAnswers = new ExamAnswers();
                            state = State.Answers;
                        }
                        break;

                    case State.Answers: {
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

            foreach (var question in element.GetExamQuestions()) {
                if (question.Answers.Count != 4) {
                    throw new FormatException("Wrong number of answers (" + question.Answers.Count.ToString() + ") near question " + question.Question.FccReference + ").");
                }
            }

Done:

            return element;
        }


        private static readonly Regex GroupRegex1 = new Regex(@"^[A-Z][0-9][A-Z] - ", RegexOptions.Compiled);
        private static readonly Regex GroupRegex2 = new Regex(@"^[A-Z][0-9][A-Z] ", RegexOptions.Compiled);
        private static readonly Regex QuestionRegex = new Regex(@"^[A-Z][0-9][A-Z][0-9][0-9] \([A-D]\)", RegexOptions.Compiled);
        private static readonly Regex AnswerRegex = new Regex(@"^[A-D]\. ", RegexOptions.Compiled);

        private enum State {
            Default,
            Question,
            Answers
        }


        private static readonly Dictionary<string, ExamIllustration> ImportBitmapCache = new Dictionary<string, ExamIllustration>();
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

            var bitmap = (Bitmap)Image.FromFile(imageFiles[0]);

            //find where content is in bitmap
            //this part is slow but I see no need to optimize as it gets used only for import
            int? left = null, right = null, top = null, bottom = null;
            for (var x = 0; x < bitmap.Width; x++) {
                for (var y = 0; y < bitmap.Height; y++) {
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
            for (var x = left.Value; x <= right.Value; x++) {
                for (var y = top.Value; y <= bottom.Value; y++) {
                    var color = bitmap.GetPixel(x, y);
                    var brightness = color.GetBrightness();
                    if (brightness <= 0.98) {
                        var alpha = (int)(255 - Math.Floor(brightness * 255));
                        if (alpha < 0) { alpha = 0; }
                        newBitmap.SetPixel(x - left.Value, y - top.Value, Color.FromArgb(alpha, Color.Black));
                    }
                }
            }
            //newBitmap.Save(imageFiles[0].Substring(0, imageFiles[0].Length - 4) + ".tmp.png");

            using (var ms = new MemoryStream()) {
                newBitmap.Save(ms, ImageFormat.Png);
                var illustration = new ExamIllustration(figureName, ms.ToArray());
                lock (ImportBitmapCacheSync) {
                    ImportBitmapCache.Add(cacheKey, illustration);
                }
                return illustration;
            }
        }

        private static string ExtractTitle(string line) {
            var args = line.Split(new char[] { '-' }, 2);
            var text = args[args.Length - 1].Trim();

            var indexOf = text.IndexOf(" - [", StringComparison.Ordinal);
            return (indexOf >= 0) ? text.Substring(0, indexOf) : text;
        }

    }
}
