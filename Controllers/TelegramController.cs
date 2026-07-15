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


                long chatId = message.Chat.Id;

                string text = message.Text.Trim();



                // =================================================
                // 1. START COMMAND
                // =================================================

                if (text == "/start")
                {
                    await _otpService.SendCustomMessageAsync(
                        chatId,
                        "User name နဲ့ Ph Number ရိုက်ပေးပါ။\n" +
                        "စာရင်းသွင်းထားသည်နှင့် တူရပါမည်။\n" +
                        "တစ်ကြောင်းစီ ရိုက်ပေးပါ။");


                    return Ok();
                }




                // =================================================
                // 2. RECEIVE USERNAME + PHONE
                // =================================================

                var lines =
                    text.Split(
                        '\n',
                        StringSplitOptions.RemoveEmptyEntries);



                if (lines.Length < 2)
                {
                    await _otpService.SendCustomMessageAsync(
                        chatId,
                        "User name နဲ့ Ph Number ကို တစ်ကြောင်းစီ ရိုက်ပေးပါ။");

                    return Ok();
                }



                string userName =
                    lines[0].Trim();


                string phoneNumber =
                    lines[1].Trim();



                Console.WriteLine(
                    "========== TELEGRAM REGISTER ==========");

                Console.WriteLine(
                    $"UserName : {userName}");

                Console.WriteLine(
                    $"Phone    : {phoneNumber}");

                Console.WriteLine(
                    $"ChatId   : {chatId}");




                // =================================================
                // 3. CHECK TELEGRAM ALREADY LINKED
                // =================================================

                bool telegramExists =
                        await _db.UserTelegramLinks
                        .AnyAsync(x =>
                            x.TelegramChatId == chatId &&
                            x.IsUsed);

                if (telegramExists)
                {
                    await _otpService.SendCustomMessageAsync(
                        chatId,
                        "စာရင်းသွင်းပြီးသား ရှိပါသည်။");

                    return Ok();
                }





                // =================================================
                // 4. CHECK USERNAME DUPLICATE
                // =================================================

                bool userNameExists =
                    await _db.Users
                    .AnyAsync(x =>
                        x.UserName == userName);



                if (userNameExists)
                {
                    await _otpService.SendCustomMessageAsync(
                        chatId,
                        "တူညီသည့် User Name ရှိနေပါသည်။\n" +
                        "နောက်တစ်မျိုး ပြောင်းပေးပါ။");

                    return Ok();
                }





                // =================================================
                // 5. CREATE OR UPDATE TELEGRAM LINK
                // =================================================

                var link =
                    await _db.UserTelegramLinks
                    .FirstOrDefaultAsync(x =>
                        x.TelegramChatId == chatId);

                if (link == null)
                {
                    link = new UserTelegramLink
                    {
                        PhoneNumber = phoneNumber,
                        TelegramChatId = chatId,
                        IsUsed = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    _db.UserTelegramLinks.Add(link);
                }
                else
                {
                    // OTP မပြီးသေးတဲ့ Telegram Link ကို Update ပြန်လုပ်
                    link.PhoneNumber = phoneNumber;
                    link.IsUsed = false;
                    link.CreatedAt = DateTime.UtcNow;
                }

                await _db.SaveChangesAsync();





                // =================================================
                // 6. GENERATE AND SEND OTP
                // =================================================

                bool sent =
                    await _otpService.SendOtpAsync(
                        chatId,
                        phoneNumber);



                if (!sent)
                {
                    await _otpService.SendCustomMessageAsync(
                        chatId,
                        "OTP ပို့၍မရပါ။");

                    return Ok();
                }







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