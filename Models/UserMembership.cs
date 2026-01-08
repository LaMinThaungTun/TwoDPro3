using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoDPro3.Models
{
    [Table("user_memberships")]
    public class UserMembership
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("membership_plan_id")]
        public int MembershipPlanId { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // 🔗 Optional navigation properties (recommended)
        public User? User { get; set; }
        public MembershipPlan? MembershipPlan { get; set; }
    }
}
