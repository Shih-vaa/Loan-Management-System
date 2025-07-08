using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanManagementSystem.Models
{
    public class Commission
{
    [Key]
    [Column("commission_id")]
    public int CommissionId { get; set; }

    [Column("lead_id")]
    public int LeadId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("calculated_at")]
    public DateTime CalculatedAt { get; set; } = DateTime.Now;

    // Navigation
    public Lead? Lead { get; set; }
    public User? User { get; set; }
}

}
