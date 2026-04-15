using Microsoft.EntityFrameworkCore;
using RoyalBank.Data;
using RoyalBank.Interfaces;
using RoyalBank.Models;
namespace RoyalBank.Repositories
{
    public class RiskRepository : IRiskRepository
    {
        private readonly AppDbContext _db;
        public RiskRepository(AppDbContext db) { _db = db; }
        public async Task<RiskProfile?> GetByCustomerIdAsync(int customerId) =>
            await _db.RiskProfiles.Include(r => r.Customer).FirstOrDefaultAsync(r => r.CustomerId == customerId);
        public async Task AddAsync(RiskProfile r) { await _db.RiskProfiles.AddAsync(r); await _db.SaveChangesAsync(); }
        public async Task UpdateAsync(RiskProfile r) { _db.RiskProfiles.Update(r); await _db.SaveChangesAsync(); }
    }
}
