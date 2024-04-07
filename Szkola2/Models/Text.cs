using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Szkola2.Models
{
    public class Text
    {
        [Key]
        [Display(Name = "Identyfikator")]
        public int TextId { get; set; }

        [Required]
        [Display(Name = "Tytuł tekstu:")]
        [MaxLength(75, ErrorMessage = "Tytuł tekstu nie może przekraczać 75 znaków")]
        public string? TextName { get; set; }

        [Required]
        [Display(Name = "Treść tekstu:")]
        public string? Content { get; set; }

        [Required]
        [Display(Name = "Data dodania:")]
        [DataType(DataType.Date, ErrorMessage = "Niepoprawny format daty")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        public System.DateTime DateAdded { get; set; }

        [Display(Name = "Grafika tytułowa tekstu:")]
        [MaxLength(128)]
        [FileExtensions(Extensions = ".jpg,.png,.gif", ErrorMessage = "Niepoprawne rozszerzenie pliku.")]
        public string? MainGraphic { get; set; }

        [Display(Name = "Data wydarzenia:")]
        [DataType(DataType.Date, ErrorMessage = "Niepoprawny format daty")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        public System.DateTime EventDate { get; set; }

        [Display(Name = "Kategoria tekstu:")]
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Czy wyświetlać?")]
        public bool IsActive { get; set; }

        [Display(Name = "Autor tekstu:")]
        public string? Id { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [ForeignKey("Id")]
        public virtual AppUser? User { get; set; }

        [NotMapped]
        public virtual IFormFileCollection? GraphicGalleryFiles { get; set; }

        public virtual List<Graphic>? GraphicGallery { get; set; }
    }
}

