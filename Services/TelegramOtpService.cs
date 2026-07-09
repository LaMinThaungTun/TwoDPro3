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
        private readonly CalendarContext _db;


        public TelegramOtpService(
            IConfiguration config,
            CalendarContext db)
        {
            _config = config;
            _db = db;
        }



        // =====================================================
        // GENERATE OTP AND SEND TO TELEGRAM
        // =====================================================

        public async Task<bool> SendOtpAsync(
            long chatId,
            string phoneNumber)
        {
            try
            {
                var otp =
                    RandomNumberGenerator
                    .GetInt32(100000, 999999)
                    .ToString();



                var otpEntry =
                    new OtpCode
                    {
                        TelegramChatId = chatId,

                        PhoneNumber = phoneNumber,

                        Code = otp,

                        CreatedAt =
                            DateTime.UtcNow,

                        ExpiresAt =
                            DateTime.UtcNow
                            .AddMinutes(3),

                        IsUsed = false
                    };



                _db.OtpCodes.Add(otpEntry);


                await _db.SaveChangesAsync();



                Console.WriteLine(
                    "========== OTP CREATED ==========");

                Console.WriteLine(
                    $"Chat ID : {chatId}");

                Console.WriteLine(
                    $"Phone   : {phoneNumber}");

                Console.WriteLine(
                    $"OTP     : {otp}");



                return await SendTelegramMessageAsync(
                    chatId,
                    $"Your OTP code: {otp}\n\n" +
                    "Valid for 3 minutes.");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return false;
            }
        }





        // =====================================================
        // VERIFY OTP
        // =====================================================

        public async Task<bool> VerifyOtpAsync(
            long chatId,
            string phoneNumber,
            string code)
        {
            try
            {
                Console.WriteLine(
                    "========== VERIFY OTP ==========");

                Console.WriteLine(
                    $"Chat ID : {chatId}");

                Console.WriteLine(
                    $"Phone   : {phoneNumber}");

                Console.WriteLine(
                    $"Code    : {code}");



                var otp =
                    await _db.OtpCodes
                    .FirstOrDefaultAsync(x =>
                        x.TelegramChatId == chatId &&
                        x.PhoneNumber == phoneNumber &&
                        x.Code == code &&
                        !x.IsUsed &&
                        x.ExpiresAt > DateTime.UtcNow);



                if (otp == null)
                {
                    Console.WriteLine(
                        "OTP INVALID");

                    return false;
                }



                otp.IsUsed = true;


                await _db.SaveChangesAsync();



                Console.WriteLine(
                    "OTP VERIFIED");


                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return false;
            }
        }





        // =====================================================
        // SEND CUSTOM TELEGRAM MESSAGE
        // =====================================================

        public async Task<bool> SendCustomMessageAsync(
            long chatId,
            string text)
        {
            return await SendTelegramMessageAsync(
                chatId,
                text);
        }





        // =====================================================
        // TELEGRAM API CALL
        // =====================================================

        private async Task<bool> SendTelegramMessageAsync(
            long chatId,
            string text)
        {
            try
            {
                var botToken =
                    _config["Telegram:BotToken"];



                if (string.IsNullOrWhiteSpace(botToken))
                {
                    Console.WriteLine(
                        "Telegram Bot Token missing");

                    return false;
                }



                var url =
                    $"https://api.telegram.org/bot{botToken}/sendMessage";



                using var client =
                    new HttpClient();



                var payload =
                    new
                    {
                        chat_id = chatId,

                        text = text
                    };



                var json =
                    JsonSerializer.Serialize(payload);



                var response =
                    await client.PostAsync(
                        url,
                        new StringContent(
                            json,
                            Encoding.UTF8,
                            "application/json"));



                if (!response.IsSuccessStatusCode)
                {
                    var error =
                        await response.Content
                        .ReadAsStringAsync();


                    Console.WriteLine(
                        $"Telegram Error: {error}");
                }



                return response.IsSuccessStatusCode;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return false;
            }
        }
    }
}