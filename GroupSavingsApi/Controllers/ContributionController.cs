using GroupSavingsApi.Data;
using GroupSavingsApi.DTOs;
using GroupSavingsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupSavingsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContributionController : ControllerBase
    {
        private readonly GroupSavingsDbContext _context;

        public ContributionController(GroupSavingsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContributionResponseDto>>> GetContributions()
        {
            var contributions = await _context.Contributions
                .Where(c => !c.IsDeleted)
                .Select(c => new ContributionResponseDto
                {
                    Id = c.Id,
                    GroupSessionId = c.GroupSessionId,
                    UserId = c.UserId,
                    Amount = c.Amount,
                    Date = c.Date,
                    Type = c.Type
                }).ToListAsync();
            return Ok(contributions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ContributionResponseDto>> GetContribution(Guid id)
        {
            var c = await _context.Contributions.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (c == null) return NotFound();
            return Ok(new ContributionResponseDto
            {
                Id = c.Id,
                GroupSessionId = c.GroupSessionId,
                UserId = c.UserId,
                Amount = c.Amount,
                Date = c.Date,
                Type = c.Type
            });
        }

        [HttpPost]
        public async Task<ActionResult<ContributionResponseDto>> CreateContribution(CreateContributionDto dto)
        {
            var contribution = new Contribution
            {
                Id = Guid.NewGuid(),
                GroupSessionId = dto.GroupSessionId,
                UserId = dto.UserId,
                Amount = dto.Amount,
                Date = DateTime.UtcNow,
                Type = dto.Type,
                IsDeleted = false
            };
            _context.Contributions.Add(contribution);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetContribution), new { id = contribution.Id }, new ContributionResponseDto
            {
                Id = contribution.Id,
                GroupSessionId = contribution.GroupSessionId,
                UserId = contribution.UserId,
                Amount = contribution.Amount,
                Date = contribution.Date,
                Type = contribution.Type
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContribution(Guid id, UpdateContributionDto dto)
        {
            var c = await _context.Contributions.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (c == null) return NotFound();
            if (dto.Amount.HasValue) c.Amount = dto.Amount.Value;
            if (!string.IsNullOrEmpty(dto.Type)) c.Type = dto.Type;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContribution(Guid id)
        {
            var c = await _context.Contributions.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (c == null) return NotFound();
            c.IsDeleted = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

