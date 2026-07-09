using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoDPro3.Models
{
    [Table("otp_codes")]
    public class OtpCode
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }


        [Column("telegram_chat_id")]
        public long TelegramChatId { get; set; }


        [Column("phone_number")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = "";


        [Column("code")]
        [Required]
        [MaxLength(10)]
        public string Code { get; set; } = "";


        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }


        [Column("is_used")]
        public bool IsUsed { get; set; } = false;


        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}