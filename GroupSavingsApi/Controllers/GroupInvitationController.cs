using GroupSavingsApi.Data;
using GroupSavingsApi.DTOs;
using GroupSavingsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupSavingsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupInvitationController : ControllerBase
    {
        private readonly GroupSavingsDbContext _context;

        public GroupInvitationController(GroupSavingsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupInvitationResponseDto>>> GetInvitations()
        {
            var invitations = await _context.Set<GroupInvitation>()
                .Where(i => !i.IsDeleted)
                .Select(i => new GroupInvitationResponseDto
                {
                    Id = i.Id,
                    GroupId = i.GroupId,
                    InviterId = i.InviterId,
                    InviteeEmail = i.InviteeEmail,
                    SentAt = i.SentAt,
                    Accepted = i.Accepted,
                    AcceptedAt = i.AcceptedAt
                }).ToListAsync();
            return Ok(invitations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GroupInvitationResponseDto>> GetInvitation(Guid id)
        {
            var i = await _context.Set<GroupInvitation>().FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
            if (i == null) return NotFound();
            return Ok(new GroupInvitationResponseDto
            {
                Id = i.Id,
                GroupId = i.GroupId,
                InviterId = i.InviterId,
                InviteeEmail = i.InviteeEmail,
                SentAt = i.SentAt,
                Accepted = i.Accepted,
                AcceptedAt = i.AcceptedAt
            });
        }

        [HttpPost]
        public async Task<ActionResult<GroupInvitationResponseDto>> CreateInvitation(CreateGroupInvitationDto dto)
        {
            var invitation = new GroupInvitation
            {
                Id = Guid.NewGuid(),
                GroupId = dto.GroupId,
                InviterId = dto.InviterId,
                InviteeEmail = dto.InviteeEmail,
                SentAt = DateTime.UtcNow,
                Accepted = false,
                IsDeleted = false
            };
            _context.Set<GroupInvitation>().Add(invitation);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetInvitation), new { id = invitation.Id }, new GroupInvitationResponseDto
            {
                Id = invitation.Id,
                GroupId = invitation.GroupId,
                InviterId = invitation.InviterId,
                InviteeEmail = invitation.InviteeEmail,
                SentAt = invitation.SentAt,
                Accepted = invitation.Accepted,
                AcceptedAt = invitation.AcceptedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvitation(Guid id)
        {
            var i = await _context.Set<GroupInvitation>().FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
            if (i == null) return NotFound();
            i.Accepted = true;
            i.AcceptedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvitation(Guid id)
        {
            var i = await _context.Set<GroupInvitation>().FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
            if (i == null) return NotFound();
            i.IsDeleted = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

