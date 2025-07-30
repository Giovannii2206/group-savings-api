using System.ComponentModel.DataAnnotations;

namespace GroupSavingsApi.DTOs
{
    public class CreatePaymentMethodDto
    {
        [Required]
        public Guid CustomerId { get; set; }
        [Required]
        public int AccountTypeId { get; set; }
        [Required]
        public string ProviderName { get; set; } = string.Empty;
        [Required]
        public string AccountName { get; set; } = string.Empty;
        [Required]
        public string AccountNumber { get; set; } = string.Empty;
        [Required]
        public string Currency { get; set; } = string.Empty;
        [Required]
        public string CountryCode { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }

    public class UpdatePaymentMethodDto
    {
        public string? ProviderName { get; set; }
        public string? AccountName { get; set; }
        public string? AccountNumber { get; set; }
        public string? Currency { get; set; }
        public string? CountryCode { get; set; }
        public bool? IsPrimary { get; set; }
    }

    public class PaymentMethodResponseDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public int AccountTypeId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
