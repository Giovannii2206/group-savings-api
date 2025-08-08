using System.ComponentModel.DataAnnotations;

namespace GroupSavingsApi.Models
{
    public class AccountType
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
    }
}

