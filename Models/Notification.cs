using System.ComponentModel.DataAnnotations;

namespace GroupSavingsApi.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; } = null!;
    }
}
