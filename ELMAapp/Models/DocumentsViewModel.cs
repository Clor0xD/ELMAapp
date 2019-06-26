using System.Collections.Generic;
using System.Linq;

namespace ELMAapp.Models
{
    public class DocumentsViewModel
    {
        public Documents Document { get; set; }
        public string Date { get; set; }
        public string BinaryFileLimit { get; set; }

        public DocumentsViewModel(Documents doc)
        {
            Document = doc;
            BinaryFileLimit = doc.BinaryFile;
            Date = doc.Date.ToShortDateString() + " " + doc.Date.ToShortTimeString();
        }

        public DocumentsViewModel(Documents doc, string binaryFileLimit)
        {
            Document = doc;
            BinaryFileLimit = binaryFileLimit;
            Date = doc.Date.ToShortDateString() + " " + doc.Date.ToShortTimeString();
        }

        public static IEnumerable<DocumentsViewModel> GetViewModel(IEnumerable<Documents> documents)
        {
            return documents.Select(doc => doc.BinaryFile.Length > 30
                    ? new DocumentsViewModel(doc, doc.BinaryFile.Substring(0, 30) + "...")
                    : new DocumentsViewModel(doc))
                .ToList();
        }
    }
}