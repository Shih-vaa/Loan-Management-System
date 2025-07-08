using Microsoft.EntityFrameworkCore;
using LoanManagementSystem.Models;

namespace LoanManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // === Tables ===
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Lead> Leads { get; set; }

        // === Fluent API Configuration ===
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 👉 COMPOSITE PRIMARY KEY for TeamMember
            modelBuilder.Entity<TeamMember>()
                .HasKey(tm => new { tm.TeamId, tm.UserId });

            // (Optional) Relationships — uncomment if you’ve defined navigation properties
            modelBuilder.Entity<TeamMember>()
                .HasOne(tm => tm.Team)
                .WithMany(t => t.Members) // if Team has ICollection<TeamMember> Members
                .HasForeignKey(tm => tm.TeamId);

            modelBuilder.Entity<TeamMember>()
                .HasOne(tm => tm.User)
                .WithMany() // no reverse nav, or you can define User.TeamMemberships
                .HasForeignKey(tm => tm.UserId);

            // Existing configurations for Lead
            modelBuilder.Entity<Lead>()
                .HasOne(l => l.LeadGenerator)
                .WithMany()
                .HasForeignKey(l => l.LeadGeneratorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Lead>()
                .HasOne(l => l.AssignedUser)
                .WithMany()
                .HasForeignKey(l => l.AssignedTo)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Lead>()
                .HasOne(l => l.Customer)
                .WithMany()
                .HasForeignKey(l => l.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        
}





        public DbSet<LeadDocument> LeadDocuments { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }


        // === Future modules ===
        // public DbSet<Team> Teams { get; set; }
        // public DbSet<TeamMember> TeamMembers { get; set; }
        // public DbSet<LeadDocument> LeadDocuments { get; set; }
        // public DbSet<Commission> Commissions { get; set; }
        // public DbSet<Notification> Notifications { get; set; }
        // public DbSet<AuditLog> AuditLogs { get; set; }
    }
}
