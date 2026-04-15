using RoyalBank.Interfaces;
using RoyalBank.Models;

namespace RoyalBank.Services
{
    public class RiskService
    {
        private readonly IRiskRepository _riskRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly IComplianceRepository _complianceRepo;

        public RiskService(IRiskRepository riskRepo, ICustomerRepository customerRepo, IComplianceRepository complianceRepo)
        {
            _riskRepo = riskRepo; _customerRepo = customerRepo; _complianceRepo = complianceRepo;
        }

        public async Task<RiskProfile> CalculateRiskScore(int customerId)
        {
            var customer = await _customerRepo.GetByIdAsync(customerId) ?? throw new Exception("Customer not found");

            int age = DateTime.Today.Year - customer.DateOfBirth.Year;
            if (customer.DateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;

            int score = 0;
            // Income scoring
            if (customer.Income < 200000) score += 40;
            else if (customer.Income < 500000) score += 25;
            else if (customer.Income < 1000000) score += 15;
            else score += 5;
            // Age scoring
            if (age < 25) score += 30;
            else if (age < 40) score += 15;
            else if (age < 60) score += 10;
            else score += 20;

            RiskLevel level = score >= 50 ? RiskLevel.HIGH : score >= 25 ? RiskLevel.MEDIUM : RiskLevel.LOW;

            var existing = await _riskRepo.GetByCustomerIdAsync(customerId);
            if (existing != null)
            {
                existing.Score = score; existing.RiskLevel = level; existing.AssessedAt = DateTime.Now;
                await _riskRepo.UpdateAsync(existing);
                return existing;
            }

            var profile = new RiskProfile { CustomerId = customerId, Score = score, RiskLevel = level, AssessedAt = DateTime.Now };
            await _riskRepo.AddAsync(profile);
            await _complianceRepo.AddAuditLogAsync(new AuditLog
            {
                CustomerId = customerId, Action = "Risk Score Calculated", Status = level.ToString(),
                Remarks = $"Score:{score}, Level:{level}, Age:{age}, Income:Rs.{customer.Income}", Timestamp = DateTime.Now
            });
            return profile;
        }

        public async Task<RiskProfile?> GetRiskResult(int customerId) => await _riskRepo.GetByCustomerIdAsync(customerId);
    }
}
