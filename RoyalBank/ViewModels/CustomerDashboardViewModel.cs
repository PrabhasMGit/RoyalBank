using RoyalBank.Models;
namespace RoyalBank.ViewModels
{
    public class CustomerDashboardViewModel
    {
        public Customer Customer { get; set; } = null!;
        public List<KycDocument> KycDocuments { get; set; } = new();
        public List<Account> Accounts { get; set; } = new();
        public bool CanCreateAccount { get; set; }
        public string? KycRejectionNote { get; set; }
    }
}
