using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Szkola2.Models
{
    public class Graphic
    {
        [Key]
        [Display(Name = "Identyfikator grafiki:")]
        public int Id { get; set; }

        [Display(Name = "Tekst do którego grafika jest przypisana:")]
        public int? TextId { get; set; }

        [Display(Name = "Wpis o historii do którego grafika jest przypisana:")]
        public int? HistoryId { get; set; }

        [Display(Name = "Wpis o wydarzeniu do którego grafika jest przypisana:")]
        public int? EventId { get; set; }

        [Display(Name = "Nazwa grafiki:")]
        public string? Name { get; set; }

        [Display(Name = "Ścieżka dostępu do grafiki:")]
        public string? URL { get; set; }

        [ForeignKey("TextId")]
        public virtual Text? Texts { get; set; }

        [ForeignKey("HistoryId")]
        public virtual History? History { get; set; }

        [ForeignKey("EventId")]
        public virtual PastSchoolEvent? PastEvent { get; set; }
    }
}
