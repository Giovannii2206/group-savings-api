using System.ComponentModel.DataAnnotations;

namespace GroupSavingsApi.Models
{
    public class GroupRole
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty; // "admin", "member", etc.
        
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
    }
}

