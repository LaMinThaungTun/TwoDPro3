using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;
using TwoDPro3.Services;
using BCrypt.Net;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OtpController : ControllerBase
    {
        private readonly TelegramOtpService _telegram;
        private readonly CalendarContext _db;


        public OtpController(
            TelegramOtpService telegram,
            CalendarContext db)
        {
            _telegram = telegram;
            _db = db;
        }



        // =====================================================
        // VERIFY OTP AND COMPLETE REGISTRATION
        // =====================================================

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp(
            [FromBody] VerifyOtpRequest request)
        {
            try
            {
                // -----------------------------------------
                // 1. Find Telegram link
                // -----------------------------------------

                var link =
                    await _db.UserTelegramLinks
                    .FirstOrDefaultAsync(x =>
                        x.PhoneNumber ==
                        request.PhoneNumber);


                if (link == null)
                {
                    return BadRequest(
                        "Telegram account not linked");
                }



                // -----------------------------------------
                // 2. Verify OTP
                // -----------------------------------------

                var ok =
                    await _telegram.VerifyOtpAsync(
                        link.TelegramChatId,
                        request.Code);


                if (!ok)
                {
                    return BadRequest(
                        "Invalid or expired OTP");
                }



                // -----------------------------------------
                // 3. Find pending registration
                // -----------------------------------------

                var pending =
                    await _db.PendingRegistrations
                    .FirstOrDefaultAsync(x =>
                        x.PhoneNumber ==
                        request.PhoneNumber &&
                        !x.IsCompleted);



                if (pending == null)
                {
                    return BadRequest(
                        "Pending registration not found");
                }



                // -----------------------------------------
                // 4. Check user again
                // -----------------------------------------

                var exists =
                    await _db.Users.AnyAsync(x =>
                        x.PhoneNumber ==
                        pending.PhoneNumber);


                if (exists)
                {
                    return Conflict(
                        "User already exists");
                }



                using var transaction =
                    await _db.Database
                    .BeginTransactionAsync();


                try
                {
                    // -------------------------------------
                    // 5. Create User
                    // -------------------------------------

                    var user =
                        new User
                        {
                            UserName =
                                pending.UserName,

                            Email =
                                pending.Email,

                            PhoneNumber =
                                pending.PhoneNumber,

                            PasswordHash =
                                pending.PasswordHash,

                            IsActive = true,

                            IsVerified = true,

                            CreatedAt =
                                DateTime.UtcNow
                        };


                    _db.Users.Add(user);

                    await _db.SaveChangesAsync();



                    // -------------------------------------
                    // 6. Add FREE TRIAL membership
                    // -------------------------------------

                    var freePlan =
                        await _db.MembershipPlans
                        .FirstOrDefaultAsync(x =>
                            x.Name ==
                            "FREE_TRIAL"
                            &&
                            x.IsActive);


                    if (freePlan == null)
                    {
                        throw new Exception(
                            "FREE_TRIAL plan not configured");
                    }



                    var membership =
                        new UserMembership
                        {
                            UserId =
                                user.Id,

                            MembershipPlanId =
                                freePlan.Id,

                            StartDate =
                                DateTime.UtcNow.Date,

                            EndDate =
                                DateTime.UtcNow.Date
                                .AddDays(
                                    freePlan.DurationDays),

                            IsActive = true,

                            CreatedAt =
                                DateTime.UtcNow
                        };


                    _db.UserMemberships
                        .Add(membership);


                    // -------------------------------------
                    // 7. Mark registration completed
                    // -------------------------------------

                    pending.IsCompleted = true;



                    await _db.SaveChangesAsync();


                    await transaction.CommitAsync();



                    return Ok(new
                    {
                        Success = true,

                        Message =
                            "Registration completed",

                        UserId =
                            user.Id
                    });

                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return StatusCode(
                    500,
                    "Registration failed");
            }
        }
    }



    public record VerifyOtpRequest(
        string PhoneNumber,
        string Code);
}