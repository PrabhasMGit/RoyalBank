using Microsoft.AspNetCore.Mvc;
using RoyalBank.Interfaces;
using RoyalBank.Models;
using RoyalBank.Services;
using RoyalBank.ViewModels;

namespace RoyalBank.Controllers
{
    public class HomeController : Controller
    {
        private readonly AuthService _authService;

        public HomeController(AuthService authService)
        {
            _authService = authService;
        }

        // GET /Home/Index  →  redirect to Login
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserRole") != null)
                return RedirectToDashboard(HttpContext.Session.GetString("UserRole")!);

            return RedirectToAction("Login");
        }

        // GET /Home/Login
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserRole") != null)
                return RedirectToDashboard(HttpContext.Session.GetString("UserRole")!);

            return View();
        }

        // POST /Home/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _authService.ValidateLogin(model.Email,model.Password);
            if(user==null)
            {   ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
			}

			// Save login info in session
			HttpContext.Session.SetInt32("UserId",     user.Id);
            HttpContext.Session.SetString("UserEmail", user.Username);
            HttpContext.Session.SetString("UserRole",  user.Role.ToString());
            HttpContext.Session.SetInt32("CustomerId", user.CustomerId ?? 0);

            return RedirectToDashboard(user.Role.ToString());
        }

        // GET /Home/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        // GET /Home/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToDashboard(string role)
        {
            return role switch
            {
                "Admin"      => RedirectToAction("Dashboard", "Admin"),
                "Compliance" => RedirectToAction("Dashboard", "ComplianceOfficer"),
                "KYC"        => RedirectToAction("Dashboard", "KycOfficer"),
                _            => RedirectToAction("Dashboard", "Customer")
            };
        }
    }
}
