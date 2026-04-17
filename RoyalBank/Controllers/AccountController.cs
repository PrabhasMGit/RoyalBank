using Microsoft.AspNetCore.Mvc;
using RoyalBank.Models;
using RoyalBank.Services;
using RoyalBank.ViewModels;

namespace RoyalBank.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;
        private readonly KycService     _kycService;

        public AccountController(AccountService accountService, KycService kycService)
        {
            _accountService = accountService;
            _kycService     = kycService;
        }

        private int GetCustomerId()
        {
            return HttpContext.Session.GetInt32("CustomerId") ?? 0;
        }
        private bool IsCustomer()
        {
            return HttpContext.Session.GetString("UserRole") == "Customer";
        }
        public async Task<IActionResult> CreateAccount()
        {
            if (!IsCustomer()) return RedirectToAction("Login", "Home");

            // Check KYC verified
            var docs = await _kycService.GetKycDocuments(GetCustomerId());
            if (!docs.Any(d => d.VerificationStatus == VerificationStatus.VERIFIED))
            {
                TempData["Error"] = "Your KYC must be verified before creating an account.";
                return RedirectToAction("Dashboard", "Customer");
            }

            var allAccounts = await _accountService.GetAllAccounts();
            var myAccounts  = allAccounts.Where(a => a.CustomerId == GetCustomerId()).ToList();

            bool hasSavings = myAccounts.Any(a => a.AccountType == "Savings");
            bool hasCurrent = myAccounts.Any(a => a.AccountType == "Current");

            if (hasSavings && hasCurrent)
            {
                TempData["Error"] = "You already have both a Savings and a Current account. No more accounts can be created.";
                return RedirectToAction("Dashboard", "Customer");
            }

            ViewBag.HasSavings = hasSavings;
            ViewBag.HasCurrent = hasCurrent;

            return View(new CreateAccountViewModel());
        }

        //CreateAccount
        [HttpPost]
        public async Task<IActionResult> CreateAccount(CreateAccountViewModel model)
        {
            if (!IsCustomer()) return RedirectToAction("Login", "Home");

            if (!ModelState.IsValid)
            {
                ViewBag.HasSavings = false;
                ViewBag.HasCurrent = false;
                return View(model);
            }

            try
            {
                await _accountService.CreateAccount(GetCustomerId(), model);
                TempData["Success"] = $"{model.AccountType} account created! Awaiting Compliance Officer approval.";
                return RedirectToAction("Dashboard", "Customer");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Dashboard", "Customer");
            }
        }
    }
}
