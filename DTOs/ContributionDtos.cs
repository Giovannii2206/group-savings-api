using System.ComponentModel.DataAnnotations;

namespace GroupSavingsApi.DTOs
{
    public class CreateContributionDto
    {
        [Required]
        public Guid GroupSessionId { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string Type { get; set; } = string.Empty;
    }

    public class UpdateContributionDto
    {
        public decimal? Amount { get; set; }
        public string? Type { get; set; }
    }

    public class ContributionResponseDto
    {
        public Guid Id { get; set; }
        public Guid GroupSessionId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
