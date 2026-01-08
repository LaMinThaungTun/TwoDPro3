using Microsoft.EntityFrameworkCore;
using TwoDPro3.Models;

namespace TwoDPro3.Data
{
    public class CalendarContext : DbContext
    {
        public CalendarContext(DbContextOptions<CalendarContext> options) : base(options) { }

        public DbSet<Calendar> Table1 { get; set; }  // maps Calendar to table1
        public DbSet<User> Users { get; set; }
        public DbSet<MembershipPlan> MembershipPlans { get; set; }
        public DbSet<UserMembership> UserMemberships { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UserMembership → User (many-to-one)
            modelBuilder.Entity<UserMembership>()
                .HasOne(um => um.User)
                .WithMany()
                .HasForeignKey(um => um.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserMembership → MembershipPlan (many-to-one)
            modelBuilder.Entity<UserMembership>()
                .HasOne(um => um.MembershipPlan)
                .WithMany()
                .HasForeignKey(um => um.MembershipPlanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
