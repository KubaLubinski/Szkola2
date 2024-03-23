using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Szkola2.Models
{
    public class Graphic
    {
        public int Id { get; set; }
        public int? TextId { get; set; }
        public int? HistoryId { get; set; }
        public int? EventId { get; set; }
        public string? Name { get; set; }
        public string? URL { get; set; }

        [ForeignKey("TextId")]
        public virtual Text? Texts { get; set; }

        [ForeignKey("HistoryId")]
        public virtual History? History { get; set; }

        [ForeignKey("EventId")]
        public virtual PastSchoolEvent? PastEvent { get; set; }
    }
}
