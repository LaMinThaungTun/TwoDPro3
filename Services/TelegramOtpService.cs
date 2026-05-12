using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Services
{
    public class TelegramOtpService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;

        public TelegramOtpService(IConfiguration config, AppDbContext db)
        {
            _config = config;
            _db = db;
        }

        public async Task<bool> SendOtpAsync(long telegramChatId)
        {
            try
            {
                var otp = new Random().Next(100000, 999999).ToString();

                var otpEntry = new OtpCode
                {
                    TelegramChatId = telegramChatId,
                    Code = otp,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(3),
                    IsUsed = false
                };

                _db.OtpCodes.Add(otpEntry);

                await _db.SaveChangesAsync();

                var botToken = _config["Telegram:BotToken"];

                var url = $"https://api.telegram.org/bot{botToken}/sendMessage";

                using var client = new HttpClient();

                var payload = new
                {
                    chat_id = telegramChatId,
                    text = $"Your OTP code is: {otp}\nExpires in 3 minutes."
                };

                var json = JsonSerializer.Serialize(payload);

                var response = await client.PostAsync(
                    url,
                    new StringContent(json, Encoding.UTF8, "application/json")
                );

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> VerifyOtpAsync(long telegramChatId, string code)
        {
            var otp = await _db.OtpCodes
                .Where(x =>
                    x.TelegramChatId == telegramChatId &&
                    x.Code == code &&
                    !x.IsUsed &&
                    x.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null)
                return false;

            otp.IsUsed = true;

            await _db.SaveChangesAsync();

            return true;
        }
    }
}