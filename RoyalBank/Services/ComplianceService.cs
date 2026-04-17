using RoyalBank.Interfaces;
using RoyalBank.Models;

namespace RoyalBank.Services
{
    public class ComplianceService
    {
        private readonly IComplianceRepository _complianceRepo;
        public ComplianceService(IComplianceRepository complianceRepo) { _complianceRepo = complianceRepo; }

        public async Task LogComplianceEvent(int customerId, string action, string status, string? remarks)
        {
            await _complianceRepo.AddAuditLogAsync(new AuditLog
            {
                CustomerId = customerId,
                Action = action,
                Status = status,
                Remarks = remarks,
                Timestamp = DateTime.Now
            });
        }
        public async Task<List<AuditLog>> GetComplianceReport() => await _complianceRepo.GetAllLogsAsync();
    }
}
