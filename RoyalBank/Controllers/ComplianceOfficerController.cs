using Microsoft.AspNetCore.Mvc;
using RoyalBank.Models;
using RoyalBank.Services;

namespace RoyalBank.Controllers
{
    public class ComplianceOfficerController : Controller
    {
        private readonly AccountService    _accountService;
        private readonly RiskService       _riskService;

        public ComplianceOfficerController(AccountService accountService, RiskService riskService)
        {
            _accountService = accountService;
            _riskService    = riskService;
        }

        private bool IsCompliance()
        {
            return HttpContext.Session.GetString("UserRole") == "Compliance";
        }

        //ComplianceOfficer Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsCompliance()) return RedirectToAction("Login", "Home");

            var accounts = await _accountService.GetAllAccounts();
            var filtered = accounts.Where(a =>
                a.Customer != null &&
                a.Customer.KycDocuments.Any(k => k.VerificationStatus == VerificationStatus.VERIFIED)
            ).ToList();

            return View(filtered);
        }

        //ComplianceOfficer AccountDetail
        [HttpGet]
        public async Task<IActionResult> AccountDetail(int id)
        {
            if (!IsCompliance()) return RedirectToAction("Login", "Home");

            var account = await _accountService.GetAccountDetails(id);
            if (account == null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("Dashboard");
            }
            ViewBag.RiskProfile = await _riskService.GetRiskResult(account.CustomerId);
            return View(account);
        }

        //CalculateRiskScore
        [HttpPost]
        public async Task<IActionResult> CalculateRiskScore(int customerId, int accountId)
        {
            if (!IsCompliance()) return RedirectToAction("Login", "Home");
            await _riskService.CalculateRiskScore(customerId);
            TempData["Success"] = "Risk score calculated.";
            return RedirectToAction("AccountDetail", new { id = accountId });
        }

        //ApproveAccount
        [HttpPost]
        public async Task<IActionResult> ApproveAccount(int AccountId)
        {
            if (!IsCompliance()) return RedirectToAction("Login", "Home");
            await _accountService.ApproveAccount(AccountId);
            TempData["Success"] = "Account approved and activated.";
            return RedirectToAction("Dashboard");
        }

        //RejectAccount
        [HttpPost]
        public async Task<IActionResult> RejectAccount(int AccountId, string? RejectionNote)
        {
            if (!IsCompliance()) return RedirectToAction("Login", "Home");
            if (string.IsNullOrWhiteSpace(RejectionNote))
            {
                TempData["Error"] = "Please provide a rejection reason.";
                return RedirectToAction("AccountDetail", new { id = AccountId });
            }
            await _accountService.RejectAccount(AccountId, RejectionNote);
            TempData["Success"] = "Account rejected.";
            return RedirectToAction("Dashboard");
        }
    }
}
