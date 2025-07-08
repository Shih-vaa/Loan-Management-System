using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Models.ViewModels
{
    public class EditLeadViewModel
    {
        public int LeadId { get; set; }

        [Required]
        public string Status { get; set; }

        [Display(Name = "Assign To")]
        public int? AssignedTo { get; set; }
    }
}
