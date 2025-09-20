using Microsoft.AspNetCore.Mvc;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Hello : Controller
    {
        [HttpGet("ping")]
        public IActionResult Ping(string message)
        {
            if (message == "Hello")
                return Ok("Hi Buddy");
            else
                return Ok("I only reply to Hello :)");
        }
    }
}
