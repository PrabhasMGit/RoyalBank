using RoyalBank.Interfaces;
using RoyalBank.Models;
using RoyalBank.ViewModels;

namespace RoyalBank.Services
{
    public class AccountService
    {
        private readonly IAccountRepository  _accountRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly IKycRepository      _kycRepo;
        private readonly IComplianceRepository _complianceRepo;

        public AccountService(IAccountRepository accountRepo, ICustomerRepository customerRepo,
            IKycRepository kycRepo, IComplianceRepository complianceRepo)
        {
            _accountRepo    = accountRepo;
            _customerRepo   = customerRepo;
            _kycRepo        = kycRepo;
            _complianceRepo = complianceRepo;
        }

        public async Task<Account> CreateAccount(int customerId, CreateAccountViewModel model)
        {
            // Check KYC is verified
            var docs = await _kycRepo.GetByCustomerIdAsync(customerId);
            if (!docs.Any(d => d.VerificationStatus == VerificationStatus.VERIFIED))
                throw new InvalidOperationException("KYC must be verified before creating an account.");

            // Fix 1: Block creating the same account type if one already exists
            // regardless of status (PENDING, ACTIVE, REJECTED, INACTIVE)
            // This prevents re-creating after compliance officer rejection
            var existing = await _accountRepo.GetByCustomerIdAsync(customerId);
            bool alreadyHasThisType = existing.Any(a =>
                a.AccountType.Equals(model.AccountType, StringComparison.OrdinalIgnoreCase));

            if (alreadyHasThisType)
                throw new InvalidOperationException(
                    $"You already have a {model.AccountType} account. " +
                    $"Only one {model.AccountType} account is allowed per customer.");

            var account = new Account
            {
                CustomerId    = customerId,
                AccountType   = model.AccountType,
                AccountStatus = AccountStatus.PENDING_APPROVAL,
                CreatedDate   = DateTime.Now
            };
            await _accountRepo.AddAsync(account);

            await _complianceRepo.AddAuditLogAsync(new AuditLog
            {
                CustomerId = customerId,
                Action     = "Account Created",
                Status     = "PENDING_APPROVAL",
                Remarks    = $"Account type: {model.AccountType} created by customer",
                Timestamp  = DateTime.Now
            });
            return account;
        }

        public async Task ApproveAccount(int accountId)
        {
            var account = await _accountRepo.GetByIdAsync(accountId);
            if (account == null) return;
            account.AccountStatus = AccountStatus.ACTIVE;
            await _accountRepo.UpdateAsync(account);

            var customer = await _customerRepo.GetByIdAsync(account.CustomerId);
            if (customer != null)
            {
                customer.OnboardingStatus = OnboardingStatus.COMPLETED;
                await _customerRepo.UpdateAsync(customer);
            }

            await _complianceRepo.AddAuditLogAsync(new AuditLog
            {
                CustomerId = account.CustomerId,
                Action     = "Account Approved",
                Status     = "ACTIVE",
                Remarks    = $"Account #{accountId} ({account.AccountType}) approved by Compliance Officer",
                Timestamp  = DateTime.Now
            });
        }

        public async Task RejectAccount(int accountId, string? note)
        {
            var account = await _accountRepo.GetByIdAsync(accountId);
            if (account == null) return;
            account.AccountStatus = AccountStatus.REJECTED;
            account.RejectionNote = note;
            await _accountRepo.UpdateAsync(account);

            var customer = await _customerRepo.GetByIdAsync(account.CustomerId);
            if (customer != null)
            {
                customer.OnboardingStatus = OnboardingStatus.COMPLETED;
                await _customerRepo.UpdateAsync(customer);
            }

            await _complianceRepo.AddAuditLogAsync(new AuditLog
            {
                CustomerId = account.CustomerId,
                Action     = "Account Rejected",
                Status     = "REJECTED",
                Remarks    = $"Account #{accountId} ({account.AccountType}) rejected by Compliance Officer. Reason: {note}",
                Timestamp  = DateTime.Now
            });
        }

        public async Task DeactivateAccount(int accountId)
        {
            var account = await _accountRepo.GetByIdAsync(accountId);
            if (account == null) return;
            account.AccountStatus = AccountStatus.INACTIVE;
            await _accountRepo.UpdateAsync(account);

            await _complianceRepo.AddAuditLogAsync(new AuditLog
            {
                CustomerId = account.CustomerId,
                Action     = "Account Deactivated",
                Status     = "INACTIVE",
                Remarks    = $"Account #{accountId} ({account.AccountType}) deactivated by Admin",
                Timestamp  = DateTime.Now
            });
        }

        public async Task<Account?>      GetAccountDetails(int accountId) => await _accountRepo.GetByIdAsync(accountId);
        public async Task<List<Account>> GetAllAccounts()                  => await _accountRepo.GetAllAsync();
    }
}
