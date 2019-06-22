using System.Collections.Generic;

namespace ELMAapp.Models
{
    public class ViewModel
    {
        public IEnumerable<ViewDoc> ViewDocs { get; set; }
        public SearchModel Search { get; set; }

        public ViewModel(IEnumerable<ViewDoc> viewDocs, SearchModel search)
        {
            ViewDocs = viewDocs;
            Search = search;
        }
    }
}