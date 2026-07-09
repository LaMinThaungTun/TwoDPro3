using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoDPro3.Models
{
    [Table("user_telegram_links")]
    public class UserTelegramLink
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }


        [Column("phone_number")]
        public string PhoneNumber { get; set; } = "";


        [Column("telegram_chat_id")]
        public long TelegramChatId { get; set; }


        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}