using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace Szkola2.Models
{
    public class History
    {
        [Key]
        [Display(Name = "Identyfikator:")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tytuł wpisu:")]
        [MaxLength(75, ErrorMessage = "Tytuł nie może przekraczać 75 znaków")]
        public string? TextName { get; set; }

        [Required]
        [Display(Name = "Opis historii szkoły")]
        public string? Content { get; set; }

        [NotMapped]
        public virtual IFormFileCollection? HistoryGalleryFiles { get; set; }

        public virtual List<HistoryGallery>? HistoryGallerys { get; set; }

        [NotMapped]
        public virtual IFormFileCollection? GraphicGalleryFiles { get; set; }

        public virtual List<Graphic>? GraphicGallery { get; set; }
    }
}
