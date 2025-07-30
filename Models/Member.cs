using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroupSavingsApi.Models
{
    public class Member
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Phone]
        public string Phone { get; set; } = string.Empty;
        
        public string Address { get; set; } = string.Empty;
        
        public DateTime? DateOfBirth { get; set; }
        
        public string Gender { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal AccountBalance { get; set; } = 0;

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
    }
}
