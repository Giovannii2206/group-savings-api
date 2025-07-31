using System;

namespace GroupSavingsApi.DTOs
{
    public class CreateAuditLogDto
    {
        public Guid? GroupId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public string? Details { get; set; }
    }

    public class AuditLogResponseDto
    {
        public Guid Id { get; set; }
        public Guid? GroupId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }
    }
}
