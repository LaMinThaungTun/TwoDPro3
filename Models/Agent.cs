using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoDPro3.Models
{
    [Table("agents")]
    public class Agent
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = "";

        [Required]
        [Column("telegram_username")]
        public string TelegramUsername { get; set; } = "";

        [Required]
        [Column("telegram_url")]
        public string TelegramUrl { get; set; } = "";

        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("priority")]
        public int Priority { get; set; }

        [Column("current_members")]
        public int CurrentMembers { get; set; }

        [Column("max_members")]
        public int MaxMembers { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}