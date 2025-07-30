using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroupSavingsApi.Models
{
    public class GroupMember
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid GroupId { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        public int RoleId { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalContributed { get; set; } = 0;

        // Navigation properties
        public Group Group { get; set; } = null!;
        public User User { get; set; } = null!;
        public GroupRole Role { get; set; } = null!;
    }
}
