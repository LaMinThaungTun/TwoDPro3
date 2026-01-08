using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoDPro3.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("display_name")]
        public string UserName { get; set; } = null!;

        [Column("email")]
        public string? Email { get; set; }

        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("password_hash")]
        public string PasswordHash { get; set; } = null!;

        [Column("device_id")]
        public string? DeviceId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("is_verified")]
        public bool IsVerified { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }
    }
}
