using LoanManagementSystem.Models;

namespace LoanManagementSystem.Models.ViewModels
{
    public class DashboardMetricsViewModel
    {
        public int TotalLeads { get; set; }
        public int ApprovedLeads { get; set; }
        public int PendingLeads { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalUsers { get; set; }
        public int TotalTeams { get; set; }
        public decimal TotalCommissionPaid { get; set; }
        public decimal TotalCommissionPending { get; set; }
        public decimal TotalCommission { get; set; }
        public int PendingDocuments { get; set; }
        public int TotalDocumentsUploaded { get; set; }
        public int UnassignedLeadsCount { get; set; }

        public List<Notification> RecentNotifications { get; set; } = new();
        public Dictionary<string, int> LeadStatusCounts { get; set; } = new();

        // ✅ Team-wise metrics
        public List<TeamPerformanceMetrics> TeamPerformances { get; set; } = new();
    }

    public class TeamPerformanceMetrics
    {
        public string TeamName { get; set; } = "";
        public int TotalLeadsGenerated { get; set; }
        public int LeadsAssigned { get; set; }
        public int DocumentsUploaded { get; set; }
        public int DocumentsVerified { get; set; }
        public int LeadsApproved { get; set; }
        public decimal TotalCommission { get; set; }
    }
}
