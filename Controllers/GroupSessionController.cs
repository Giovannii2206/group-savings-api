using GroupSavingsApi.Data;
using GroupSavingsApi.DTOs;
using GroupSavingsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupSavingsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupSessionController : ControllerBase
    {
        private readonly GroupSavingsDbContext _context;

        public GroupSessionController(GroupSavingsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupSessionResponseDto>>> GetGroupSessions()
        {
            var sessions = await _context.GroupSessions.Select(s => new GroupSessionResponseDto
            {
                Id = s.Id,
                GroupId = s.GroupId,
                TargetAmount = s.TargetAmount,
                TargetDate = s.TargetDate,
                StartDate = s.StartDate,
                Frequency = s.Frequency,
                Status = s.Status,
                TotalContributed = s.TotalContributed
            }).ToListAsync();
            return Ok(sessions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GroupSessionResponseDto>> GetGroupSession(Guid id)
        {
            var s = await _context.GroupSessions.FindAsync(id);
            if (s == null) return NotFound();
            return Ok(new GroupSessionResponseDto
            {
                Id = s.Id,
                GroupId = s.GroupId,
                TargetAmount = s.TargetAmount,
                TargetDate = s.TargetDate,
                StartDate = s.StartDate,
                Frequency = s.Frequency,
                Status = s.Status,
                TotalContributed = s.TotalContributed
            });
        }

        [HttpPost]
        public async Task<ActionResult<GroupSessionResponseDto>> CreateGroupSession(CreateGroupSessionDto dto)
        {
            var session = new GroupSession
            {
                Id = Guid.NewGuid(),
                GroupId = dto.GroupId,
                TargetAmount = dto.TargetAmount,
                TargetDate = dto.TargetDate,
                StartDate = dto.StartDate,
                Frequency = dto.Frequency,
                Status = dto.Status,
                TotalContributed = 0
            };
            _context.GroupSessions.Add(session);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGroupSession), new { id = session.Id }, new GroupSessionResponseDto
            {
                Id = session.Id,
                GroupId = session.GroupId,
                TargetAmount = session.TargetAmount,
                TargetDate = session.TargetDate,
                StartDate = session.StartDate,
                Frequency = session.Frequency,
                Status = session.Status,
                TotalContributed = session.TotalContributed
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroupSession(Guid id, UpdateGroupSessionDto dto)
        {
            var session = await _context.GroupSessions.FindAsync(id);
            if (session == null) return NotFound();
            if (dto.TargetAmount.HasValue) session.TargetAmount = dto.TargetAmount.Value;
            if (dto.TargetDate.HasValue) session.TargetDate = dto.TargetDate.Value;
            if (dto.StartDate.HasValue) session.StartDate = dto.StartDate.Value;
            if (dto.Frequency != null) session.Frequency = dto.Frequency;
            if (dto.Status != null) session.Status = dto.Status;
            if (dto.TotalContributed.HasValue) session.TotalContributed = dto.TotalContributed.Value;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroupSession(Guid id)
        {
            var session = await _context.GroupSessions.FindAsync(id);
            if (session == null) return NotFound();
            _context.GroupSessions.Remove(session);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
