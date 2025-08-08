using System;

namespace GroupSavingsApi.Models
{
    public class GroupInvitation
    {
        // Navigation properties
        public Group Group { get; set; } = null!;
        public User Inviter { get; set; } = null!;
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public Guid InviterId { get; set; }
        public string InviteeEmail { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool Accepted { get; set; } = false;
        public DateTime? AcceptedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}

