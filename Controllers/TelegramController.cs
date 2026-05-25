using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;
using TwoDPro3.Services;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly TelegramOtpService _otpService;

        public TelegramController(
            AppDbContext db,
            TelegramOtpService otpService)
        {
            _db = db;
            _otpService = otpService;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook(
            [FromBody] TelegramUpdate update)
        {
            try
            {
                Console.WriteLine("WEBHOOK HIT");

                var message = update?.Message;

                Console.WriteLine("RAW MESSAGE:");
                Console.WriteLine(message?.Text);

                if (message == null ||
                    string.IsNullOrWhiteSpace(message.Text))
                {
                    return Ok();
                }

                Console.WriteLine(message.Text);

                if (!message.Text.StartsWith("/start"))
                    return Ok();

                var parts = message.Text.Split(' ');

                if (parts.Length < 2)
                    return Ok();

                string phone = parts[1].Trim();

                long chatId = message.Chat.Id;

                Console.WriteLine($"PHONE = {phone}");
                Console.WriteLine($"CHAT ID = {chatId}");

                // ----------------------------------------
                // SAVE LINK
                // ----------------------------------------

                var existing = await _db.UserTelegramLinks
                    .FirstOrDefaultAsync(x =>
                        x.PhoneNumber == phone);

                if (existing == null)
                {
                    _db.UserTelegramLinks.Add(
                        new UserTelegramLink
                        {
                            PhoneNumber = phone,
                            TelegramChatId = chatId,
                            CreatedAt = DateTime.UtcNow
                        });
                }
                else
                {
                    existing.TelegramChatId = chatId;
                }

                await _db.SaveChangesAsync();

                Console.WriteLine("LINK SAVED");

                // ----------------------------------------
                // SEND OTP
                // ----------------------------------------

                Console.WriteLine("CALLING OTP SERVICE");

                var sent = await _otpService.SendOtpAsync(chatId);

                Console.WriteLine($"OTP SENT RESULT = {sent}");

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return Ok();
            }
        }
    }

    // ----------------------------------------
    // DTOs
    // ----------------------------------------

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