using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace LoanManagementSystem.Services
{
    public class EmailHelper
    {
        private readonly IConfiguration _config;

        public EmailHelper(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var smtpClient = new SmtpClient(_config["Email:SmtpServer"])
            {
                Port = int.Parse(_config["Email:Port"]),
                Credentials = new NetworkCredential(_config["Email:Username"], _config["Email:Password"]),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["Email:From"]),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }

        public static string LeadAssignmentTemplate(string userName, int leadId)
        {
            return $@"
                <h2>New Lead Assigned</h2>
                <p>Dear {userName},</p>
                <p>You have been assigned a new lead with ID <strong>#{leadId}</strong>.</p>
                <p>Please log in to the Loan Management System to proceed.</p>
            ";
        }
    }
}
