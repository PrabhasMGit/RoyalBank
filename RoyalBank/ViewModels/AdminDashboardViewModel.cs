using RoyalBank.Models;
namespace RoyalBank.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int PendingAccounts { get; set; }
        public int ActiveAccounts { get; set; }
        public int TotalAuditLogs { get; set; }
        public List<User> Officers { get; set; } = new();
    }
}
