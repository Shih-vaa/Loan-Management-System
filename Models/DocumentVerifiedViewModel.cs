namespace LoanManagementSystem.ViewModels
{
    public class DocumentVerifiedViewModel
    {
        public string UserName { get; set; } = "";
        public int LeadId { get; set; }
        public string Status { get; set; } = ""; // Approved or Rejected
        public string? Remarks { get; set; }
    }
}
