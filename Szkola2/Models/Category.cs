using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Szkola2.Models
{
    public class Category
    {
        [Key]
        [Display(Name = "Identyfikator kategorii:")]
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Nazwa kategorii:")]
        [MaxLength(75, ErrorMessage = "Nazwa kategorii nie może być dłuższa niż 75 znaków")]
        public string? CategoryName { get; set; }

        [Required]
        [Display(Name = "Opis kategorii:")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Czy kategoria jest aktywna?")]
        public bool IsActive { get; set; }

        public virtual List<Text>? Texts { get; set; }
    }
}
