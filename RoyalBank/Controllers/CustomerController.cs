using Microsoft.AspNetCore.Mvc;
using RoyalBank.Models;
using RoyalBank.Services;
using RoyalBank.ViewModels;
using RoyalBank.Interfaces;

namespace RoyalBank.Controllers
{
    public class CustomerController : Controller
    {
        private readonly CustomerService  _customerService;
        private readonly KycService       _kycService;
        private readonly AccountService   _accountService;
        private readonly IWebHostEnvironment _env;

        public CustomerController(CustomerService customerService, KycService kycService,
            AccountService accountService,IWebHostEnvironment env)
        {
            _customerService = customerService; _kycService = kycService;
            _accountService  = accountService; _env = env;
        }

        private int    GetCustomerId() => HttpContext.Session.GetInt32("CustomerId") ?? 0;
        private string GetRole()       => HttpContext.Session.GetString("UserRole") ?? "";
        private bool   IsCustomer()    => GetRole() == "Customer";

        // ── Register ──────────────────────────────────────────────────────────
        public IActionResult Register()
        {
            if (GetRole() != "") return RedirectToAction("Login", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            int age = DateTime.Today.Year - model.DateOfBirth.Year;
            if (model.DateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
            if (age < 18)
            {
                ModelState.AddModelError("DateOfBirth", "You must be at least 18 years old.");
                return View(model);
            }

            if (await _customerService.EmailAlreadyExists(model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            var (_, user) = await _customerService.RegisterCustomer(model);
            TempData["Info"] = "Registration successful! Now set your password.";
            return RedirectToAction("SetPassword", new { userId = user.Id });
        }

        // ── SetPassword ───────────────────────────────────────────────────────
        public IActionResult SetPassword(int userId)
        {
            if (GetRole() != "") return RedirectToAction("Login", "Home");
            return View(new SetPasswordViewModel { UserId = userId });
        }

        [HttpPost]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            bool success = await _customerService.SetPassword(model.UserId, model.Password);
            if(!success)
            {
                ModelState.AddModelError("", "User not found");
                return View(model);
			}

            TempData["Success"] = "Password set! Please login.";
            return RedirectToAction("Login", "Home");
        }

        // ── Dashboard ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            // Authorization: only Customer role
            if (!IsCustomer()) return RedirectToAction("Login", "Home");

            int cid       = GetCustomerId();
            var customer  = await _customerService.GetCustomerDetails(cid);
            if (customer == null) return RedirectToAction("Login", "Home");

            var docs        = await _kycService.GetKycDocuments(cid);
            var allAccounts = await _accountService.GetAllAccounts();
            var myAccounts  = allAccounts.Where(a => a.CustomerId == cid).ToList();

            bool kycVerified = docs.Any(d => d.VerificationStatus == VerificationStatus.VERIFIED);

            // Fix 1: Show rejection note ONLY if latest document is still REJECTED
            // Once new doc is uploaded (PENDING) or verified, note disappears
            string? rejNote = _kycService.GetActiveRejectionNote(docs);

            // Upload enabled: no docs yet OR latest doc is rejected
            bool canUpload = !docs.Any() ||
                             docs.OrderByDescending(d => d.UploadedAt)
                                 .First().VerificationStatus == VerificationStatus.REJECTED;

            ViewBag.CanUpload = canUpload;

            return View(new CustomerDashboardViewModel
            {
                Customer         = customer,
                KycDocuments     = docs,
                Accounts         = myAccounts,
                CanCreateAccount = kycVerified,
                KycRejectionNote = rejNote
            });
        }

        // ── UploadDocument ────────────────────────────────────────────────────
        public async Task<IActionResult> UploadDocument()
        {
            if (!IsCustomer()) return RedirectToAction("Login", "Home");

            var docs = await _kycService.GetKycDocuments(GetCustomerId());
            bool canUpload = !docs.Any() ||
                             docs.OrderByDescending(d => d.UploadedAt)
                                 .First().VerificationStatus == VerificationStatus.REJECTED;
            if (!canUpload)
            {
                TempData["Error"] = "Upload is disabled while your document is pending review or already verified.";
                return RedirectToAction("Dashboard");
            }
            return View(new UploadDocumentViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocument(UploadDocumentViewModel model)
        {
            if (!IsCustomer()) return RedirectToAction("Login", "Home");
            if (!ModelState.IsValid) return View(model);

            var ext = Path.GetExtension(model.DocumentFile.FileName).ToLower();
            if (!new[] { ".jpg", ".jpeg", ".png", ".pdf" }.Contains(ext))
            {
                ModelState.AddModelError("DocumentFile", "Only JPG, PNG and PDF files are allowed.");
                return View(model);
            }
            if (model.DocumentFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("DocumentFile", "File size must not exceed 5 MB.");
                return View(model);
            }

            await _kycService.UploadDocument(GetCustomerId(), model, _env);
            TempData["Success"] = "Document uploaded. Awaiting KYC Officer verification.";
            return RedirectToAction("Dashboard");
        }

        // ── UpdateProfile ─────────────────────────────────────────────────────
        public async Task<IActionResult> UpdateProfile()
        {
            if (!IsCustomer()) return RedirectToAction("Login", "Home");
            var customer = await _customerService.GetCustomerDetails(GetCustomerId());
            if (customer == null) return RedirectToAction("Login", "Home");

            return View(new UpdateProfileViewModel
            {
                CustomerId   = customer.CustomerId,
                FullName     = customer.FullName,
                MobileNumber = customer.MobileNumber,
                Address      = customer.Address
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
        {
            if (!IsCustomer()) return RedirectToAction("Login", "Home");
            if (!ModelState.IsValid) return View(model);
            model.CustomerId = GetCustomerId();
            await _customerService.UpdateCustomerProfile(model);
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Dashboard");
        }
    }
}
