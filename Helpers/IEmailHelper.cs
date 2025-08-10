// File: Helpers/IEmailHelper.cs
using LoanManagementSystem.Models;

namespace LoanManagementSystem.Helpers
{
    public interface IEmailHelper
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendLeadAssignmentEmailAsync(string toEmail, string toName, int leadId);
        Task SendLeadReassignmentEmailAsync(string toEmail, string userName, int leadId);
        Task SendLeadRejectedEmailAsync(string toEmail, string userName, int leadId, string? reason);
        Task SendLeadRemovedEmailAsync(string removedUserEmail, string removedUserName, int leadId);
        Task SendLeadDeletedEmailAsync(string toEmail, string userName, int leadId, string message);

    }
}
