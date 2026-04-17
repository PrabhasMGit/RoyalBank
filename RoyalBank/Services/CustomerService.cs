using RoyalBank.Interfaces;
using RoyalBank.Models;
using RoyalBank.ViewModels;
namespace RoyalBank.Services
{   public class CustomerService
    {
        private readonly ICustomerRepository  _customerRepo;
        private readonly IComplianceRepository _complianceRepo;
        private readonly IUserRepository      _userRepo;
        public CustomerService(ICustomerRepository customerRepo,IComplianceRepository complianceRepo, IUserRepository userRepo)
        {
            _customerRepo   = customerRepo;
            _complianceRepo = complianceRepo;
            _userRepo       = userRepo;
        }
        public async Task<(Customer customer, User user)> RegisterCustomer(RegisterViewModel model)
        {
            var customer = new Customer
            {
                FullName         = model.FullName,
                Email            = model.Email,
                MobileNumber     = model.MobileNumber,
                Address          = model.Address,
                DateOfBirth      = model.DateOfBirth,
                Income           = model.Income,
                OnboardingStatus = OnboardingStatus.NEW
            };
            await _customerRepo.AddAsync(customer);

            var user = new User
            {
                Username     = model.Email,
                Password     = string.Empty,
                HashPassword = string.Empty,
                Role         = UserRole.Customer,
                CustomerId   = customer.CustomerId
            };
            await _userRepo.AddAsync(user);

            await _complianceRepo.AddAuditLogAsync(new AuditLog
            {
                CustomerId = customer.CustomerId,
                Action     = "Customer Registered",
                Status     = "NEW",
                Remarks    = $"{customer.FullName} registered with email {customer.Email}",
                Timestamp  = DateTime.Now
            });

            return (customer, user);
        }
        public async Task UpdateCustomerProfile(UpdateProfileViewModel model)
        {
            var customer = await _customerRepo.GetByIdAsync(model.CustomerId);
            if (customer == null) return;

            customer.FullName     = model.FullName;
            customer.MobileNumber = model.MobileNumber;
            customer.Address      = model.Address;
            await _customerRepo.UpdateAsync(customer);
        }
        public async Task<Customer?> GetCustomerDetails(int customerId) =>
            await _customerRepo.GetByIdAsync(customerId);

        public async Task<List<Customer>> GetAllCustomers() =>
            await _customerRepo.GetAllAsync();
		public async Task<bool> EmailAlreadyExists(string email)
		{
			var user = await _userRepo.GetByUsernameAsync(email);
			return user != null;
		}
		public async Task<bool> SetPassword(int userId, string password)
		{
			var user = await _userRepo.GetByIdAsync(userId);
			if (user == null) return false;
			user.Password = password;
			user.HashPassword = BCrypt.Net.BCrypt.HashPassword(password);
			await _userRepo.UpdateAsync(user);
			return true;
		}
		public async Task<User?> GetUserById(int userId)
		{
			return await _userRepo.GetByIdAsync(userId);
		}
	}
}
