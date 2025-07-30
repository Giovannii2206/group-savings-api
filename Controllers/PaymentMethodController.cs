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
            var method = await _context.PaymentMethods.FindAsync(id);
            if (method == null) return NotFound();
            if (dto.ProviderName != null) method.ProviderName = dto.ProviderName;
            if (dto.AccountName != null) method.AccountName = dto.AccountName;
            if (dto.AccountNumber != null) method.AccountNumber = dto.AccountNumber;
            if (dto.Currency != null) method.Currency = dto.Currency;
            if (dto.CountryCode != null) method.CountryCode = dto.CountryCode;
            if (dto.IsPrimary.HasValue) method.IsPrimary = dto.IsPrimary.Value;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentMethod(Guid id)
        {
            var method = await _context.PaymentMethods.FindAsync(id);
            if (method == null) return NotFound();
            _context.PaymentMethods.Remove(method);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
