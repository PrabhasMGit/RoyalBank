using Microsoft.EntityFrameworkCore;
using RoyalBank.Models;

namespace RoyalBank.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User>        Users        { get; set; }
        public DbSet<Customer>    Customers    { get; set; }
        public DbSet<KycDocument> KycDocuments { get; set; }
        public DbSet<RiskProfile> RiskProfiles { get; set; }
        public DbSet<Account>     Accounts     { get; set; }
        public DbSet<AuditLog>    AuditLogs    { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One Customer -> One User
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User).WithOne(u => u.Customer)
                .HasForeignKey<User>(u => u.CustomerId);

            // One Customer -> Many KycDocuments
            modelBuilder.Entity<KycDocument>()
                .HasOne(k => k.Customer).WithMany(c => c.KycDocuments)
                .HasForeignKey(k => k.CustomerId);

            // One Customer -> One RiskProfile
            modelBuilder.Entity<RiskProfile>()
                .HasOne(r => r.Customer).WithOne(c => c.RiskProfile)
                .HasForeignKey<RiskProfile>(r => r.CustomerId);

            // One Customer -> Many Accounts
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Customer).WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CustomerId);

            // AuditLog
            modelBuilder.Entity<AuditLog>()
                .HasOne(l => l.Customer).WithMany(c => c.AuditLogs)
                .HasForeignKey(l => l.CustomerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Store enums as strings
            modelBuilder.Entity<Customer>()
            .Property(c => c.OnboardingStatus)
            .HasConversion<string>();

            modelBuilder.Entity<KycDocument>()
            .Property(k => k.VerificationStatus)
            .HasConversion<string>();

            modelBuilder.Entity<RiskProfile>()
            .Property(r => r.RiskLevel)
            .HasConversion<string>();

            modelBuilder.Entity<Account>()
            .Property(a => a.AccountStatus)
            .HasConversion<string>();

            modelBuilder.Entity<User>()
            .Property(u => u.Role)              
            .HasConversion<string>();
        }
    }
}
