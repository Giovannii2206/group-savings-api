using System;
using System.ComponentModel.DataAnnotations;

namespace GroupSavingsApi.Models
{
    public class SavingsGoal
    {
        public Guid Id { get; set; }
        [Required]
        public Guid GroupId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; } = 0;
        public DateTime TargetDate { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
