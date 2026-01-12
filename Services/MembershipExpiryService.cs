using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TwoDPro3.Data;

namespace TwoDPro3.Services
{
    public class MembershipExpiryService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public MembershipExpiryService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Initial delay to align with your desired UTC time (e.g., 5:30 AM UTC)
            await DelayUntilNextCheckAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine($"[MembershipExpiryService] Running at {DateTime.UtcNow:u}");

                    await ExpireMembershipsAsync();

                    Console.WriteLine($"[MembershipExpiryService] Completed check at {DateTime.UtcNow:u}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MembershipExpiryService] Error: {ex.Message}");
                }

                try
                {
                    // Wait 24 hours before next check
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Ignore if cancellation requested during delay
                }
            }
        }

        private async Task ExpireMembershipsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CalendarContext>();

            var utcToday = DateTime.UtcNow.Date;

            var expiredMemberships = await db.UserMemberships
                .Where(m => m.IsActive && m.EndDate < utcToday)
                .ToListAsync();

            if (expiredMemberships.Count == 0)
            {
                Console.WriteLine("[MembershipExpiryService] No expired memberships found.");
                return;
            }

            foreach (var membership in expiredMemberships)
            {
                membership.IsActive = false;
                Console.WriteLine($"[MembershipExpiryService] Deactivated membership ID {membership.Id} for user ID {membership.UserId}");
            }

            await db.SaveChangesAsync();
        }

        private async Task DelayUntilNextCheckAsync(CancellationToken stoppingToken)
        {
            // Your desired daily check time in UTC (e.g., 05:30 AM UTC for Myanmar noon)
            var targetTimeUtc = new TimeSpan(13, 00, 0);

            var nowUtc = DateTime.UtcNow;
            var nextRunTimeUtc = nowUtc.Date + targetTimeUtc;

            if (nowUtc > nextRunTimeUtc)
                nextRunTimeUtc = nextRunTimeUtc.AddDays(1);

            var delay = nextRunTimeUtc - nowUtc;

            Console.WriteLine($"[MembershipExpiryService] Waiting {delay} until first run at {nextRunTimeUtc:u}");

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Ignore if cancellation requested during delay
            }
        }
    }
}
