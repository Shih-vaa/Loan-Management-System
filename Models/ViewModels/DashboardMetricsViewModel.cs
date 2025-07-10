using LoanManagementSystem.Models;

namespace LoanManagementSystem.Models.ViewModels
{
    public class DashboardMetricsViewModel
    {
        // Existing properties (retain if needed elsewhere)
        public int TotalLeads { get; set; }
        public int ApprovedLeads { get; set; }
        public int PendingLeads { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalUsers { get; set; }
        public int TotalTeams { get; set; }
        public decimal TotalCommissionPaid { get; set; }
        public decimal TotalCommissionPending { get; set; }

        // 🆕 Add these for widgets and analytics
        public int PendingDocuments { get; set; }
        public int TotalDocumentsUploaded { get; set; }
        public decimal TotalCommission { get; set; }
        public List<Notification> RecentNotifications { get; set; } = new();
        public Dictionary<string, int> LeadStatusCounts { get; set; } = new();
        
    }
}
