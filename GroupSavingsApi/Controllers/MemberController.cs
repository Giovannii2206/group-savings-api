using GroupSavingsApi.Data;
using GroupSavingsApi.DTOs;
using GroupSavingsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupSavingsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemberController : ControllerBase
    {
        private readonly GroupSavingsDbContext _context;

        public MemberController(GroupSavingsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberResponseDto>>> GetMembers()
        {
            var members = await _context.Members
                .Where(m => !m.IsDeleted)
                .Select(m => new MemberResponseDto
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                    Phone = m.Phone,
                    Address = m.Address,
                    DateOfBirth = m.DateOfBirth,
                    Gender = m.Gender,
                    AccountBalance = m.AccountBalance
                }).ToListAsync();
            return Ok(members);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MemberResponseDto>> GetMember(Guid id)
        {
            var m = await _context.Members.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
            if (m == null) return NotFound();
            return Ok(new MemberResponseDto
            {
                Id = m.Id,
                UserId = m.UserId,
                FirstName = m.FirstName,
                LastName = m.LastName,
                Phone = m.Phone,
                Address = m.Address,
                DateOfBirth = m.DateOfBirth,
                Gender = m.Gender,
                AccountBalance = m.AccountBalance
            });
        }

        [HttpPost]
        public async Task<ActionResult<MemberResponseDto>> CreateMember(CreateMemberDto dto)
        {
            var member = new Member
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                Address = dto.Address,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                AccountBalance = 0,
                IsDeleted = false
            };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMember), new { id = member.Id }, new MemberResponseDto
            {
                Id = member.Id,
                UserId = member.UserId,
                FirstName = member.FirstName,
                LastName = member.LastName,
                Phone = member.Phone,
                Address = member.Address,
                DateOfBirth = member.DateOfBirth,
                Gender = member.Gender,
                AccountBalance = member.AccountBalance
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMember(Guid id, UpdateMemberDto dto)
        {
            var m = await _context.Members.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
            if (m == null) return NotFound();
            if (dto.FirstName != null) m.FirstName = dto.FirstName;
            if (dto.LastName != null) m.LastName = dto.LastName;
            if (dto.Phone != null) m.Phone = dto.Phone;
            if (dto.Address != null) m.Address = dto.Address;
            if (dto.DateOfBirth.HasValue) m.DateOfBirth = dto.DateOfBirth.Value;
            if (dto.Gender != null) m.Gender = dto.Gender;
            if (dto.AccountBalance.HasValue) m.AccountBalance = dto.AccountBalance.Value;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(Guid id)
        {
            var m = await _context.Members.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
            if (m == null) return NotFound();
            m.IsDeleted = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

