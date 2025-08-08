using GroupSavingsApi.Data;
using GroupSavingsApi.DTOs;
using GroupSavingsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroupSavingsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly GroupSavingsDbContext _context;

        public NotificationController(GroupSavingsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetNotifications()
        {
            var notifications = await _context.Notifications.Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Message = n.Message,
                Type = n.Type,
                CreatedAt = n.CreatedAt
            }).ToListAsync();
            return Ok(notifications);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationResponseDto>> GetNotification(Guid id)
        {
            var n = await _context.Notifications.FindAsync(id);
            if (n == null) return NotFound();
            return Ok(new NotificationResponseDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Message = n.Message,
                Type = n.Type,
                CreatedAt = n.CreatedAt
            });
        }

        [HttpPost]
        public async Task<ActionResult<NotificationResponseDto>> CreateNotification(CreateNotificationDto dto)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                Message = dto.Message,
                Type = dto.Type,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, new NotificationResponseDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Message = notification.Message,
                Type = notification.Type,
                CreatedAt = notification.CreatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotification(Guid id, UpdateNotificationDto dto)
        {
            var n = await _context.Notifications.FindAsync(id);
            if (n == null) return NotFound();
            if (dto.Message != null) n.Message = dto.Message;
            if (dto.Type != null) n.Type = dto.Type;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var n = await _context.Notifications.FindAsync(id);
            if (n == null) return NotFound();
            _context.Notifications.Remove(n);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

