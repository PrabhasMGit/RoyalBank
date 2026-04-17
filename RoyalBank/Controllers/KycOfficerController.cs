using Microsoft.AspNetCore.Mvc;
using RoyalBank.Services;
using RoyalBank.ViewModels;

namespace RoyalBank.Controllers
{
    public class KycOfficerController : Controller
    {
        private readonly KycService _kycService;
        public KycOfficerController(KycService kycService) 
        {
            _kycService = kycService;
        }

        private bool IsKycOfficer()
        {
            return HttpContext.Session.GetString("UserRole") == "KYC";
        }

        //KycOfficer Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsKycOfficer()) return RedirectToAction("Login", "Home");
            return View(await _kycService.GetAllDocuments());
        }

        //VerifyDocument/{id}
        [HttpGet]
        public async Task<IActionResult> VerifyDocument(int id)
        {
            if (!IsKycOfficer()) return RedirectToAction("Login", "Home");
            var doc = await _kycService.GetDocumentById(id);
            if (doc == null) { TempData["Error"] = "Document not found."; return RedirectToAction("Dashboard"); }
            return View(doc);
        }

        //ApproveDocument
        [HttpPost]
        public async Task<IActionResult> ApproveDocument(int DocumentId)
        {
            if (!IsKycOfficer()) return RedirectToAction("Login", "Home");
            await _kycService.VerifyDocument(DocumentId, true, null);
            TempData["Success"] = "Document approved successfully.";
            return RedirectToAction("Dashboard");
        }

        //RejectDocument
        [HttpPost]
        public async Task<IActionResult> RejectDocument(int DocumentId, string? RejectionNote)
        {
            if (!IsKycOfficer()) return RedirectToAction("Login", "Home");
            if (string.IsNullOrWhiteSpace(RejectionNote))
            {
                TempData["Error"] = "Please provide a rejection reason.";
                return RedirectToAction("VerifyDocument", new { id = DocumentId });
            }
            await _kycService.VerifyDocument(DocumentId, false, RejectionNote);
            TempData["Success"] = "Document rejected. Customer has been notified.";
            return RedirectToAction("Dashboard");
        }
    }
}
