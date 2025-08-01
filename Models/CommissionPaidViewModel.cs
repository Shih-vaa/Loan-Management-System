namespace LoanManagementSystem.ViewModels
{
    public class CommissionPaidViewModel
    {
        public string UserName { get; set; } = "";
        public int LeadId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidDate { get; set; }
    }
}
