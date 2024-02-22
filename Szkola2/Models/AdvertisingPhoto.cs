using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Szkola2.Models
{
    public class AdvertisingPhoto
    {
        [Key]
        [Display(Name = "Identyfikator obrazu")]
        public int PhotoId { get; set; }

        [Display(Name = "Obraz:")]
        [MaxLength(128)]
        [FileExtensions(Extensions = ".jpg,.png,.gif", ErrorMessage = "Niepoprawne rozszerzenie pliku.")]
        public string? Photo { get; set; }
    }
}
