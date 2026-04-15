using RoyalBank.Models;
namespace RoyalBank.Interfaces
{
    public interface IKycRepository
    {
        Task<KycDocument?> GetByIdAsync(int documentId);
        Task<List<KycDocument>> GetByCustomerIdAsync(int customerId);
        Task<List<KycDocument>> GetAllAsync();
        Task AddAsync(KycDocument document);
        Task UpdateAsync(KycDocument document);
    }
}
