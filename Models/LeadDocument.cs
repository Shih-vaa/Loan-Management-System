using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanManagementSystem.Models
{
    [Table("lead_documents")]
    public class LeadDocument
    {
        [Key]
        [Column("doc_id")]
        public int DocId { get; set; }

        [Required]
        [Column("lead_id")]
        public int LeadId { get; set; }

        [Required]
        [Column("file_name")]
        public required string FileName { get; set; }

        [Required]
        [Column("file_path")]
        public required string FilePath { get; set; }

        [Column("document_type")]
        public string? DocumentType { get; set; }

        [Column("status")]
        public string Status { get; set; } = "pending";  // pending | approved | rejected

        [Column("uploaded_by")]
        public int? UploadedBy { get; set; }

        [Column("uploaded_at")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [Column("verified_by")]
        public int? VerifiedBy { get; set; }

        [Column("verified_at")]
        public DateTime? VerifiedAt { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        // 🔗 Navigation Properties
        [ForeignKey("UploadedBy")]
        public User? UploadedByUser { get; set; }

        [ForeignKey("VerifiedBy")]
        public User? Verifier { get; set; }

        [ForeignKey("LeadId")]
        public Lead? Lead { get; set; }
    }
}
