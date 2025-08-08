using GroupSavingsApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupSavingsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly GroupSavingsDbContext _context;
        public HealthController(GroupSavingsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("SELECT 1");
                return Ok(new { status = "Healthy", database = "Connected" });
            }
            catch
            {
                return StatusCode(503, new { status = "Unhealthy", database = "Disconnected" });
            }
        }
    }
}

