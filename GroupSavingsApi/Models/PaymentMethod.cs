using System.ComponentModel.DataAnnotations;

namespace GroupSavingsApi.Models
{
    public class PaymentMethod
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid CustomerId { get; set; }
        
        [Required]
        public int AccountTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ProviderName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string AccountName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string AccountNumber { get; set; } = string.Empty;
        
        public bool IsPrimary { get; set; }
        
        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(2)]
        public string CountryCode { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Member Member { get; set; } = null!;
        public AccountType AccountType { get; set; } = null!;
    }
}

