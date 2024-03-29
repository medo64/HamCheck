using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace HamCheck.Import {
    internal class App {

        internal static void Main() {
            try {
                var exams = new List<ExamElement> {
                    Input.ImportFromText(2,
                                         "Technician Class Question Pool",
                                         new DateTime(2022, 7, 1),
                                         new DateTime(2026, 6, 30),
                                         @"..\Reference\2 Technician [2022-2026] {20220307}"),

                    Input.ImportFromText(3,
                                         "General Class Question Pool",
                                         new DateTime(2023, 7, 1),
                                         new DateTime(2027, 6, 30),
                                         @"..\Reference\3 General [2023-2027] {20230415}"),

                    Input.ImportFromText(4,
                                         "Extra Class Question Pool",
                                         new DateTime(2020, 7, 1),
                                         new DateTime(2024, 6, 30),
                                         @"..\Reference\4 Extra [2020-2024] {20210817}")
                };

                foreach (var exam in exams) {
                    using (var stream = File.OpenWrite(@"..\Source\HamCheck.Exam\Resources\Element" + exam.Number.ToString(CultureInfo.InvariantCulture) + "-" + exam.ValidFrom.Date.Year.ToString(CultureInfo.InvariantCulture) +  ".xml")) {
                        Output.SaveElement(exam, stream);
                    }

                    Console.WriteLine("Element " + exam.Number.ToString() + ": " + exam.Title);
                    Console.WriteLine("    Pool size ...........: " + exam.PoolSize.ToString().PadLeft(3));
                    Console.WriteLine("    Number of questions .: " + exam.DefaultQuestionsCount.ToString().PadLeft(3));
                    Console.WriteLine("    Minimum passing score: " + exam.DefaultMinimumCorrect.ToString().PadLeft(3));
                    Console.WriteLine();
                }
                Console.ReadKey();
            } catch (Exception ex) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Error.WriteLine(ex.StackTrace);
                Console.ResetColor();

                Console.Error.WriteLine();
                Console.Error.WriteLine("Press any key to continue.");
                Console.ReadKey();
                System.Environment.Exit(1);
            }
        }

    }
}
