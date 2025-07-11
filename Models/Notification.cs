using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanManagementSystem.Models
{
    [Table("notifications")]
    public class Notification
    {
        [Key]
        [Column("notification_id")]
        public int NotificationId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("message")]
        public required string Message { get; set; }

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("UserId")]
        public User? User { get; set; }
        [Column("Link")]
        public string? Link { get; set; } // In Notification.cs model

    }
}
