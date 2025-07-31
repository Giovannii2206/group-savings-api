using GroupSavingsApi.Data;
using GroupSavingsApi.DTOs;
using GroupSavingsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupSavingsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupController : ControllerBase
    {
        private readonly GroupSavingsDbContext _context;

        public GroupController(GroupSavingsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupResponseDto>>> GetGroups([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? name = null)
        {
            var query = _context.Groups.Where(g => !g.IsDeleted);
            if (!string.IsNullOrEmpty(name)) query = query.Where(g => g.Name.Contains(name));
            var groups = await query
                .OrderByDescending(g => g.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(g => new GroupResponseDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    CreatedBy = g.CreatedBy,
                    Status = g.Status,
                    CreatedAt = g.CreatedAt
                }).ToListAsync();
            return Ok(groups);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GroupResponseDto>> GetGroup(Guid id)
        {
            var g = await _context.Groups.FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
            if (g == null) return NotFound();
            return Ok(new GroupResponseDto
            {
                Id = g.Id,
                Name = g.Name,
                CreatedBy = g.CreatedBy,
                Status = g.Status,
                CreatedAt = g.CreatedAt
            });
        }

        [HttpPost]
        public async Task<ActionResult<GroupResponseDto>> CreateGroup(CreateGroupDto dto)
        {
            var group = new Group
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                CreatedBy = dto.CreatedBy,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, new GroupResponseDto
            {
                Id = group.Id,
                Name = group.Name,
                CreatedBy = group.CreatedBy,
                Status = group.Status,
                CreatedAt = group.CreatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroup(Guid id, UpdateGroupDto dto)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
            if (group == null) return NotFound();
            if (dto.Name != null) group.Name = dto.Name;
            if (dto.Status != null) group.Status = dto.Status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(Guid id)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
            if (group == null) return NotFound();
            group.IsDeleted = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
