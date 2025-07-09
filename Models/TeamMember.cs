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
        [Column("can_manage_leads")]
        public bool CanManageLeads { get; set; } = false;

        [Column("can_upload_docs")]
        public bool CanUploadDocs { get; set; } = false;

        [Column("can_verify_docs")]
        public bool CanVerifyDocs { get; set; } = false;

        public Team? Team { get; set; }
        public User? User { get; set; }
    }
}
