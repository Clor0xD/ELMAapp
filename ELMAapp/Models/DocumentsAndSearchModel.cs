using System.Collections.Generic;

namespace ELMAapp.Models
{
    public class DocumentsAndSearchModel
    {
        public IEnumerable<DocumentsViewModel> ViewDocs { get; set; }
        public SearchModel Search { get; set; }
    }
}