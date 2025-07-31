using GroupSavingsApi.Data;
using GroupSavingsApi.DTOs;
using GroupSavingsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupSavingsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogController : ControllerBase
    {
        private readonly GroupSavingsDbContext _context;

        public AuditLogController(GroupSavingsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuditLogResponseDto>>> GetAuditLogs()
        {
            var logs = await _context.Set<AuditLog>()
                .OrderByDescending(l => l.Timestamp)
                .Select(l => new AuditLogResponseDto
                {
                    Id = l.Id,
                    GroupId = l.GroupId,
                    MemberId = l.MemberId,
                    UserId = l.UserId,
                    Action = l.Action,
                    EntityType = l.EntityType,
                    EntityId = l.EntityId,
                    Timestamp = l.Timestamp,
                    Details = l.Details
                }).ToListAsync();
            return Ok(logs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuditLogResponseDto>> GetAuditLog(Guid id)
        {
            var l = await _context.Set<AuditLog>().FindAsync(id);
            if (l == null) return NotFound();
            return Ok(new AuditLogResponseDto
            {
                Id = l.Id,
                GroupId = l.GroupId,
                MemberId = l.MemberId,
                UserId = l.UserId,
                Action = l.Action,
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                Timestamp = l.Timestamp,
                Details = l.Details
            });
        }

        [HttpPost]
        public async Task<ActionResult<AuditLogResponseDto>> CreateAuditLog(CreateAuditLogDto dto)
        {
            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                GroupId = dto.GroupId,
                MemberId = dto.MemberId,
                UserId = dto.UserId,
                Action = dto.Action,
                EntityType = dto.EntityType,
                EntityId = dto.EntityId,
                Details = dto.Details,
                Timestamp = DateTime.UtcNow
            };
            _context.Set<AuditLog>().Add(log);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAuditLog), new { id = log.Id }, new AuditLogResponseDto
            {
                Id = log.Id,
                GroupId = log.GroupId,
                MemberId = log.MemberId,
                UserId = log.UserId,
                Action = log.Action,
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                Timestamp = log.Timestamp,
                Details = log.Details
            });
        }
    }
}
