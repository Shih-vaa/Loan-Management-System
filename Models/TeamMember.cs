using System.ComponentModel.DataAnnotations.Schema;

namespace LoanManagementSystem.Models
{
    [Table("team_members")] // 👈 Tells EF to use the snake_case table
    public class TeamMember
    {
        [Column("team_id")]
        public int TeamId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        public Team? Team { get; set; }
        public User? User { get; set; }
    }
}
