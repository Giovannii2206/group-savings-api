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
            var sessions = await _context.GroupSessions
                .Where(s => !s.IsDeleted)
                .Select(s => new GroupSessionResponseDto
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
            var s = await _context.GroupSessions.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
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
                TotalContributed = 0,
                IsDeleted = false
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
            var s = await _context.GroupSessions.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            if (s == null) return NotFound();
            if (dto.TargetAmount.HasValue) s.TargetAmount = dto.TargetAmount.Value;
            if (dto.TargetDate.HasValue) s.TargetDate = dto.TargetDate.Value;
            if (dto.StartDate.HasValue) s.StartDate = dto.StartDate.Value;
            if (dto.Frequency != null) s.Frequency = dto.Frequency;
            if (dto.Status != null) s.Status = dto.Status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroupSession(Guid id)
        {
            var s = await _context.GroupSessions.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            if (s == null) return NotFound();
            s.IsDeleted = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

