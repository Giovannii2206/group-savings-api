using System.ComponentModel.DataAnnotations;

namespace GroupSavingsApi.DTOs
{
    public class CreateGroupDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public Guid CreatedBy { get; set; }
        public string Status { get; set; } = "Active";
    }

    public class UpdateGroupDto
    {
        public string? Name { get; set; }
        public string? Status { get; set; }
    }

    public class GroupResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid CreatedBy { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
