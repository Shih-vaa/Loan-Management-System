using System;

namespace LoanManagementSystem.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public int? UserId { get; set; }
        public string? Role { get; set; }

        public string Action { get; set; } = null!;
        public string Description { get; set; } = null!;

        public string? Controller { get; set; }
        public string? ActionMethod { get; set; }

        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
