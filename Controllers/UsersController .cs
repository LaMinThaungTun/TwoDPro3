using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;
using TwoDPro3.Models;

namespace TwoDPro3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly CalendarContext _context;

        public UsersController(CalendarContext context)
        {
            _context = context;
        }

        // 🔹 GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserProfile(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    u.LastLoginAt,

                    Membership = _context.UserMemberships
                        .Where(um => um.UserId == u.Id && um.IsActive)
                        .Select(um => new
                        {
                            PlanId = um.MembershipPlan.Id,
                            PlanName = um.MembershipPlan.Name,
                            um.StartDate,
                            um.EndDate,
                            um.IsActive
                        })
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }
    }
}
