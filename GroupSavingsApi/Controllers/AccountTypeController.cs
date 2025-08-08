using GroupSavingsApi.Data;
using GroupSavingsApi.DTOs;
using GroupSavingsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupSavingsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountTypeController : ControllerBase
    {
        private readonly GroupSavingsDbContext _context;

        public AccountTypeController(GroupSavingsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountTypeResponseDto>>> GetAccountTypes()
        {
            var types = await _context.AccountTypes.Select(a => new AccountTypeResponseDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description
            }).ToListAsync();
            return Ok(types);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AccountTypeResponseDto>> GetAccountType(int id)
        {
            var a = await _context.AccountTypes.FindAsync(id);
            if (a == null) return NotFound();
            return Ok(new AccountTypeResponseDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description
            });
        }

        [HttpPost]
        public async Task<ActionResult<AccountTypeResponseDto>> CreateAccountType(CreateAccountTypeDto dto)
        {
            var type = new AccountType
            {
                Name = dto.Name,
                Description = dto.Description
            };
            _context.AccountTypes.Add(type);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAccountType), new { id = type.Id }, new AccountTypeResponseDto
            {
                Id = type.Id,
                Name = type.Name,
                Description = type.Description
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccountType(int id, UpdateAccountTypeDto dto)
        {
            var a = await _context.AccountTypes.FindAsync(id);
            if (a == null) return NotFound();
            if (dto.Name != null) a.Name = dto.Name;
            if (dto.Description != null) a.Description = dto.Description;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccountType(int id)
        {
            var a = await _context.AccountTypes.FindAsync(id);
            if (a == null) return NotFound();
            _context.AccountTypes.Remove(a);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

