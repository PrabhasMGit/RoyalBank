using Microsoft.EntityFrameworkCore;
using RoyalBank.Data;
using RoyalBank.Interfaces;
using RoyalBank.Models;
namespace RoyalBank.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _db;
        public AccountRepository(AppDbContext db) 
        { 
            _db = db; 
        }
        public async Task<Account?> GetByIdAsync(int id) =>
            await _db.Accounts.Include(a => a.Customer).ThenInclude(c => c!.RiskProfile)
                .Include(a => a.Customer).ThenInclude(c => c!.KycDocuments)
                .FirstOrDefaultAsync(a => a.AccountId == id);
        public async Task<List<Account>> GetByCustomerIdAsync(int cid) =>
            await _db.Accounts.Where(a => a.CustomerId == cid).ToListAsync();
        public async Task<List<Account>> GetAllAsync() =>
            await _db.Accounts.Include(a => a.Customer).ThenInclude(c => c!.RiskProfile)
                .Include(a => a.Customer).ThenInclude(c => c!.KycDocuments)
                .OrderByDescending(a => a.CreatedDate).ToListAsync();
        public async Task AddAsync(Account a) 
        { 
            await _db.Accounts.AddAsync(a); 
            await _db.SaveChangesAsync(); 
        }
        public async Task UpdateAsync(Account a) 
        { 
            _db.Accounts.Update(a); 
            await _db.SaveChangesAsync(); 
        }
    }
}
