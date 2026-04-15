using RoyalBank.Models;
namespace RoyalBank.Interfaces
{
    public interface IRiskRepository
    {
        Task<RiskProfile?> GetByCustomerIdAsync(int customerId);
        Task AddAsync(RiskProfile riskProfile);
        Task UpdateAsync(RiskProfile riskProfile);
    }
}
