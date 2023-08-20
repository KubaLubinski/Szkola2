using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;

namespace Szkola2.Models
{
    public class AppUser : IdentityUser
    {
        [Display(Name = "Imię użytkownika")]
        [MaxLength(30)]
        public string? FirstName { get; set; }

        [Display(Name = "Nazwisko użytkownika")]
        [MaxLength(60)]
        public string? LastName { get; set; }
        #region
        [NotMapped]
        [Display(Name = "Pan/Pani:")]
        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }
        #endregion

        [Display(Name = "Informacje o użytkowniku:")]
        [MaxLength(255, ErrorMessage = "Opis zbyt długi, proszę skrócić do 255 znakow.")]
        public string? Information { get; set; }

        [Required]
        [Display(Name = "Czy konto jest aktywne?")]
        public bool IsActive { get; set; }

        [Required]
        [Display(Name = "Czy konto może dodwawać nowe teksty?")]
        public bool CanAddTexts { get; set; }

        public virtual List<Text>? Texts { get; set; }
    }
}
