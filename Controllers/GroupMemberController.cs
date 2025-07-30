using GroupSavingsApi.Data;
using GroupSavingsApi.DTOs;
using GroupSavingsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupSavingsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupMemberController : ControllerBase
    {
        private readonly GroupSavingsDbContext _context;

        public GroupMemberController(GroupSavingsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupMemberResponseDto>>> GetGroupMembers()
        {
            var members = await _context.GroupMembers.Select(m => new GroupMemberResponseDto
            {
                Id = m.Id,
                GroupId = m.GroupId,
                UserId = m.UserId,
                RoleId = m.RoleId,
                JoinedAt = m.JoinedAt,
                TotalContributed = m.TotalContributed
            }).ToListAsync();
            return Ok(members);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GroupMemberResponseDto>> GetGroupMember(Guid id)
        {
            var m = await _context.GroupMembers.FindAsync(id);
            if (m == null) return NotFound();
            return Ok(new GroupMemberResponseDto
            {
                Id = m.Id,
                GroupId = m.GroupId,
                UserId = m.UserId,
                RoleId = m.RoleId,
                JoinedAt = m.JoinedAt,
                TotalContributed = m.TotalContributed
            });
        }

        [HttpPost]
        public async Task<ActionResult<GroupMemberResponseDto>> CreateGroupMember(CreateGroupMemberDto dto)
        {
            var member = new GroupMember
            {
                Id = Guid.NewGuid(),
                GroupId = dto.GroupId,
                UserId = dto.UserId,
                RoleId = dto.RoleId,
                JoinedAt = DateTime.UtcNow,
                TotalContributed = 0
            };
            _context.GroupMembers.Add(member);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGroupMember), new { id = member.Id }, new GroupMemberResponseDto
            {
                Id = member.Id,
                GroupId = member.GroupId,
                UserId = member.UserId,
                RoleId = member.RoleId,
                JoinedAt = member.JoinedAt,
                TotalContributed = member.TotalContributed
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroupMember(Guid id, UpdateGroupMemberDto dto)
        {
            var member = await _context.GroupMembers.FindAsync(id);
            if (member == null) return NotFound();
            if (dto.RoleId.HasValue) member.RoleId = dto.RoleId.Value;
            if (dto.TotalContributed.HasValue) member.TotalContributed = dto.TotalContributed.Value;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroupMember(Guid id)
        {
            var member = await _context.GroupMembers.FindAsync(id);
            if (member == null) return NotFound();
            _context.GroupMembers.Remove(member);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
