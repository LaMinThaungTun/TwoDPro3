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

        public TelegramOtpService(
            IConfiguration config,
            AppDbContext db)
        {
            _config = config;
            _db = db;
        }

        // --------------------------------------------------
        // GENERATE + SEND OTP
        // --------------------------------------------------

        public async Task<bool> SendOtpAsync(long telegramChatId)
        {
            try
            {
                Console.WriteLine("========== SEND OTP START ==========");

                // ----------------------------------------
                // OPTIONAL: prevent OTP spam
                // ----------------------------------------

                var recentOtp = await _db.OtpCodes
                    .Where(x =>
                        x.TelegramChatId == telegramChatId &&
                        !x.IsUsed &&
                        x.ExpiresAt > DateTime.UtcNow)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (recentOtp != null)
                {
                    Console.WriteLine("Existing valid OTP found.");

                    return await SendTelegramMessageAsync(
                        telegramChatId,
                        $"Your OTP code is: {recentOtp.Code}\nExpires in 3 minutes.");
                }

                // ----------------------------------------
                // Generate secure 6-digit OTP
                // ----------------------------------------

                var otp = RandomNumberGenerator
                    .GetInt32(100000, 999999)
                    .ToString();

                Console.WriteLine($"OTP GENERATED: {otp}");

                // ----------------------------------------
                // Save OTP to database
                // ----------------------------------------

                var otpEntry = new OtpCode
                {
                    TelegramChatId = telegramChatId,
                    Code = otp,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(3),
                    IsUsed = false
                };

                _db.OtpCodes.Add(otpEntry);

                await _db.SaveChangesAsync();

                Console.WriteLine("OTP SAVED TO DATABASE");

                // ----------------------------------------
                // Send Telegram message
                // ----------------------------------------

                var sent = await SendTelegramMessageAsync(
                    telegramChatId,
                    $"Your OTP code is: {otp}\nExpires in 3 minutes.");

                Console.WriteLine($"TELEGRAM SEND RESULT: {sent}");

                Console.WriteLine("========== SEND OTP END ==========");

                return sent;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SEND OTP ERROR:");
                Console.WriteLine(ex.ToString());

                return false;
            }
        }

        // --------------------------------------------------
        // VERIFY OTP
        // --------------------------------------------------

        public async Task<bool> VerifyOtpAsync(
            long telegramChatId,
            string code)
        {
            try
            {
                Console.WriteLine("========== VERIFY OTP START ==========");

                Console.WriteLine($"CHAT ID: {telegramChatId}");
                Console.WriteLine($"CODE: {code}");

                var otp = await _db.OtpCodes
                    .Where(x =>
                        x.TelegramChatId == telegramChatId &&
                        x.Code == code &&
                        !x.IsUsed &&
                        x.ExpiresAt > DateTime.UtcNow)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (otp == null)
                {
                    Console.WriteLine("OTP NOT FOUND OR INVALID");

                    return false;
                }

                otp.IsUsed = true;

                await _db.SaveChangesAsync();

                Console.WriteLine("OTP VERIFIED SUCCESSFULLY");

                Console.WriteLine("========== VERIFY OTP END ==========");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("VERIFY OTP ERROR:");
                Console.WriteLine(ex.ToString());

                return false;
            }
        }

        // --------------------------------------------------
        // SEND TELEGRAM MESSAGE
        // --------------------------------------------------

        private async Task<bool> SendTelegramMessageAsync(
            long chatId,
            string text)
        {
            try
            {
                var botToken = _config["Telegram:BotToken"];

                if (string.IsNullOrWhiteSpace(botToken))
                {
                    Console.WriteLine("BOT TOKEN NOT FOUND");

                    return false;
                }

                var url =
                    $"https://api.telegram.org/bot{botToken}/sendMessage";

                using var client = new HttpClient();

                var payload = new
                {
                    chat_id = chatId,
                    text = text
                };

                var json = JsonSerializer.Serialize(payload);

                Console.WriteLine("SENDING TELEGRAM MESSAGE...");
                Console.WriteLine(json);

                var response = await client.PostAsync(
                    url,
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json"));

                var responseText =
                    await response.Content.ReadAsStringAsync();

                Console.WriteLine($"TELEGRAM STATUS: {response.StatusCode}");
                Console.WriteLine(responseText);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine("TELEGRAM MESSAGE ERROR:");
                Console.WriteLine(ex.ToString());

                return false;
            }
        }
    }
}