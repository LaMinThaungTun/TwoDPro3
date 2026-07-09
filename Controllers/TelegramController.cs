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
        private readonly CalendarContext _db;
        private readonly TelegramOtpService _otpService;


        public TelegramController(
            CalendarContext db,
            TelegramOtpService otpService)
        {
            _db = db;
            _otpService = otpService;
        }



        // =====================================================
        // TELEGRAM WEBHOOK
        // Receives /start PHONE_NUMBER
        // =====================================================

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook(
            [FromBody] TelegramUpdate update)
        {
            try
            {
                var message = update?.Message;


                if (message == null ||
                    string.IsNullOrWhiteSpace(message.Text))
                {
                    return Ok();
                }


                if (!message.Text.StartsWith("/start"))
                {
                    return Ok();
                }


                var parts =
                    message.Text.Split(' ');


                if (parts.Length < 2)
                {
                    return Ok();
                }



                // MAUI sends phone number
                string phoneNumber =
                    parts[1].Trim();


                long chatId =
                    message.Chat.Id;



                Console.WriteLine(
                    $"PHONE = {phoneNumber}");

                Console.WriteLine(
                    $"CHAT ID = {chatId}");



                // =====================================================
                // 1. CHECK TELEGRAM CHAT ALREADY REGISTERED
                // =====================================================

                var alreadyLinked =
                    await _db.UserTelegramLinks
                    .AnyAsync(x =>
                        x.TelegramChatId == chatId);



                if (alreadyLinked)
                {
                    await _otpService
                        .SendCustomMessageAsync(
                            chatId,
                            "စာရင်းသွင်းပြီးသားဖြစ်ပါသည်။");

                    return Ok();
                }




                // =====================================================
                // 2. FIND PENDING REGISTRATION
                // =====================================================

                var pending =
                    await _db.PendingRegistrations
                    .FirstOrDefaultAsync(x =>
                        x.PhoneNumber == phoneNumber &&
                        !x.IsCompleted &&
                        x.ExpiresAt > DateTime.UtcNow);



                if (pending == null)
                {
                    await _otpService
                        .SendCustomMessageAsync(
                            chatId,
                            "စာရင်းသွင်းရန် အချက်အလက်မတွေ့ပါ သို့မဟုတ် သက်တမ်းကုန်သွားပါပြီ။");

                    return Ok();
                }




                // =====================================================
                // 3. CHECK PHONE ALREADY LINKED
                // =====================================================

                var phoneLinked =
                    await _db.UserTelegramLinks
                    .AnyAsync(x =>
                        x.PhoneNumber == phoneNumber);



                if (phoneLinked)
                {
                    await _otpService
                        .SendCustomMessageAsync(
                            chatId,
                            "ဤဖုန်းနံပါတ်ကို Telegram ဖြင့် စာရင်းသွင်းပြီးသားဖြစ်ပါသည်။");

                    return Ok();
                }




                // =====================================================
                // 4. SAVE TELEGRAM LINK
                // =====================================================

                var link =
                    new UserTelegramLink
                    {
                        PhoneNumber = phoneNumber,

                        TelegramChatId = chatId,

                        CreatedAt = DateTime.UtcNow
                    };


                _db.UserTelegramLinks.Add(link);


                await _db.SaveChangesAsync();




                // =====================================================
                // 5. SEND OTP
                // =====================================================

                var otpSent =
                    await _otpService
                    .SendOtpAsync(chatId);



                if (!otpSent)
                {
                    await _otpService
                        .SendCustomMessageAsync(
                            chatId,
                            "OTP ပို့၍မရပါ။");

                    return Ok();
                }



                await _otpService
                    .SendCustomMessageAsync(
                        chatId,
                        "OTP ကို Telegram မှာ ပို့ပေးထားပါတယ်။");



                return Ok();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return Ok();
            }
        }
    }





    // =====================================================
    // TELEGRAM JSON MODELS
    // =====================================================

    public class TelegramUpdate
    {
        public TelegramMessage? Message { get; set; }
    }


    public class TelegramMessage
    {
        public TelegramChat Chat { get; set; }
            = new();


        public string? Text { get; set; }
    }


    public class TelegramChat
    {
        public long Id { get; set; }
    }
}