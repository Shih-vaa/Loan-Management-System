using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanManagementSystem.Models
{
    public class Lead
    {
        [Key]
        [Column("lead_id")]
        public int LeadId { get; set; }

        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Column("loan_amount")]
        public decimal LoanAmount { get; set; }

        [Column("loan_type")]
        public string LoanType { get; set; }

        [Column("status")]
        public string Status { get; set; } = "new";

        [Column("lead_generator_id")]
        public int LeadGeneratorId { get; set; }

        [Column("assigned_to")]
        public int? AssignedTo { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("assigned_at")]
        public DateTime? AssignedAt { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        // Navigation
        [ForeignKey("LeadGeneratorId")]
        public User? LeadGenerator { get; set; }

        [ForeignKey("AssignedTo")]
        public User? AssignedUser { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [Column("deadline")]
        public DateTime? Deadline { get; set; }

        public virtual ICollection<LeadDocument>? Documents { get; set; }



        public bool IsDeleted { get; set; } = false;
        public DateTime? DateDeleted { get; set; }

    }
}
