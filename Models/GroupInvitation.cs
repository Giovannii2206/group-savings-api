using System;
using System.ComponentModel.DataAnnotations;

namespace GroupSavingsApi.Models
{
    public class GroupInvitation
    {
        public Guid Id { get; set; }
        [Required]
        public Guid GroupId { get; set; }
        [Required]
        public Guid InviterId { get; set; }
        [Required]
        public string InviteeEmail { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool Accepted { get; set; } = false;
        public DateTime? AcceptedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
