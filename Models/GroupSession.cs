using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroupSavingsApi.Models
{
    public class GroupSession
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid GroupId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TargetAmount { get; set; }
        
        [Required]
        public DateTime TargetDate { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Frequency { get; set; } = string.Empty; // "daily" | "weekly"
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Active";
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalContributed { get; set; } = 0;

        // Navigation properties
        public Group Group { get; set; } = null!;
        public ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();
    }
}
