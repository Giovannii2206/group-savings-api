using System;

namespace GroupSavingsApi.DTOs
{
    public class CreateGroupInvitationDto
    {
        public Guid GroupId { get; set; }
        public Guid InviterId { get; set; }
        public string InviteeEmail { get; set; } = string.Empty;
    }

    public class GroupInvitationResponseDto
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public Guid InviterId { get; set; }
        public string InviteeEmail { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool Accepted { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }
}
