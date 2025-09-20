using Microsoft.AspNetCore.Mvc;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelloController : ControllerBase
    {
        // GET: https://twodpro3.onrender.com/api/hello/ping?message=Hello
        [HttpGet("ping")]
        public IActionResult Ping([FromQuery] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return BadRequest("Message cannot be empty.");
            }

            return Ok($"Hi Buddy! You said: {message}");
        }
    }

}
