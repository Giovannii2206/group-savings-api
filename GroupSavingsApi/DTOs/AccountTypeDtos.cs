using System.ComponentModel.DataAnnotations;

namespace GroupSavingsApi.DTOs
{
    public class CreateAccountTypeDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateAccountTypeDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class AccountTypeResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}

