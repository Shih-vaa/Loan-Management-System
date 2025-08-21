using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace LoanManagementSystem.Models
{
    [Table("customers")]
    public class Customer
    {
        [Key]
        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Required]
        [Column("full_name")]
        public string FullName { get; set; }

        [Column("email")]
        [EmailAddress]
        public string? Email { get; set; }

        [Column("phone")]
        [Phone]
        public string? Phone { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("date_of_birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Column("gender")]
        public string? Gender { get; set; }

        [Column("occupation")]
        public string? Occupation { get; set; }

        [Column("annual_income")]
        [DataType(DataType.Currency)]
        public decimal? AnnualIncome { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("passport_photo_path")]
        public string? PassportPhotoPath { get; set; }

        [NotMapped]
        [Display(Name = "Passport Photo")]
        public IFormFile? PassportPhotoFile { get; set; }
 
    }
}