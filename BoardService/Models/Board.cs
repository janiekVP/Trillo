using System.ComponentModel.DataAnnotations;

namespace BoardService.Models
{
    public class Board
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Name must be between 1 and 50 characters.", MinimumLength = 1)]
        [RegularExpression(@"^[\w\s\-\']+$", ErrorMessage = "Name contains invalid characters.")]
        public string Name { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Description can be up to 100 characters.")]
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
