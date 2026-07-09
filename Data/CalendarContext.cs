using Microsoft.EntityFrameworkCore;
using TwoDPro3.Models;

namespace TwoDPro3.Data
{
    public class CalendarContext : DbContext
    {
        public CalendarContext(
            DbContextOptions<CalendarContext> options)
            : base(options)
        {
        }

        public DbSet<Calendar> Table1 { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<MembershipPlan> MembershipPlans { get; set; }

        public DbSet<UserMembership> UserMemberships { get; set; }

        public DbSet<PendingRegistration> PendingRegistrations { get; set; }

        public DbSet<UserTelegramLink> UserTelegramLinks { get; set; }

        public DbSet<PendingTelegramLink> PendingTelegramLinks { get; set; }

        public DbSet<OtpCode> OtpCodes { get; set; }

        public DbSet<AdminContact> AdminContact { get; set; }


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}