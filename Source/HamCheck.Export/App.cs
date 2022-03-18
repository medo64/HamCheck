using System;
using System.IO;

namespace HamCheck.Export {
    class App {
        static void Main(string[] args) {
            foreach (var exam in ExamElements.All) {
                using (var file = new FileStream(exam.Number.ToString() + ".html", FileMode.Create))
                using (var w = new StreamWriter(file)) {
                    foreach (var subelement in exam.Subelements) {
                        w.Write($"<h1 id=\"{subelement.Code.ToLowerInvariant()}\">");
                        w.Write(subelement.Code);
                        w.Write($"<br/>");
                        w.Write(subelement.Title);
                        w.WriteLine("</h1>");
                        w.WriteLine();

                        foreach (var group in subelement.Groups) {
                            w.Write($"<h2 id=\"{group.Code.ToLowerInvariant()}\">");
                            w.Write(group.Code);
                            w.Write($"<br/>");
                            w.Write(group.Title);
                            w.WriteLine("</h2>");
                            w.WriteLine();

                            foreach (var question in group.Questions) {
                                w.WriteLine(@"<div class=""item"">");
                                w.Write($"<h3 id=\"{question.Code.ToLowerInvariant()}\">");
                                w.Write(question.Code);
                                w.Write($"<br/>");
                                w.Write(CleanupText(question.Text));
                                w.WriteLine("</h3>");

                                if (question.Illustration != null) {
                                    var imgName = question.Illustration.Name.ToLowerInvariant() + ".png";
                                    w.WriteLine($"<img src=\"../images/{imgName}\" alt=\"{question.Illustration.Name}\" />");
                                    question.Illustration.Picture.Save(imgName);
                                }

                                var ch = 'A';
                                foreach (var answer in question.Answers) {
                                    if (answer.IsCorrect) {
                                        w.Write($"<p class=\"x\">");
                                    } else {
                                        w.Write($"<p class=\"o\">");
                                    }
                                    w.Write($"<span>{ch}: </span>");
                                    w.Write(CleanupText(answer.Text));
                                    w.WriteLine("</p>");
                                    ch++;
                                }

                                w.WriteLine(@"<p>");
                                w.WriteLine(@"</p>");

                                if (!string.IsNullOrEmpty(question.FccReference)) {
                                    w.Write($"<p class=\"note\">");
                                    w.Write($"FCC Part {question.FccReference}");
                                    w.WriteLine("</p>");
                                }

                                w.WriteLine(@"</div>");

                                w.WriteLine();
                            }
                        }
                    }
                }
            }
        }

        private static string CleanupText(string text) {
            return text.Replace("“", "\"").Replace("”", "\"").Replace("", "x");
        }

    }
}
