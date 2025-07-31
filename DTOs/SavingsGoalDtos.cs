using System;

namespace GroupSavingsApi.DTOs
{
    public class CreateSavingsGoalDto
    {
        public Guid GroupSessionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public DateTime TargetDate { get; set; }
    }

    public class SavingsGoalResponseDto
    {
        public Guid Id { get; set; }
        public Guid GroupSessionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime TargetDate { get; set; }
    }
}
