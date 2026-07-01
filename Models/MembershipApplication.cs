using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoDPro3.Models
{
    [Table("membership_applications")]
    public class MembershipApplication
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("agent_id")]
        public int AgentId { get; set; }

        [Required]
        [Column("status")]
        public string Status { get; set; } = "Pending";

        [Column("applied_at")]
        public DateTime AppliedAt { get; set; }

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [ForeignKey(nameof(AgentId))]
        public Agent? Agent { get; set; }
    }
}