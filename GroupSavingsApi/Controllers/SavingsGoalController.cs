using GroupSavingsApi.Data;
using GroupSavingsApi.DTOs;
using GroupSavingsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupSavingsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavingsGoalController : ControllerBase
    {
        private readonly GroupSavingsDbContext _context;

        public SavingsGoalController(GroupSavingsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SavingsGoalResponseDto>>> GetGoals()
        {
            var goals = await _context.Set<SavingsGoal>()
                .Where(g => !g.IsDeleted)
                .Select(g => new SavingsGoalResponseDto
                {
                    Id = g.Id,
                    GroupSessionId = g.GroupSessionId,
                    Name = g.Name,
                    TargetAmount = g.TargetAmount,
                    CurrentAmount = g.CurrentAmount,
                    TargetDate = g.TargetDate
                }).ToListAsync();
            return Ok(goals);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SavingsGoalResponseDto>> GetGoal(Guid id)
        {
            var g = await _context.Set<SavingsGoal>().FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
            if (g == null) return NotFound();
            return Ok(new SavingsGoalResponseDto
            {
                Id = g.Id,
                GroupSessionId = g.GroupSessionId,
                Name = g.Name,
                TargetAmount = g.TargetAmount,
                CurrentAmount = g.CurrentAmount,
                TargetDate = g.TargetDate
            });
        }

        [HttpPost]
        public async Task<ActionResult<SavingsGoalResponseDto>> CreateGoal(CreateSavingsGoalDto dto)
        {
            var goal = new SavingsGoal
            {
                Id = Guid.NewGuid(),
                GroupSessionId = dto.GroupSessionId,
                Name = dto.Name,
                TargetAmount = dto.TargetAmount,
                CurrentAmount = 0,
                TargetDate = dto.TargetDate,
                IsDeleted = false
            };
            _context.Set<SavingsGoal>().Add(goal);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGoal), new { id = goal.Id }, new SavingsGoalResponseDto
            {
                Id = goal.Id,
                GroupSessionId = goal.GroupSessionId,
                Name = goal.Name,
                TargetAmount = goal.TargetAmount,
                CurrentAmount = goal.CurrentAmount,
                TargetDate = goal.TargetDate
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGoal(Guid id, UpdateSavingsGoalDto dto)
        {
            var g = await _context.Set<SavingsGoal>().FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
            if (g == null) return NotFound();
            if (dto.Name != null) g.Name = dto.Name;
            if (dto.TargetAmount.HasValue) g.TargetAmount = dto.TargetAmount.Value;
            if (dto.CurrentAmount.HasValue) g.CurrentAmount = dto.CurrentAmount.Value;
            if (dto.TargetDate.HasValue) g.TargetDate = dto.TargetDate.Value;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGoal(Guid id)
        {
            var g = await _context.Set<SavingsGoal>().FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
            if (g == null) return NotFound();
            g.IsDeleted = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

