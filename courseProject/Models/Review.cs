using System;
using System.ComponentModel.DataAnnotations;

namespace courseProject.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string AuthorId { get; set; } = null!;

        [Required]
        public string AuthorName { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        public string Text { get; set; } = null!;

        [Range(1,5)]
        public int Rating { get; set; } = 5;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
