using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models.Requests;
using TwoDPro3.Models.Responses;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/login")]
    public class LoginController : ControllerBase
    {
        private readonly CalendarContext _context;

        public LoginController(CalendarContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password required");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == request.Username);

            if (user == null)
                return Unauthorized("Invalid username or password");

            if (!user.IsActive)
                return Unauthorized("Account is disabled");

            bool passwordOk = BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash
            );

            if (!passwordOk)
                return Unauthorized("Invalid username or password");

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                UserId = user.Id,
                DisplayName = user.UserName
            });
        }
    }
}
