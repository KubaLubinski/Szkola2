using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace Szkola2.Models
{
    public class PastSchoolEvent
    {
        [Key]
        [Display(Name = "Identyfikator:")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nazwa wydarzenia:")]
        [MaxLength(75, ErrorMessage = "Nazwa nie może przekraczać 75 znaków")]
        public string? EventName { get; set; }

        [Required]
        [Display(Name = "Data wydarzenia:")]
        [DataType(DataType.Date, ErrorMessage = "Niepoprawny format daty")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        public System.DateTime EventDate { get; set; }

        [Required]
        [Display(Name = "Opis wydarzenia")]
        public string? Content { get; set; }

        [Display(Name = "Grafika tytułowa:")]
        [MaxLength(128)]
        [FileExtensions(Extensions = ".jpg,.png,.gif", ErrorMessage = "Niepoprawne rozszerzenie pliku.")]
        public string? MainGraphic { get; set; }

        [NotMapped]
        public virtual IFormFileCollection? PastEventGalleryFiles { get; set; }

        public virtual List<EventsTimelineGraphic>? PastEventGallerys { get; set; }

        [NotMapped]
        public virtual IFormFileCollection? GraphicGalleryFiles { get; set; }

        public virtual List<Graphic>? GraphicGallery { get; set; }
    }
}
