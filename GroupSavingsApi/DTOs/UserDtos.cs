using System.ComponentModel.DataAnnotations;

namespace GroupSavingsApi.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        public string Role { get; set; } = "Member";
    }

    public class UpdateUserDto
    {
        public string? Password { get; set; }
        
        [EmailAddress]
        public string? Email { get; set; }
        
        public string? Role { get; set; }
        
        public bool? IsActive { get; set; }
    }

    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserResponseDto User { get; set; } = null!;
    }
}

