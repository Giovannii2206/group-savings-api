using System.ComponentModel.DataAnnotations;

namespace GroupSavingsApi.DTOs
{
    public class CreateGroupSessionDto
    {
        [Required]
        public Guid GroupId { get; set; }
        [Required]
        public decimal TargetAmount { get; set; }
        [Required]
        public DateTime TargetDate { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public string Frequency { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }

    public class UpdateGroupSessionDto
    {
        public decimal? TargetAmount { get; set; }
        public DateTime? TargetDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string? Frequency { get; set; }
        public string? Status { get; set; }
        public decimal? TotalContributed { get; set; }
    }

    public class GroupSessionResponseDto
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public decimal TargetAmount { get; set; }
        public DateTime TargetDate { get; set; }
        public DateTime StartDate { get; set; }
        public string Frequency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalContributed { get; set; }
    }
}

