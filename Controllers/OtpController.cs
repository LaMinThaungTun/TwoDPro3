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
        // VERIFY OTP AND CREATE USER
        // =====================================================

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp(
            [FromBody] VerifyOtpRequest request)
        {

            Console.WriteLine("========== VERIFY REQUEST ==========");
            Console.WriteLine($"UserName   = {request.UserName}");
            Console.WriteLine($"Phone      = {request.PhoneNumber}");
            Console.WriteLine($"Password   = {request.Password}");
            Console.WriteLine($"Code       = {request.Code}");
            Console.WriteLine("====================================");


            try
            {
                // -----------------------------
                // Validation
                // -----------------------------

                if (string.IsNullOrWhiteSpace(request.UserName))
                    return BadRequest("User name required");


                if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                    return BadRequest("Phone number required");


                if (string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest("Password required");


                if (string.IsNullOrWhiteSpace(request.Code))
                    return BadRequest("OTP required");



                string userName =
                    request.UserName.Trim();


                string phone =
                    request.PhoneNumber.Trim();


                string? email =
                    request.Email?
                    .Trim()
                    .ToLower();




                // =================================================
                // 1. FIND TELEGRAM LINK
                // =================================================

                var telegramLink =
                    await _db.UserTelegramLinks
                    .FirstOrDefaultAsync(x =>
                        x.PhoneNumber == phone);



                if (telegramLink == null)
                {
                    return BadRequest(
                        "Telegram account not linked");
                }




                // =================================================
                // 2. VERIFY OTP
                // =================================================

                bool verified =
                    await _telegram.VerifyOtpAsync(
                        telegramLink.TelegramChatId,
                        phone,
                        request.Code);



                if (!verified)
                {
                    return BadRequest(
                        "Invalid or expired OTP");
                }





                // =================================================
                // 3. CHECK DUPLICATE USER
                // =================================================

                bool exists =
                    await _db.Users.AnyAsync(x =>
                        x.UserName == userName ||

                        x.PhoneNumber == phone ||

                        (!string.IsNullOrEmpty(email)
                        && x.Email == email));



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
                    // =================================================
                    // 4. CREATE USER
                    // =================================================

                    var user =
                        new User
                        {
                            UserName = userName,

                            Email = email,

                            PhoneNumber = phone,

                            PasswordHash =
                                BCrypt.Net.BCrypt
                                .HashPassword(
                                    request.Password),


                            IsActive = true,

                            IsVerified = true,

                            CreatedAt =
                                DateTime.UtcNow
                        };



                    _db.Users.Add(user);


                    await _db.SaveChangesAsync();






                    // =================================================
                    // 5. CREATE FREE TRIAL MEMBERSHIP
                    // =================================================

                    var freePlan =
                        await _db.MembershipPlans
                        .FirstOrDefaultAsync(x =>
                            x.Name == "FREE_TRIAL"
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



                    await _db.SaveChangesAsync();



                    await transaction.CommitAsync();





                    // =================================================
                    // SUCCESS
                    // =================================================

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
                Console.WriteLine(ex.ToString());

                return StatusCode(
                    500,
                    "Registration failed");
            }
        }
    }



    // =====================================================
    // MAUI REQUEST MODEL
    // =====================================================

    public class VerifyOtpRequest
    {
        public string UserName { get; set; }
            = "";

        public string? Email { get; set; }


        public string PhoneNumber { get; set; }
            = "";


        public string Password { get; set; }
            = "";


        public string Code { get; set; }
            = "";
    }
}