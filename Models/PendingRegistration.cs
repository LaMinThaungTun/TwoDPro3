using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoDPro3.Models
{
    [Table("pending_registrations")]
    public class PendingRegistration
    {
        [Key]
        public int Id { get; set; }

        [Column("user_name")]
        public string UserName { get; set; } = "";

        [Column("email")]
        public string? Email { get; set; }

        [Column("phone_number")]
        public string PhoneNumber { get; set; } = "";

        [Column("password_hash")]
        public string PasswordHash { get; set; } = "";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("is_completed")]
        public bool IsCompleted { get; set; }
    }
}