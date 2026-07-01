using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoDPro3.Models
{
    [Table("agent_rotation")]
    public class AgentRotation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("last_agent_id")]
        public int? LastAgentId { get; set; }

        [ForeignKey(nameof(LastAgentId))]
        public Agent? LastAgent { get; set; }
    }
}