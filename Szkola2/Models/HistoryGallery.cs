using System.ComponentModel.DataAnnotations.Schema;

namespace Szkola2.Models
{
    public class HistoryGallery
    {
        public int Id { get; set; }
        public int? HistoryId { get; set; }
        public string? Name { get; set; }
        public string? URL { get; set; }

        [ForeignKey("HistoryId")]
        public virtual History? History { get; set; }
    }
}
