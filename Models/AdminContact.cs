using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoDPro3.Models
{
    [Table("admin_contact")]
    public class AdminContact
    {
        public int id { get; set; }

        public bool is_active { get; set; }

        public string admin_name { get; set; } = "";

        public string telegram_url { get; set; } = "";

        public string? phone { get; set; }

        
    }
}
