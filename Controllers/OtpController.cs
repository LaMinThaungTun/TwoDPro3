using Microsoft.AspNetCore.Mvc;
using TwoDPro3.Services;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OtpController : ControllerBase
    {
        private readonly TwilioVerifyService _twilio;

        public OtpController(TwilioVerifyService twilio)
        {
            _twilio = twilio;
        }

        // 1️⃣ SEND OTP
        [HttpPost("send")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Phone))
                return BadRequest("Phone number required");

            var ok = await _twilio.SendOtpAsync(request.Phone);

            if (!ok)
                return StatusCode(500, "OTP sending failed");

            return Ok("OTP sent");
        }

        // 2️⃣ VERIFY OTP
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var ok = await _twilio.VerifyOtpAsync(request.Phone, request.Code);

            if (!ok)
                return BadRequest("Invalid OTP");

            return Ok("OTP verified");
        }
    }

    public record SendOtpRequest(string Phone);
    public record VerifyOtpRequest(string Phone, string Code);
}
