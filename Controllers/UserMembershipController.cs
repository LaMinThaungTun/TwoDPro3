using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/usermembership")]
    public class UserMembershipController : ControllerBase
    {
        private readonly CalendarContext _context;

        public UserMembershipController(CalendarContext context)
        {
            _context = context;
        }

        // GET api/usermembership/status/{userId}
        [HttpGet("status/{userId}")]
        public async Task<IActionResult> GetMembershipStatus(int userId)
        {
            // Get active membership for user
            var membership = await _context.UserMemberships
                .Include(um => um.MembershipPlan)
                .Where(um => um.UserId == userId && um.IsActive)
                .OrderByDescending(um => um.EndDate)
                .FirstOrDefaultAsync();

            if (membership == null)
            {
                // No active membership found
                return Ok(new
                {
                    IsActive = false,
                    PlanName = "No active membership",
                    EndDate = (DateTime?)null
                });
            }

            return Ok(new
            {
                IsActive = membership.EndDate >= DateTime.UtcNow.Date,
                PlanName = membership.MembershipPlan.Name,
                EndDate = membership.EndDate
            });
        }
    }
}
