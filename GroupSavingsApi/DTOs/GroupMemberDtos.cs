using System.ComponentModel.DataAnnotations;

namespace GroupSavingsApi.DTOs
{
    public class CreateGroupMemberDto
    {
        [Required]
        public Guid GroupId { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public int RoleId { get; set; }
    }

    public class UpdateGroupMemberDto
    {
        public int? RoleId { get; set; }
        public decimal? TotalContributed { get; set; }
    }

    public class GroupMemberResponseDto
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public Guid UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime JoinedAt { get; set; }
        public decimal TotalContributed { get; set; }
    }
}

