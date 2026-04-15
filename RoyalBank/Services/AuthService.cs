using RoyalBank.Interfaces;

using RoyalBank.Models;



namespace RoyalBank.Services

{

	public class AuthService

	{

		private readonly IUserRepository _userRepo;



		public AuthService(IUserRepository userRepo)

		{

			_userRepo = userRepo;

		}



		// Called by HomeController.Login

		public async Task<User?> ValidateLogin(string email, string password)

		{

			var user = await _userRepo.GetByUsernameAsync(email);

			if (user == null) return null;

			if (string.IsNullOrEmpty(user.HashPassword)) return null;

			if (!BCrypt.Net.BCrypt.Verify(password, user.HashPassword)) return null;

			return user;

		}

	}

}