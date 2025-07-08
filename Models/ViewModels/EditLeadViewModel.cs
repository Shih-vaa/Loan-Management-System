using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Models.ViewModels
{
    public class EditLeadViewModel
    {
        public int LeadId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }

        [Display(Name = "Assign To")]
        [Required(ErrorMessage = "Please select a staff to assign")]
        public int? AssignedTo { get; set; }
    }
}
