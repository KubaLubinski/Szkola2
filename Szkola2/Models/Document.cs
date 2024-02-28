using System.ComponentModel.DataAnnotations;

namespace Szkola2.Models
{
    public class Document
    {
        [Key]
        [Display(Name = "Identyfikator dokumentu")]
        public int DocumentId { get; set; }

        [Display(Name = "Pobierz")]
        public string? DocumentDownload { get; set; }

        [Display(Name = "Nazwa dokumentu")]
        [FileExtensions(Extensions = ".pdf,.docx,.doc,.txt", ErrorMessage = "Niepoprawne rozszerzenie pliku.")]
        public string? DocumentName { get; set; }

        [Display(Name = "Typ dokumentu")]
        public string? DocumentType { get; set; }

        

        //plik jakos jak zapisac do bazy i potem trezba zrobić jego link do pobrania
    }
}
