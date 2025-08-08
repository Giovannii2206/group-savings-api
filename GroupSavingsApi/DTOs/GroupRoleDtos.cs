using System.ComponentModel.DataAnnotations;

namespace GroupSavingsApi.DTOs
{
    public class CreateGroupRoleDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateGroupRoleDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class GroupRoleResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}

