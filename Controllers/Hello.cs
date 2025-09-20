using Microsoft.AspNetCore.Mvc;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("test/[controller]")]
    public class Hello : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping(string message)
        {
            return Ok($"Hi Buddy! You said: {message}");
        }
    }
}
