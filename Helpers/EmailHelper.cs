using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using LoanManagementSystem.Models;
using LoanManagementSystem.Services;
using LoanManagementSystem.ViewModels;
using Microsoft.Extensions.Configuration;

namespace LoanManagementSystem.Helpers
{
    public class EmailHelper : IEmailHelper
    {
        private readonly IConfiguration _config;
        private readonly IRazorViewToStringRenderer _razorRenderer;

        public EmailHelper(IConfiguration config, IRazorViewToStringRenderer razorRenderer)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _razorRenderer = razorRenderer ?? throw new ArgumentNullException(nameof(razorRenderer));
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrEmpty(toEmail))
                throw new ArgumentException("Recipient email cannot be null or empty", nameof(toEmail));

            var smtpServer = _config["Email:SmtpServer"] ?? throw new ArgumentNullException("Email:SmtpServer configuration is missing");
            var port = int.Parse(_config["Email:Port"] ?? throw new ArgumentNullException("Email:Port configuration is missing"));
            var username = _config["Email:Username"] ?? throw new ArgumentNullException("Email:Username configuration is missing");
            var password = _config["Email:Password"] ?? throw new ArgumentNullException("Email:Password configuration is missing");
            var fromEmail = _config["Email:From"] ?? throw new ArgumentNullException("Email:From configuration is missing");

            using var smtpClient = new SmtpClient(smtpServer)
            {
                Port = port,
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendLeadAssignmentEmailAsync(string toEmail, string userName, int leadId)
        {
            var model = new
            {
                UserName = userName,
                LeadId = leadId
            };

            string htmlBody = await _razorRenderer.RenderViewToStringAsync("Emails/LeadAssignment", model);
            string subject = "New Lead Assigned";

            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task SendLeadApprovedEmailAsync(string toEmail, string userName, int leadId)
        {
            var body = await _razorRenderer.RenderViewToStringAsync("/Views/Emails/LeadApproved.cshtml",
                new LeadNotificationViewModel
                {
                    UserName = userName,
                    LeadId = leadId
                });

            await SendEmailAsync(toEmail, $"Lead Approved: LMS-{leadId:D4}", body);
        }

        public async Task SendLeadRejectedEmailAsync(string toEmail, string userName, int leadId, string? reason)
        {
            var body = await _razorRenderer.RenderViewToStringAsync("/Views/Emails/LeadRejected.cshtml",
                new LeadRejectedViewModel
                {
                    UserName = userName,
                    LeadId = leadId,
                    Reason = reason
                });

            await SendEmailAsync(toEmail, $"Lead Rejected: LMS-{leadId:D4}", body);
        }

        public async Task SendDocumentVerificationEmailAsync(string toEmail, string userName, int leadId, string status, string? remarks)
        {
            var body = await _razorRenderer.RenderViewToStringAsync("/Views/Emails/DocumentVerified.cshtml",
                new DocumentVerifiedViewModel
                {
                    UserName = userName,
                    LeadId = leadId,
                    Status = status,
                    Remarks = remarks
                });

            await SendEmailAsync(toEmail, $"Document {status}: LMS-{leadId:D4}", body);
        }

        public async Task SendCommissionPaidEmailAsync(string toEmail, string userName, int leadId, decimal amount, DateTime paidDate)
        {
            var body = await _razorRenderer.RenderViewToStringAsync("/Views/Emails/CommissionPaid.cshtml",
                new CommissionPaidViewModel
                {
                    UserName = userName,
                    LeadId = leadId,
                    Amount = amount,
                    PaidDate = paidDate
                });

            await SendEmailAsync(toEmail, $"Commission Paid: LMS-{leadId:D4}", body);
        }
        public async Task SendLeadReassignmentEmailAsync(string toEmail, string userName, int leadId)
        {
            var model = new
            {
                UserName = userName,
                LeadId = leadId
            };

            string body = await _razorRenderer.RenderViewToStringAsync("Emails/LeadReassignment", model);

            await SendEmailAsync(toEmail, "Lead Reassigned", body);
        }
        public async Task SendLeadRemovedEmailAsync(string removedUserEmail, string removedUserName, int leadId)
        {
            var model = new
            {
                UserName = removedUserName,
                LeadId = leadId
            };

            string body = await _razorRenderer.RenderViewToStringAsync("Emails/LeadRemoved", model);
            string subject = $"Lead LMS-{leadId:D4} Removed";

            await SendEmailAsync(removedUserEmail, subject, body);
        }
        public async Task SendLeadDeletedEmailAsync(string toEmail, string userName, int leadId, string message)
        {
            var model = new
            {
                UserName = userName,
                LeadId = leadId,
                Message = message
            };

            string body = await _razorRenderer.RenderViewToStringAsync("Emails/LeadDeleted", model);

            await SendEmailAsync(toEmail, $"Lead Deleted: LMS-{leadId:D4}", body);
        }

    }
}