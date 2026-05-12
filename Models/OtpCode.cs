using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoDPro3.Models
{
    public class OtpCode
    {
        [Key]
        public int Id { get; set; }

        public long TelegramChatId { get; set; }

        [Required]
        [MaxLength(10)]
        public string Code { get; set; } = "";

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}