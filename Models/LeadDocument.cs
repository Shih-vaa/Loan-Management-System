using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanManagementSystem.Models
{
    [Table("lead_documents")]  // 👈 Tells EF the exact table name
    public class LeadDocument
    {
        [Key]
        [Column("doc_id")]
        public int DocId { get; set; }

        [Column("lead_id")]
        public int LeadId { get; set; }

        [Column("file_name")]
        public string? FileName { get; set; }

        [Column("file_path")]
        public string? FilePath { get; set; }

        [Column("uploaded_by")]
        public int? UploadedBy { get; set; }

        [Column("uploaded_at")]
        public DateTime UploadedAt { get; set; }

        // Navigation properties (optional, if used)
        [ForeignKey("UploadedBy")]
        public User? UploadedByUser { get; set; }

        [ForeignKey("LeadId")]
        public Lead? Lead { get; set; }
    }
}
