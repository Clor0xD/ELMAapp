using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ELMAapp.Models
{
    public class SearchModel
    {
        [Display(Name = "Search")] public string SearchString { get; set; } = "";

        [DataType(DataType.Date), Required] public DateTime StartDate { get; set; } = DateTime.Now.AddYears(-99);

        [DataType(DataType.Date), Required] public DateTime EndDate { get; set; } = DateTime.Now.AddDays(1);

        public bool reverse { get; set; } = false;

        public string SelectCategory { get; set; } = "";

        public readonly SelectList SelectList = new SelectList(new[] {"All Fields", "Name", "Author", "BinaryFile"});
    }
}