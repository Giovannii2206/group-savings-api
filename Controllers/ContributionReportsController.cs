using GroupSavingsApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GroupSavingsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContributionReportsController : ControllerBase
    {
        private readonly GroupSavingsDbContext _context;
        public ContributionReportsController(GroupSavingsDbContext context)
        {
            _context = context;
        }

        // GET: api/ContributionReports/ByUser
        [HttpGet("ByUser")]
        public async Task<IActionResult> GetContributionsByUser()
        {
            var report = await _context.Contributions
                .Where(c => !c.IsDeleted)
                .GroupBy(c => c.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalContributed = g.Sum(x => x.Amount),
                    Count = g.Count()
                })
                .ToListAsync();
            return Ok(report);
        }

        // GET: api/ContributionReports/ByGroup
        [HttpGet("ByGroup")]
        public async Task<IActionResult> GetContributionsByGroup()
        {
            var report = await _context.Contributions
                .Where(c => !c.IsDeleted)
                .Include(c => c.GroupSession)
                .GroupBy(c => c.GroupSession.GroupId)
                .Select(g => new
                {
                    GroupId = g.Key,
                    TotalContributed = g.Sum(x => x.Amount),
                    Count = g.Count()
                })
                .ToListAsync();
            return Ok(report);
        }

        // GET: api/ContributionReports/BySession
        [HttpGet("BySession")]
        public async Task<IActionResult> GetContributionsBySession()
        {
            var report = await _context.Contributions
                .Where(c => !c.IsDeleted)
                .GroupBy(c => c.GroupSessionId)
                .Select(g => new
                {
                    GroupSessionId = g.Key,
                    TotalContributed = g.Sum(x => x.Amount),
                    Count = g.Count()
                })
                .ToListAsync();
            return Ok(report);
        }
    }
}
