using GroupSavingsApi.Data;
using GroupSavingsApi.DTOs;
using GroupSavingsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupSavingsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentMethodController : ControllerBase
    {
        private readonly GroupSavingsDbContext _context;

        public PaymentMethodController(GroupSavingsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentMethodResponseDto>>> GetPaymentMethods()
        {
            var methods = await _context.PaymentMethods.Select(p => new PaymentMethodResponseDto
            {
                Id = p.Id,
                CustomerId = p.CustomerId,
                AccountTypeId = p.AccountTypeId,
                ProviderName = p.ProviderName,
                AccountName = p.AccountName,
                AccountNumber = p.AccountNumber,
                IsPrimary = p.IsPrimary,
                Currency = p.Currency,
                CountryCode = p.CountryCode,
                CreatedAt = p.CreatedAt
            }).ToListAsync();
            return Ok(methods);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentMethodResponseDto>> GetPaymentMethod(Guid id)
        {
            var p = await _context.PaymentMethods.FindAsync(id);
            if (p == null) return NotFound();
            return Ok(new PaymentMethodResponseDto
            {
                Id = p.Id,
                CustomerId = p.CustomerId,
                AccountTypeId = p.AccountTypeId,
                ProviderName = p.ProviderName,
                AccountName = p.AccountName,
                AccountNumber = p.AccountNumber,
                IsPrimary = p.IsPrimary,
                Currency = p.Currency,
                CountryCode = p.CountryCode,
                CreatedAt = p.CreatedAt
            });
        }

        [HttpPost]
        public async Task<ActionResult<PaymentMethodResponseDto>> CreatePaymentMethod(CreatePaymentMethodDto dto)
        {
            var method = new PaymentMethod
            {
                Id = Guid.NewGuid(),
                CustomerId = dto.CustomerId,
                AccountTypeId = dto.AccountTypeId,
                ProviderName = dto.ProviderName,
                AccountName = dto.AccountName,
                AccountNumber = dto.AccountNumber,
                IsPrimary = dto.IsPrimary,
                Currency = dto.Currency,
                CountryCode = dto.CountryCode,
                CreatedAt = DateTime.UtcNow
            };
            _context.PaymentMethods.Add(method);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPaymentMethod), new { id = method.Id }, new PaymentMethodResponseDto
            {
                Id = method.Id,
                CustomerId = method.CustomerId,
                AccountTypeId = method.AccountTypeId,
                ProviderName = method.ProviderName,
                AccountName = method.AccountName,
                AccountNumber = method.AccountNumber,
                IsPrimary = method.IsPrimary,
                Currency = method.Currency,
                CountryCode = method.CountryCode,
                CreatedAt = method.CreatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePaymentMethod(Guid id, UpdatePaymentMethodDto dto)
        {
            var p = await _context.PaymentMethods.FindAsync(id);
            if (p == null) return NotFound();
            if (dto.ProviderName != null) p.ProviderName = dto.ProviderName;
            if (dto.AccountName != null) p.AccountName = dto.AccountName;
            if (dto.AccountNumber != null) p.AccountNumber = dto.AccountNumber;
            if (dto.IsPrimary.HasValue) p.IsPrimary = dto.IsPrimary.Value;
            if (dto.Currency != null) p.Currency = dto.Currency;
            if (dto.CountryCode != null) p.CountryCode = dto.CountryCode;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentMethod(Guid id)
        {
            var p = await _context.PaymentMethods.FindAsync(id);
            if (p == null) return NotFound();
            _context.PaymentMethods.Remove(p);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

