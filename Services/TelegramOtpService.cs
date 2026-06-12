using System.Security.Cryptography;
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

        // -------------------------------------------------
        // OTP GENERATION (ONLY FOR REGISTRATION CONFIRMATION)
        // -------------------------------------------------
        public async Task<bool> SendOtpAsync(long chatId)
        {
            try
            {
                var otp = RandomNumberGenerator
                    .GetInt32(100000, 999999)
                    .ToString();

                var otpEntry = new OtpCode
                {
                    TelegramChatId = chatId,
                    Code = otp,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(3),
                    IsUsed = false
                };

                _db.OtpCodes.Add(otpEntry);
                await _db.SaveChangesAsync();

                return await SendTelegramMessageAsync(
                    chatId,
                    $"Your OTP code: {otp}\nValid for 3 minutes.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        // -------------------------------------------------
        // VERIFY OTP (REGISTRATION ONLY)
        // -------------------------------------------------
        public async Task<bool> VerifyOtpAsync(long chatId, string code)
        {
            var otp = await _db.OtpCodes
                .FirstOrDefaultAsync(x =>
                    x.TelegramChatId == chatId &&
                    x.Code == code &&
                    !x.IsUsed &&
                    x.ExpiresAt > DateTime.UtcNow);

            if (otp == null)
                return false;

            otp.IsUsed = true;
            await _db.SaveChangesAsync();

            return true;
        }

        // -------------------------------------------------
        // SIMPLE MESSAGE SENDER
        // -------------------------------------------------
        public async Task<bool> SendCustomMessageAsync(long chatId, string text)
        {
            return await SendTelegramMessageAsync(chatId, text);
        }

        // -------------------------------------------------
        // INTERNAL TELEGRAM API CALL
        // -------------------------------------------------
        private async Task<bool> SendTelegramMessageAsync(long chatId, string text)
        {
            var botToken = _config["Telegram:BotToken"];

            if (string.IsNullOrWhiteSpace(botToken))
                return false;

            var url = $"https://api.telegram.org/bot{botToken}/sendMessage";

            using var client = new HttpClient();

            var payload = new
            {
                chat_id = chatId,
                text = text
            };

            var json = JsonSerializer.Serialize(payload);

            var response = await client.PostAsync(
                url,
                new StringContent(json, Encoding.UTF8, "application/json"));

            return response.IsSuccessStatusCode;
        }
    }
}