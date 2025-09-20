using Microsoft.AspNetCore.Mvc;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Base URL: /api/hello
    public class HelloController : ControllerBase
    {
        // GET: /api/hello/ping?message=Hello
        [HttpGet("ping")]
        public IActionResult Ping([FromQuery] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return BadRequest("Message cannot be empty.");
            }

            // Simple response to test API
            return Ok($"Hi Buddy! You said: {message}");
        }
    }

}
