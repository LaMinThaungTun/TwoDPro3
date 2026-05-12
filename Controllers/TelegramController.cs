using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TelegramController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] TelegramUpdate update)
        {
            var message = update?.Message;

            if (message == null || string.IsNullOrWhiteSpace(message.Text))
                return Ok();

            if (!message.Text.StartsWith("/start"))
                return Ok();

            var parts = message.Text.Split(' ');

            if (parts.Length < 2)
                return Ok();

            string phone = parts[1];

            long chatId = message.Chat.Id;

            // check existing
            var existing = await _db.UserTelegramLinks
                .FirstOrDefaultAsync(x => x.PhoneNumber == phone);

            if (existing == null)
            {
                _db.UserTelegramLinks.Add(new UserTelegramLink
                {
                    PhoneNumber = phone,
                    TelegramChatId = chatId
                });
            }
            else
            {
                existing.TelegramChatId = chatId;
            }

            await _db.SaveChangesAsync();

            return Ok();
        }
    }

    // ----------------------
    // Telegram DTOs

    public class TelegramUpdate
    {
        public TelegramMessage? Message { get; set; }
    }

    public class TelegramMessage
    {
        public TelegramChat Chat { get; set; } = new();

        public string? Text { get; set; }
    }

    public class TelegramChat
    {
        public long Id { get; set; }
    }
}