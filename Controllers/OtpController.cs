using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Services;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OtpController : ControllerBase
    {
        private readonly TelegramOtpService _telegram;
        private readonly AppDbContext _db;

        public OtpController(
            TelegramOtpService telegram,
            AppDbContext db)
        {
            _telegram = telegram;
            _db = db;
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp(
            [FromBody] VerifyOtpRequest request)
        {
            var link = await _db.UserTelegramLinks
                .FirstOrDefaultAsync(x =>
                    x.PhoneNumber == request.PhoneNumber);

            if (link == null)
            {
                return BadRequest(
                    "Telegram account not linked");
            }

            var ok = await _telegram.VerifyOtpAsync(
                link.TelegramChatId,
                request.Code);

            if (!ok)
            {
                return BadRequest(
                    "Invalid or expired OTP");
            }

            return Ok("OTP verified");
        }
    }

    public record VerifyOtpRequest(
        string PhoneNumber,
        string Code);
}