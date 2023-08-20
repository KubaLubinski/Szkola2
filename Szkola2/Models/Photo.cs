using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Szkola2.Models
{
    public class Photo
    {
        [Key]
        [Display(Name = "Identyfikator obrazu:")]
        public int PhotoId { get; set; }

        [Required]
        [Display(Name = "Grafika:")]
        [MaxLength(128)]
        [FileExtensions(Extensions = ".jpg,.png,.gif", ErrorMessage = "Niepoprawne rozszerzenie pliku.")]
        public string? Graphic { get; set; }

        [Display(Name = "Identyfikator tekstu:")]
        public int? TextId { get; set; }

        [Display(Name = "Identyfikator historii:")]
        public string? HistoryId { get; set; }

        [ForeignKey("TextId")]
        public virtual Text? Text { get; set; }

        [ForeignKey("Id")]
        public virtual History? History { get; set; }

    }
}
