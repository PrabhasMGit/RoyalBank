using System.ComponentModel.DataAnnotations;
using Moq;
using NUnit.Framework;
using RoyalBank.Interfaces;
using RoyalBank.Models;
using RoyalBank.Services;
using RoyalBank.ViewModels;

namespace RoyalBank.Tests
{
	[TestFixture]
	public class CustomerServiceTests
	{
		private Mock<ICustomerRepository> _customerRepoMock;
		private Mock<IUserRepository> _userRepoMock;
		private Mock<IComplianceRepository> _complianceRepoMock;
		private CustomerService _service;

		[SetUp]
		public void Setup()
		{
			_customerRepoMock = new Mock<ICustomerRepository>();
			_userRepoMock = new Mock<IUserRepository>();
			_complianceRepoMock = new Mock<IComplianceRepository>();

			_service = new CustomerService(
				_customerRepoMock.Object,
				_complianceRepoMock.Object,
				_userRepoMock.Object
			);
		}

		[Test]
		public async Task RegisterCustomer_WithValidData_ShouldPassEverything()
		{
		
			var model = new RegisterViewModel
			{
				FullName = "John kumar",           // Valid: Letters only
				Email = "john.kumar@gmail.com",  // Valid: Correct format
				MobileNumber = "9876543210",       // Valid: Starts with 9, 10 digits
				Address = "123 Royal Bank Street, Chennai", // Valid: > 10 chars
				Income = 50000,                    // Valid: > 1000
				DateOfBirth = new DateTime(1995, 1, 1)
			};

			// 1. Manually Validate the ViewModel (Checking DataAnnotations)
			var context = new ValidationContext(model);
			var validationResults = new List<ValidationResult>();
			bool isModelValid = Validator.TryValidateObject(model, context, validationResults, true);

			// --- ACT ---
			// 2. Call the actual service method
			var result = await _service.RegisterCustomer(model);

			// --- ASSERT ---

			// A. Check Validation Logic
			// If this fails, it means your 'model' data above broke one of your rules
			Assert.That(isModelValid, Is.True,
				$"ViewModel validation failed! Error: {validationResults.FirstOrDefault()?.ErrorMessage}");

			// B. Check Mapping Logic (The 'True' intent of your service method)
			Assert.Multiple(() =>
			{
				Assert.That(result.customer.FullName, Is.EqualTo(model.FullName));
				Assert.That(result.customer.Email, Is.EqualTo(model.Email));
				Assert.That(result.user.Username, Is.EqualTo(model.Email));
				Assert.That(result.user.Role, Is.EqualTo(UserRole.Customer));
			});

			// C. Verify Repositories were called
			_customerRepoMock.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Once);
			_userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
		}
	}
}