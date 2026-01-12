using Microsoft.EntityFrameworkCore;
using TwoDPro3.Data;

namespace TwoDPro3.Services
{
    public class MembershipService
    {
        private readonly CalendarContext _context;

        public MembershipService(CalendarContext context)
        {
            _context = context;
        }

        public async Task<UserMembership?> GetActiveMembershipAsync(int userId)
        {
            // Use UTC date only (Neon-safe, consistent)
            var todayUtc = DateTime.UtcNow.Date;

            return await _context.UserMemberships
                .Include(m => m.MembershipPlan)
                .Where(m =>
                    m.UserId == userId &&
                    m.IsActive &&
                    m.StartDate <= todayUtc &&
                    m.EndDate >= todayUtc
                )
                .OrderByDescending(m => m.EndDate)
                .FirstOrDefaultAsync();
        }
    }
}
