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

        public TelegramController(AppDbContext db, TelegramOtpService otpService)
        {
            _db = db;
            _otpService = otpService;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] TelegramUpdate update)
        {
            try
            {
                var message = update?.Message;

                if (message == null || string.IsNullOrWhiteSpace(message.Text))
                    return Ok();

                if (!message.Text.StartsWith("/start"))
                    return Ok();

                var parts = message.Text.Split(' ');
                if (parts.Length < 2)
                    return Ok();

                string token = parts[1].Trim();
                long chatId = message.Chat.Id;

                Console.WriteLine($"TOKEN = {token}");
                Console.WriteLine($"CHAT ID = {chatId}");

                // -------------------------------------------------
                // 1. CHECK IF TELEGRAM ALREADY LINKED
                // -------------------------------------------------
                var alreadyLinked = await _db.UserTelegramLinks
                    .AnyAsync(x => x.TelegramChatId == chatId);

                if (alreadyLinked)
                {
                    await _otpService.SendCustomMessageAsync(
                        chatId,
                        "စာရင်းသွင်းပြီးသားဖြစ်ပါသည်။");

                    return Ok();
                }

                // -------------------------------------------------
                // 2. FIND PENDING REGISTRATION TOKEN
                // -------------------------------------------------
                var pending = await _db.PendingTelegramLinks
                    .FirstOrDefaultAsync(x =>
                        x.Token == token &&
                        !x.IsUsed &&
                        x.ExpiresAt > DateTime.UtcNow);

                if (pending == null)
                {
                    await _otpService.SendCustomMessageAsync(
                        chatId,
                        "လင့်ခ်မမှန်ပါ သို့မဟုတ် သက်တမ်းကုန်သွားပါပြီ။");

                    return Ok();
                }

                // -------------------------------------------------
                // 3. CREATE LINK (FINAL REGISTRATION STEP)
                // -------------------------------------------------
                _db.UserTelegramLinks.Add(new UserTelegramLink
                {
                    PhoneNumber = pending.PhoneNumber,
                    TelegramChatId = chatId,
                    CreatedAt = DateTime.UtcNow
                });

                // mark token used
                pending.IsUsed = true;

                await _db.SaveChangesAsync();

                // -------------------------------------------------
                // 4. SEND OTP (ONLY FOR REGISTRATION CONFIRMATION)
                // -------------------------------------------------
                await _otpService.SendOtpAsync(chatId);

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Ok();
            }
        }
    }

    // -------------------------------------------------
    // DTOs
    // -------------------------------------------------
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