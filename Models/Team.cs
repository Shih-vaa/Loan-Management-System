using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanManagementSystem.Models
{
    [Table("teams")]
    public class Team
    {
        [Key]
        [Column("team_id")]
        public int TeamId { get; set; }

        [Column("team_name")]
        public string TeamName { get; set; } = "";

        [Column("created_by")]
        public int CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public User? Creator { get; set; }

        public List<TeamMember> Members { get; set; } = new();
        
    }
}
