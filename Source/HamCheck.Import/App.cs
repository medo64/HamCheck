using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace HamCheck.Import {
    internal class App {

        internal static void Main(string[] args) {
            try {
                var exams = new List<ExamElement>();

                exams.Add(ExamElement.ImportFromText(2, "Technician Class Question Pool",
                                                     new DateTime(2018, 7, 1), new DateTime(2022, 6, 30),
                                                     @"..\Reference\2 Technician [2022-2026] {20220111}"));

                exams.Add(ExamElement.ImportFromText(3, "General Class Question Pool",
                                                     new DateTime(2019, 7, 1), new DateTime(2023, 6, 30),
                                                     @"..\Reference\3 General [2019-2023] {20210817}"));

                exams.Add(ExamElement.ImportFromText(4, "Extra Class Question Pool",
                                                     new DateTime(2020, 7, 1), new DateTime(2024, 6, 30),
                                                     @"..\Reference\4 Extra [2020-2024] {20210817}"));

                foreach (var exam in exams) {
                    using (var stream = File.OpenWrite(@"Element" + exam.Number.ToString(CultureInfo.InvariantCulture) + "-" + exam.ValidFrom.Date.Year.ToString(CultureInfo.InvariantCulture) +  ".xml")) {
                        exam.Save(stream);
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
