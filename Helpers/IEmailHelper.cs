// File: Helpers/IEmailHelper.cs
namespace LoanManagementSystem.Helpers
{
    public interface IEmailHelper
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendLeadAssignmentEmailAsync(string toEmail, string toName, int leadId);
    }
}
