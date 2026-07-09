using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;
using TwoDPro3.Models.Requests;
using BCrypt.Net;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/register")]
    public class RegisterController : ControllerBase
    {
        private readonly CalendarContext _context;

        public RegisterController(CalendarContext context)
        {
            _context = context;
        }


        // =====================================================
        // START REGISTRATION
        // Creates PendingRegistration only
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request)
        {
            try
            {
                // -----------------------------
                // Validation
                // -----------------------------

                if (string.IsNullOrWhiteSpace(request.UserName))
                    return BadRequest("User name required");


                if (string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest("Password required");


                if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                    return BadRequest(
                        "Phone number required");


                var userName =
                    request.UserName.Trim();


                var email =
                    request.Email?
                    .Trim()
                    .ToLower();


                var phone =
                    request.PhoneNumber.Trim();



                // -----------------------------
                // Check existing account
                // -----------------------------

                bool exists =
                    await _context.Users.AnyAsync(u =>
                        u.UserName == userName ||

                        (!string.IsNullOrEmpty(email)
                         && u.Email == email) ||

                        (!string.IsNullOrEmpty(phone)
                         && u.PhoneNumber == phone)
                    );


                if (exists)
                {
                    return Conflict(
                        "User already exists");
                }



                // -----------------------------
                // Remove old pending registration
                // same phone
                // -----------------------------

                var oldPending =
                    await _context.PendingRegistrations
                    .Where(x =>
                        x.PhoneNumber == phone &&
                        !x.IsCompleted)
                    .ToListAsync();


                if (oldPending.Any())
                {
                    _context.PendingRegistrations
                        .RemoveRange(oldPending);

                    await _context.SaveChangesAsync();
                }



                // -----------------------------
                // Create pending registration
                // -----------------------------

                var pending =
                    new PendingRegistration
                    {
                        UserName = userName,

                        Email = email,

                        PhoneNumber = phone,

                        PasswordHash =
                            BCrypt.Net.BCrypt
                            .HashPassword(
                                request.Password),

                        CreatedAt =
                            DateTime.UtcNow,

                        ExpiresAt =
                            DateTime.UtcNow
                            .AddMinutes(10),

                        IsCompleted = false
                    };


                _context.PendingRegistrations
                    .Add(pending);


                await _context.SaveChangesAsync();



                // -----------------------------
                // Return success
                // MAUI opens Telegram next
                // -----------------------------

                return Ok(new
                {
                    Success = true,

                    Message =
                    "Please verify Telegram OTP",

                    PhoneNumber = phone
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine("==============================");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("==============================");

                return StatusCode(
                    500,
                    ex.ToString());
            }
        }
    }
}