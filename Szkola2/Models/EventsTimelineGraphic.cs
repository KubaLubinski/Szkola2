using System.ComponentModel.DataAnnotations.Schema;

namespace Szkola2.Models
{
    public class EventsTimelineGraphic
    {
        public int Id { get; set; }
        public int? EventId { get; set; }
        public string? Name { get; set; }
        public string? URL { get; set; }

        [ForeignKey("EventId")]
        public virtual PastSchoolEvent? PastEvent { get; set; }
    }
}
