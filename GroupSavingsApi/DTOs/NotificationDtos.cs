using System.ComponentModel.DataAnnotations;

namespace GroupSavingsApi.DTOs
{
    public class CreateNotificationDto
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string Message { get; set; } = string.Empty;
        [Required]
        public string Type { get; set; } = string.Empty;
    }

    public class UpdateNotificationDto
    {
        public string? Message { get; set; }
        public string? Type { get; set; }
    }

    public class NotificationResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

