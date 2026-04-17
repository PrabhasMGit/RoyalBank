using Microsoft.EntityFrameworkCore;
using RoyalBank.Data;
using RoyalBank.Interfaces;
using RoyalBank.Models;

namespace RoyalBank.Repositories
{
    public class ComplianceRepository : IComplianceRepository
    {
        private readonly AppDbContext _db;
        public ComplianceRepository(AppDbContext db) 
        { 
            _db = db; 
        }

        public async Task AddAuditLogAsync(AuditLog log)
        {
            await _db.AuditLogs.AddAsync(log);
            await _db.SaveChangesAsync();
        }

        public async Task<List<AuditLog>> GetAllLogsAsync() =>
            await _db.AuditLogs
                .Include(l => l.Customer)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

        public async Task<List<AuditLog>> GetByCustomerIdAsync(int customerId) =>
            await _db.AuditLogs
                .Where(l => l.CustomerId == customerId)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
    }
}
