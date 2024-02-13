using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Szkola2.Models
{
    public class Gallery
    {
        public int Id { get; set; }
        public int? TextId { get; set; }
        public string? Name { get; set; }
        public string? URL { get; set; }

        [ForeignKey("TextId")]
        public virtual Text? Texts { get; set; }
    }
}
