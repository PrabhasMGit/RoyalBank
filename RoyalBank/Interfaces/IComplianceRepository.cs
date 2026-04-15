using RoyalBank.Models;

namespace RoyalBank.Interfaces
{
    public interface IComplianceRepository
    {
        Task AddAuditLogAsync(AuditLog log);
        Task<List<AuditLog>> GetAllLogsAsync();
        Task<List<AuditLog>> GetByCustomerIdAsync(int customerId);
    }
}
