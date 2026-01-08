using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;
using TwoDPro3.Models.Requests;
using TwoDPro3.Models.Responses;
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

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // ---------- Validation ----------
            if (string.IsNullOrWhiteSpace(request.UserName))
                return BadRequest("User name required");

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Password required");

            if (string.IsNullOrWhiteSpace(request.Email) &&
                string.IsNullOrWhiteSpace(request.PhoneNumber))
                return BadRequest("Email or Phone required");

            string? email = request.Email?.Trim().ToLower();
            string? phone = request.PhoneNumber?.Trim();

            // ---------- Duplicate Check ----------
            bool exists = await _context.Users.AnyAsync(u =>
                (!string.IsNullOrEmpty(email) && u.Email == email) ||
                (!string.IsNullOrEmpty(phone) && u.PhoneNumber == phone)
            );

            if (exists)
                return Conflict("User already exists");

            // ---------- Free Trial Plan ----------
            var freePlan = await _context.MembershipPlans
                .FirstOrDefaultAsync(p => p.Name == "FREE_TRIAL" && p.IsActive);

            if (freePlan == null)
                return StatusCode(500, "Free trial plan not configured");

            // ---------- Transaction ----------
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new User
                {
                    UserName = request.UserName.Trim(),
                    Email = email,
                    PhoneNumber = phone,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    IsActive = true,
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var membership = new UserMembership
                {
                    UserId = user.Id,
                    MembershipPlanId = freePlan.Id,
                    StartDate = DateTime.UtcNow.Date,
                    EndDate = DateTime.UtcNow.Date.AddDays(14),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserMemberships.Add(membership);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                return Ok(new RegisterResponse
                {
                    Success = true,
                    Message = "Registered successfully",
                    UserId = user.Id
                });
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
