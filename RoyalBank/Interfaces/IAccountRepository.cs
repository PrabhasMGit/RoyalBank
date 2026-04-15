using RoyalBank.Models;
namespace RoyalBank.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(int accountId);
        Task<List<Account>> GetByCustomerIdAsync(int customerId);
        Task<List<Account>> GetAllAsync();
        Task AddAsync(Account account);
        Task UpdateAsync(Account account);
    }
}
