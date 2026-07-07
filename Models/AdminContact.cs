using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoDPro3.Models
{
    [Table("admin_contact")]
    public class AdminContact
    {
        public int Id { get; set; }

        public string AdminName { get; set; } = "";

        public string TelegramUrl { get; set; } = "";

        public string? Phone { get; set; }

        public bool IsActive { get; set; }
    }
}
