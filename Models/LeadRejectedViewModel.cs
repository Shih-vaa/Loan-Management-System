namespace LoanManagementSystem.ViewModels
{
    public class LeadRejectedViewModel
    {
        public string UserName { get; set; } = "";
        public int LeadId { get; set; }
        public string? Reason { get; set; }
    }
}
