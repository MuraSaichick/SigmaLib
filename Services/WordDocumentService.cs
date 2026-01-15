using Avalonia.Controls;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SigmaLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Words.NET;

namespace SigmaLib.Services
{
    public class WordDocumentService
    {
        /// Создать договор выдачи книги на основе шаблона
        public void CreateIssueContract(Reservation reservation, User librarian, User reader, Book book, BookCopy bookCopy)
        {
            string templatePath = "Assets/Templates/IssueContractTemplate.docx";
            string outputPath = $"Assets/Contracts/Договор_выдачи_{reader.LastName}_{book.Title}_{reservation.Id}.docx";

            Directory.CreateDirectory("Assets/Contracts");
            File.Copy(templatePath, outputPath, true);

            using (var doc = DocX.Load(outputPath))
            {
                doc.ReplaceText("{{id}}", reservation.Id.ToString());
                doc.ReplaceText("{{BookTitle}}", book.Title ?? "");
                doc.ReplaceText("{{Author}}", book.Author ?? "");
                doc.ReplaceText("{{Year}}", book.Year.ToString());
                doc.ReplaceText("{{IdBookCopy}}", bookCopy.Id.ToString());
                doc.ReplaceText("{{Condition}}", bookCopy.Condition ?? "");
                doc.ReplaceText("{{ReaderName}}", reader.FullName ?? "");
                doc.ReplaceText("{{LibrarianName}}", librarian.FullName ?? "");
                doc.ReplaceText("{{IssueDate}}", reservation.PickUpAt?.ToString("dd.MM.yyyy") ?? "");
                doc.ReplaceText("{{ReturnDate}}", reservation.ReturnDate?.ToString("dd.MM.yyyy") ?? "");
                doc.ReplaceText("{{Fine}}", reservation.Fine.ToString("0.##"));

                doc.Save();
            }
        }

        // Договор возврата
        public void CreateReturnContract(
        Reservation reservation,
        User librarian,
        User reader,
        Book book,
        int delayDays,
        decimal fineAmount,
        DateTime todayDate)
        {
            string templatePath = "Assets/Templates/ReturnContractTemplate.docx";
            string outputPath = $"Assets/Contracts/Договор_возврата_{reader.LastName}_{book.Title}_{reservation.Id}.docx";

            Directory.CreateDirectory("Assets/Contracts");
            File.Copy(templatePath, outputPath, true);

            using (var doc = DocX.Load(outputPath))
            {
                doc.ReplaceText("{{id}}", reservation.Id.ToString());
                doc.ReplaceText("{{BookTitle}}", book.Title ?? "");
                doc.ReplaceText("{{Author}}", book.Author ?? "");
                doc.ReplaceText("{{Year}}", book.Year.ToString());
                doc.ReplaceText("{{idBookCopy}}", reservation.BookCopyId.ToString());
                doc.ReplaceText("{{ReaderName}}", reader.FullName ?? "");
                doc.ReplaceText("{{IssueDate}}", reservation.PickUpAt?.ToString("dd.MM.yyyy") ?? "");
                doc.ReplaceText("{{ReturnDate}}", reservation.ReturnDate?.ToString("dd.MM.yyyy") ?? "");
                doc.ReplaceText("{{NowDate}}", todayDate.ToString("dd.MM.yyyy") ?? "");
                doc.ReplaceText("{{DelayDays}}", delayDays.ToString());
                doc.ReplaceText("{{FineAmount}}", fineAmount.ToString("0.##"));
                doc.ReplaceText("{{LibrarianName}}", librarian.FullName ?? "");

                doc.Save();
            }
        }
    }
}
