using Microsoft.EntityFrameworkCore;
using RoyalBank.Data;
using RoyalBank.Interfaces;
using RoyalBank.Models;
namespace RoyalBank.Repositories
{
    public class KycRepository : IKycRepository
    {
        private readonly AppDbContext _db;
        public KycRepository(AppDbContext db) { _db = db; }
        public async Task<KycDocument?> GetByIdAsync(int id) =>
            await _db.KycDocuments.Include(k => k.Customer).FirstOrDefaultAsync(k => k.DocumentId == id);
        public async Task<List<KycDocument>> GetByCustomerIdAsync(int customerId) =>
            await _db.KycDocuments.Where(k => k.CustomerId == customerId).OrderByDescending(k => k.UploadedAt).ToListAsync();
        public async Task<List<KycDocument>> GetAllAsync() =>
            await _db.KycDocuments.Include(k => k.Customer).OrderByDescending(k => k.UploadedAt).ToListAsync();
        public async Task AddAsync(KycDocument d) 
        { 
            await _db.KycDocuments.AddAsync(d); 
            await _db.SaveChangesAsync(); 
        }
        public async Task UpdateAsync(KycDocument d) 
        { 
            _db.KycDocuments.Update(d); 
            await _db.SaveChangesAsync(); 
        }
    }
}
