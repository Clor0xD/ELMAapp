using System;
using System.Collections.Generic;
using System.Linq;

namespace ELMAapp.Models
{
    public class ViewDoc
    {
        public Document Document { get; set; }
        public string Date { get; set; }
        public string BinaryFileLimit { get; set; }

        public static readonly List<string> SortedFields = new List<string>() { "Name", "Author", "Date" };

        public static readonly List<string> SelectFields = new List<string>()
            {"Name", "Author", "BinaryFile", "All Fields"};

        public ViewDoc(Document doc)
        {
            Document = doc;
            BinaryFileLimit = doc.BinaryFile;
            Date = doc.Date.ToShortDateString() + " " + doc.Date.ToShortTimeString();
        }

        public ViewDoc(Document doc, string binaryFileLimit)
        {
            Document = doc;
            BinaryFileLimit = binaryFileLimit;
            Date = doc.Date.ToShortDateString() + " " + doc.Date.ToShortTimeString();
        }

        public static IEnumerable<ViewDoc> GetViewListDoc(AppDBContext dbContext, bool reverse, string sortBy, string selectField, string searchString, DateTime startDate, DateTime endDate)
        {
            if (searchString == null) searchString = "";
            if (!SortedFields.Contains(sortBy)) sortBy = "Name";

            var documents = dbContext.Documents
                .Where(d =>
                    (d.Date >= startDate && d.Date < endDate) && (
                    (!SelectFields.Contains(selectField) || searchString == "") ||
                    ((selectField == "Name" || selectField == "All Fields") && d.Name == searchString) ||
                    ((selectField == "Author" || selectField == "All Fields") && d.Author == searchString) ||
                    ((selectField == "BinaryFile" || selectField == "All Fields") && d.BinaryFile == searchString)))
                .OrderBy(sortBy, !reverse)
                .ThenBy(d =>
                    d.Date) // ограничился вторичной сортировквкой по дате. при наложении сортировок linq гарантирует сохранность порядка.
                .ToList();

            return documents.Select(doc => doc.BinaryFile.Length > 30
                    ? new ViewDoc(doc, doc.BinaryFile.Substring(0, 30) + "...")
                    : new ViewDoc(doc))
                .ToList();
        }
    }
}