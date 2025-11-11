using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VinylVibe.Models
{
    public class Genre
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        [DisplayName("Genre Name")]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1, 150)]
        public int DisplayOrder { get; set; }

    }
}
