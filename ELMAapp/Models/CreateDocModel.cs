using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ELMAapp.Models
{
    public class CreateDocModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public HttpPostedFileBase BinaryFile { get; set; }
    }
}