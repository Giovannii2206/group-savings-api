using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroupSavingsApi.Models
{
    public class Contribution
    {
        public Guid MemberId { get; set; }
        public Member Member { get; set; } = null!;
        public Guid Id { get; set; }
        
        [Required]
        public Guid GroupSessionId { get; set; }
        
        [Required]
        public Guid UserId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;
        
        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = string.Empty; // "manual" or "auto"
        // Navigation properties
        public GroupSession GroupSession { get; set; } = null!;
        public User User { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
    }
}

