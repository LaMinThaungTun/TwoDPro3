using System.ComponentModel.DataAnnotations;

namespace TwoDPro3.Models
{
    public class UserTelegramLink
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = "";

        public long TelegramChatId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}