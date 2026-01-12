using Microsoft.AspNetCore.Mvc;
using TwoDPro3.Data;
using TwoDPro3.Services;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly CalendarContext _context;
        private readonly MembershipService _membershipService;

        public ProfileController(
            CalendarContext context,
            MembershipService membershipService)
        {
            _context = context;
            _membershipService = membershipService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var membership = await _membershipService.GetActiveMembershipAsync(userId);

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.PhoneNumber,

                Membership = membership?.MembershipPlan.Name ?? "FREE",

                MembershipEndDate = membership == null
                    ? (DateTime?)null
                    : DateTime.SpecifyKind(membership.EndDate, DateTimeKind.Utc),

                IsMembershipActive = membership != null
            });


        }
    }
}
