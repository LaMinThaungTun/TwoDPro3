using Microsoft.EntityFrameworkCore;
using TwoDPro3.Models;

namespace TwoDPro3.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ----------------------
        // Tables

        public DbSet<OtpCode> OtpCodes => Set<OtpCode>();

        public DbSet<User> Users { get; set; }

        public DbSet<UserTelegramLink> UserTelegramLinks => Set<UserTelegramLink>();

        public DbSet<PendingTelegramLink> PendingTelegramLinks { get; set; }

        public DbSet<Agent> Agents { get; set; }

        public DbSet<MembershipApplication> MembershipApplications { get; set; }

        public DbSet<AgentRotation> AgentRotations { get; set; }

        public DbSet<AdminContact> AdminContacts { get; set; }


        // ----------------------
        // Model Configuration

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----------------------
            // otp_codes table

            modelBuilder.Entity<OtpCode>(entity =>
            {
                entity.ToTable("otp_codes");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id");

                entity.Property(x => x.TelegramChatId)
                    .HasColumnName("telegram_chat_id");

                entity.Property(x => x.Code)
                    .HasColumnName("code")
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(x => x.ExpiresAt)
                    .HasColumnName("expires_at");

                entity.Property(x => x.IsUsed)
                    .HasColumnName("is_used");

                entity.Property(x => x.CreatedAt)
                    .HasColumnName("created_at");
            });

            // ----------------------
            // user_telegram_links table

            modelBuilder.Entity<UserTelegramLink>(entity =>
            {
                entity.ToTable("user_telegram_links");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id");

                entity.Property(x => x.PhoneNumber)
                    .HasColumnName("phone_number")
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.TelegramChatId)
                    .HasColumnName("telegram_chat_id");

                entity.Property(x => x.CreatedAt)
                    .HasColumnName("created_at");

                // UNIQUE phone number
                entity.HasIndex(x => x.PhoneNumber)
                    .IsUnique();

                // UNIQUE telegram id
                entity.HasIndex(x => x.TelegramChatId)
                    .IsUnique();
            });
        }
    }
}