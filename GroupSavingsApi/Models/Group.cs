using System.ComponentModel.DataAnnotations;

namespace GroupSavingsApi.Models
{
    public class Group
    {
        public ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
        public ICollection<GroupSession> GroupSessions { get; set; } = new List<GroupSession>();
        public ICollection<GroupInvitation> GroupInvitations { get; set; } = new List<GroupInvitation>();
        // ... existing properties ...
        public User CreatedByUser { get; set; } = null!;
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public Guid CreatedBy { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Active";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public ICollection<GroupSession> Sessions { get; set; } = new List<GroupSession>();
        public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    }
}

