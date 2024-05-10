using System.ComponentModel.DataAnnotations;

namespace Szkola2.Models
{
    public class Document
    {
        [Key]
        [Display(Name = "Identyfikator dokumentu")]
        public int DocumentId { get; set; }

        [Required]
        [Display(Name = "Nazwa dokumentu:")]
        [MaxLength(100, ErrorMessage = "Tytuł nie może przekraczać 100 znaków")]
        public string? DocumentDownload { get; set; }

        [Display(Name = "Wczytaj dokument:")]
        [FileExtensions(Extensions = ".pdf,.docx,.doc,.txt", ErrorMessage = "Niepoprawne rozszerzenie pliku.")]
        public string? DocumentName { get; set; }

        [MaxLength(50, ErrorMessage = "Rozszerzenie nie może przekraczać 50 znaków")]
        [Display(Name = "Typ dokumentu:")]
        public string? DocumentType { get; set; }

    }
}
