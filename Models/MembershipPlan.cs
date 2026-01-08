using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoDPro3.Models
{
    [Table("membership_plans")]
    public class MembershipPlan
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("price")]
        public decimal Price { get; set; }

        [Column("duration_days")]
        public int DurationDays { get; set; }

        [Column("max_search_per_day")]
        public int? MaxSearchPerDay { get; set; }

        [Column("can_view_history")]
        public bool CanViewHistory { get; set; }

        [Column("can_export")]
        public bool CanExport { get; set; }

        [Column("priority_support")]
        public bool PrioritySupport { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
