using RoyalBank.Interfaces;

using RoyalBank.Models;

using RoyalBank.ViewModels;



namespace RoyalBank.Services

{

	public class AdminService

	{

		private readonly IUserRepository _userRepo;

		private readonly ICustomerRepository _customerRepo;

		private readonly IComplianceRepository _complianceRepo;



		public AdminService(IUserRepository userRepo, ICustomerRepository customerRepo,

		IComplianceRepository complianceRepo)

		{

			_userRepo = userRepo;

			_customerRepo = customerRepo;

			_complianceRepo = complianceRepo;

		}



		public async Task<List<User>> GetAllOfficers() =>

		await _userRepo.GetAllOfficersAsync();



		public async Task<bool> CreateOfficer(CreateOfficerViewModel model, string createdByEmail)

		{

			// Check email already exists

			if (await _userRepo.GetByUsernameAsync(model.Email) != null)

				return false;



			await _userRepo.AddAsync(new User

			{

				Username = model.Email,

				Password = model.Password,

				HashPassword = BCrypt.Net.BCrypt.HashPassword(model.Password),

				Role = model.Role,

				CustomerId = null

			});



			// Record in audit history

			await _complianceRepo.AddAuditLogAsync(new AuditLog

			{

				CustomerId = null,

				Action = $"{model.Role} Officer Created",

				Status = "CREATED",

				Remarks = $"Officer {model.Email} created by Admin ({createdByEmail})",

				Timestamp = DateTime.Now

			});



			return true;

		}



		public async Task DeleteOfficer(int id, string deletedByEmail)

		{

			var officer = await _userRepo.GetByIdAsync(id);

			string email = officer?.Username ?? $"ID:{id}";

			string role = officer?.Role.ToString() ?? "Officer";



			await _userRepo.DeleteAsync(id);



			await _complianceRepo.AddAuditLogAsync(new AuditLog

			{

				CustomerId = null,

				Action = $"{role} Officer Deleted",

				Status = "DELETED",

				Remarks = $"Officer {email} deleted by Admin ({deletedByEmail})",

				Timestamp = DateTime.Now

			});

		}



		public async Task<List<Customer>> GetAllCustomers() =>

		await _customerRepo.GetAllAsync();



		public async Task DeleteCustomer(int customerId, string deletedByEmail)

		{

			var customer = await _customerRepo.GetByIdAsync(customerId);

			string name = customer?.FullName ?? $"ID:{customerId}";

			string email = customer?.Email ?? "";



			await _customerRepo.DeleteAsync(customerId);



			await _complianceRepo.AddAuditLogAsync(new AuditLog

			{

				CustomerId = null,

				Action = "Customer Deleted",

				Status = "DELETED",

				Remarks = $"Customer {name} ({email}) deleted by Admin ({deletedByEmail})",

				Timestamp = DateTime.Now

			});

		}

	}
}