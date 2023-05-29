using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace HamCheck.Import {
    internal static class Output {

        public static void SaveElement(ExamElement examElement, Stream stream) {
            using (var xml = new XmlTextWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)) { Formatting = Formatting.Indented, Indentation = 4 }) {
                xml.WriteStartDocument();
                xml.WriteStartElement("HamExam");
                xml.WriteAttributeString("elementNumber", examElement.Number.ToString(CultureInfo.InvariantCulture));
                xml.WriteAttributeString("title", examElement.Title.ToString(CultureInfo.InvariantCulture));
                xml.WriteAttributeString("validFrom", examElement.ValidFrom.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                xml.WriteAttributeString("validTo", examElement.ValidTo.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                xml.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                xml.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, "https://medo64.com/schema/hamexam.xsd");

                xml.WriteStartElement("Illustrations");
                foreach (var illustration in examElement.Illustrations) {
                    xml.WriteStartElement("Illustration");
                    xml.WriteAttributeString("name", illustration.Name);
                    xml.WriteAttributeString("picture", Convert.ToBase64String(illustration.PictureBytes));
                    xml.WriteEndElement();
                }
                xml.WriteEndElement();

                foreach (var subelement in examElement.Subelements) {
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
                                if (question.Explanation.IllustrationBytes != null) {
                                    xml.WriteAttributeString("illustration", Convert.ToBase64String(question.Explanation.IllustrationBytes));
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

    }
}
