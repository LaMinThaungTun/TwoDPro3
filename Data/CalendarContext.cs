using Microsoft.EntityFrameworkCore;
using TwoDPro3.Models;

namespace TwoDPro3.Data
{
    public class CalendarContext : DbContext
    {
        public CalendarContext(DbContextOptions<CalendarContext> options)
            : base(options) { }

        public DbSet<Calendar> Table1 { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<MembershipPlan> MembershipPlans { get; set; }
        public DbSet<UserMembership> UserMemberships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔑 Primary Key
            modelBuilder.Entity<UserMembership>()
                .HasKey(um => um.UserId);

            // ✅ User ↔ UserMembership (ONE TO ONE)
            modelBuilder.Entity<UserMembership>()
                .HasOne(um => um.User)
                .WithOne(u => u.UserMembership)
                .HasForeignKey<UserMembership>(um => um.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ MembershipPlan ↔ UserMembership (ONE TO MANY)
            modelBuilder.Entity<UserMembership>()
                .HasOne(um => um.MembershipPlan)
                .WithMany(mp => mp.UserMemberships)
                .HasForeignKey(um => um.MembershipPlanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
