using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Szkola2.Models
{
    public class History
    {
        [Key]
        [Display(Name = "Identyfikator:")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Rok w historii szkoły")]
        public int Year { get; set; }

        [Required]
        [Display(Name = "Opis wydarzeń w danym roku")]
        public string? Content { get; set; }

        public virtual List<Photo>? Photos { get; set; }
    }
}
