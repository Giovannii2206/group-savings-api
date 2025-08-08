using System.ComponentModel.DataAnnotations;

namespace GroupSavingsApi.Models
{
    public class User
    {
        public Guid Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = "Member";
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Member? Member { get; set; }
        public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
        public ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}

