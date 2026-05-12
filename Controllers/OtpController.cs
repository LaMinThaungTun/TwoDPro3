using Microsoft.AspNetCore.Mvc;
using TwoDPro3.Services;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OtpController : ControllerBase
    {
        private readonly TelegramOtpService _telegram;

        public OtpController(TelegramOtpService telegram)
        {
            _telegram = telegram;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            var ok = await _telegram.SendOtpAsync(request.TelegramChatId);

            if (!ok)
                return StatusCode(500, "OTP sending failed");

            return Ok("OTP sent");
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var ok = await _telegram.VerifyOtpAsync(
                request.TelegramChatId,
                request.Code);

            if (!ok)
                return BadRequest("Invalid or expired OTP");

            return Ok("OTP verified");
        }
    }

    public record SendOtpRequest(long TelegramChatId);

    public record VerifyOtpRequest(long TelegramChatId, string Code);
}