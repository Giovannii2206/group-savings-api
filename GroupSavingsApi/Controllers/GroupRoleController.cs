using GroupSavingsApi.Data;
using GroupSavingsApi.DTOs;
using GroupSavingsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupSavingsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupRoleController : ControllerBase
    {
        private readonly GroupSavingsDbContext _context;

        public GroupRoleController(GroupSavingsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupRoleResponseDto>>> GetGroupRoles()
        {
            var roles = await _context.GroupRoles.Select(r => new GroupRoleResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description
            }).ToListAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GroupRoleResponseDto>> GetGroupRole(int id)
        {
            var r = await _context.GroupRoles.FindAsync(id);
            if (r == null) return NotFound();
            return Ok(new GroupRoleResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description
            });
        }

        [HttpPost]
        public async Task<ActionResult<GroupRoleResponseDto>> CreateGroupRole(CreateGroupRoleDto dto)
        {
            var role = new GroupRole
            {
                Name = dto.Name,
                Description = dto.Description
            };
            _context.GroupRoles.Add(role);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGroupRole), new { id = role.Id }, new GroupRoleResponseDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroupRole(int id, UpdateGroupRoleDto dto)
        {
            var r = await _context.GroupRoles.FindAsync(id);
            if (r == null) return NotFound();
            if (dto.Name != null) r.Name = dto.Name;
            if (dto.Description != null) r.Description = dto.Description;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroupRole(int id)
        {
            var r = await _context.GroupRoles.FindAsync(id);
            if (r == null) return NotFound();
            _context.GroupRoles.Remove(r);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

