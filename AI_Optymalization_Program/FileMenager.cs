using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Layout;
using iText.Layout.Element;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Optymalization_Program
{
    public interface IGeneratePDFReport
    {
        void GenerateReport(string path, string reportContent);
    }
    public interface IGenerateTextReport
    {
        void GenerateReport(string path, string reportContent);
    }
    public class PdfReportGenerator : IGeneratePDFReport
    {
        public void GenerateReport(string path, string reportContent)
        {
            try
            {
                string existiongText = string.Empty;
                if (File.Exists(path))
                {
                    using (PdfReader pdfReader = new PdfReader(path))
                    {
                        using (PdfDocument pdfDocument = new PdfDocument(pdfReader))
                        {
                            for (int pageNum = 1; pageNum <= pdfDocument.GetNumberOfPages(); pageNum++)
                            {
                                existiongText += PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(pageNum));
                            }
                        }
                    }
                }
                using (PdfWriter writer = new PdfWriter(path))
                {
                    using (PdfDocument pdf = new PdfDocument(writer))
                    {
                        Document document = new Document(pdf);
                        document.Add(new Paragraph(existiongText + "\n" + reportContent));
                        document.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating PDF report: {ex.Message}");

                Exception innerException = ex.InnerException;
                while (innerException != null)
                {
                    Console.WriteLine($"Inner Exception: {innerException.Message}");
                    innerException = innerException.InnerException;
                }
            }
        }
    }
    public class TextReportGenerator : IGenerateTextReport
    {
        public void GenerateReport(string path, string reportContent)
        {
            try
            {
                if (!File.Exists(path))
                {
                    File.Create(path);
                }
                string existingContent = File.ReadAllText(path);
                string updatedContent;
                if (existingContent == string.Empty)
                {
                    updatedContent = reportContent;
                }
                else
                {
                    updatedContent = existingContent + reportContent;
                }
                File.WriteAllText(path, updatedContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd podczas zapisywania wartości do zrealizowania: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }
    }
    public class FileMenager
    {
        public void DelatePDF(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        public void WriteValuesToRun(string path, List<string> OptymalizationAlgorithms, List<string> TestFunctions, List<int> I, List<int> D, List<int> M)
        {
            try
            {
                if (!File.Exists(path))
                {
                    File.Create(path);
                }
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.Write("OptymalizationAlgorithms:".PadRight(15) + " ");
                    for (int i = 0; i < OptymalizationAlgorithms.Count; i++)
                    {
                        writer.Write(OptymalizationAlgorithms[i].ToString().PadRight(15) + " ");
                    }
                    writer.WriteLine();
                    writer.Write("TestFunctions:".PadRight(15) + " ");
                    for (int i = 0; i < TestFunctions.Count; i++)
                    {
                        writer.Write(TestFunctions[i].ToString().PadRight(15) + " ");
                    }
                    writer.WriteLine();
                    writer.Write("I:".PadRight(5) + " ");
                    for (int i = 0; i < I.Count; i++)
                    {
                        writer.Write(I[i].ToString().PadRight(5) + " ");
                    }
                    writer.WriteLine();
                    writer.Write("D:".PadRight(5) + " ");
                    for (int i = 0; i < D.Count; i++)
                    {
                        writer.Write(D[i].ToString().PadRight(5) + " ");
                    }
                    writer.WriteLine();
                    writer.Write("M:".PadRight(5) + " ");
                    for (int i = 0; i < M.Count; i++)
                    {
                        writer.Write(M[i].ToString().PadRight(5) + " ");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd podczas zapisywania wartości do zrealizowania: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }
        public List<string> GetLastOperation(string path)
        {
            List<string> values = new List<string>();
            if (File.Exists(path))
            {
                if (File.ReadAllLines(path) != null)
                {
                    string lastOperation = File.ReadAllLines(path).Last();
                    string[] temptValues = lastOperation.Split(';');
                    for (int i = 0; i < temptValues.Length; i++)
                    {
                        values.Add(temptValues[i].Split(':').Last().Replace(" ", ""));
                    }
                }
            }
            return values;
        }
        public void DelateResultsFile(string path,bool continuation)
        {
            if (continuation)
            {
                return;
            }
            if (File.Exists(path))
            {
                File.WriteAllText(path, string.Empty);
            }
        }
    }
}