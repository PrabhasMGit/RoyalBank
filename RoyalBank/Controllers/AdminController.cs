using Microsoft.AspNetCore.Mvc;
using RoyalBank.Interfaces;
using RoyalBank.Models;
using RoyalBank.Services;
using RoyalBank.ViewModels;

namespace RoyalBank.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminService _adminService;
        private readonly AccountService       _accountService;
        private readonly ComplianceService    _complianceService;

        public AdminController(AdminService adminService,AccountService accountService, ComplianceService complianceService)
        {
            _adminService      = adminService;
            _accountService    = accountService;
            _complianceService = complianceService;
        }

        private bool   IsAdmin()    => HttpContext.Session.GetString("UserRole") == "Admin";
        private string AdminEmail() => HttpContext.Session.GetString("UserEmail") ?? "Admin";

       
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");

            var customers = await _adminService.GetAllCustomers();
            var accounts  = await _accountService.GetAllAccounts();
            var officers  = await _adminService.GetAllOfficers();
            var logs      = await _complianceService.GetComplianceReport();

            return View(new AdminDashboardViewModel
            {
                TotalCustomers  = customers.Count,
                PendingAccounts = accounts.Count(a => a.AccountStatus == AccountStatus.PENDING_APPROVAL),
                ActiveAccounts  = accounts.Count(a => a.AccountStatus == AccountStatus.ACTIVE),
                TotalAuditLogs  = logs.Count,
                Officers        = officers
            });
        }

        // GET /Admin/CreateOfficer
        public IActionResult CreateOfficer()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            return View(new CreateOfficerViewModel());
        }

        // POST /Admin/CreateOfficer
        [HttpPost]
        public async Task<IActionResult> CreateOfficer(CreateOfficerViewModel model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            if (!ModelState.IsValid) return View(model);

            if (model.Role != UserRole.Compliance && model.Role != UserRole.KYC)
            {
                ModelState.AddModelError("Role", "Role must be KYC or Compliance.");
                return View(model);
            }

            bool created=await _adminService.CreateOfficer(model,AdminEmail());
            if (!created)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

			TempData["Success"] = $"{model.Role} Officer created successfully. Email: {model.Email}";
            return RedirectToAction("OfficersList");
        }

        // GET /Admin/OfficersList
        public async Task<IActionResult> OfficersList()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            return View(await _adminService.GetAllOfficers());
        }

        // POST /Admin/DeleteOfficer
        [HttpPost]
        public async Task<IActionResult> DeleteOfficer(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");

            await _adminService.DeleteOfficer(id, AdminEmail());

			TempData["Success"] = "Officer deleted successfully.";
            return RedirectToAction("OfficersList");
        }

        // GET /Admin/CustomersList
        public async Task<IActionResult> CustomersList()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            return View(await _adminService.GetAllCustomers());
        }

        // POST /Admin/DeleteCustomer
        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");

            await _adminService.DeleteCustomer(id, AdminEmail());

			TempData["Success"] = "Customer and all related data deleted successfully.";
            return RedirectToAction("CustomersList");
        }

        // GET /Admin/AccountsList
        public async Task<IActionResult> AccountsList()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            return View(await _accountService.GetAllAccounts());
        }

        // POST /Admin/DeactivateAccount
        [HttpPost]
        public async Task<IActionResult> DeactivateAccount(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            await _accountService.DeactivateAccount(id);
            TempData["Success"] = "Account deactivated.";
            return RedirectToAction("AccountsList");
        }

        // GET /Admin/AuditRecord
        public async Task<IActionResult> AuditRecord()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            return View(await _complianceService.GetComplianceReport());
        }
    }
}
