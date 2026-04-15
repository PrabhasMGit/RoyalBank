using RoyalBank.Interfaces;
using RoyalBank.Models;
using RoyalBank.ViewModels;

namespace RoyalBank.Services
{
    public class KycService
    {
        private readonly IKycRepository _kycRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly IComplianceRepository _complianceRepo;

        public KycService(IKycRepository kycRepo, ICustomerRepository customerRepo,
            IComplianceRepository complianceRepo)
        {
            _kycRepo = kycRepo; _customerRepo = customerRepo; _complianceRepo = complianceRepo;
        }

        public async Task UploadDocument(int customerId, UploadDocumentViewModel model, IWebHostEnvironment env)
        {
            var folder = Path.Combine(env.WebRootPath, "uploads", "documents");
            Directory.CreateDirectory(folder);
            var ext      = Path.GetExtension(model.DocumentFile.FileName);
            var fileName = $"{customerId}_{DateTime.Now.Ticks}{ext}";
            using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
                await model.DocumentFile.CopyToAsync(stream);

            await _kycRepo.AddAsync(new KycDocument
            {
                CustomerId         = customerId,
                DocumentType       = model.DocumentType,
                FilePath           = $"/uploads/documents/{fileName}",
                VerificationStatus = VerificationStatus.PENDING,
                UploadedAt         = DateTime.Now
            });

            var customer = await _customerRepo.GetByIdAsync(customerId);
            if (customer != null && customer.OnboardingStatus == OnboardingStatus.NEW)
            {
                customer.OnboardingStatus = OnboardingStatus.IN_PROGRESS;
                await _customerRepo.UpdateAsync(customer);
            }

            await _complianceRepo.AddAuditLogAsync(new AuditLog
            {
                CustomerId = customerId, Action = "Document Uploaded", Status = "PENDING",
                Remarks = $"Document type: {model.DocumentType}", Timestamp = DateTime.Now
            });
        }

        public async Task VerifyDocument(int documentId, bool approved, string? rejectionNote)
        {
            var doc = await _kycRepo.GetByIdAsync(documentId);
            if (doc == null) return;

            doc.VerificationStatus = approved ? VerificationStatus.VERIFIED : VerificationStatus.REJECTED;
            doc.RejectionNote      = approved ? null : rejectionNote;
            await _kycRepo.UpdateAsync(doc);

            await _complianceRepo.AddAuditLogAsync(new AuditLog
            {
                CustomerId = doc.CustomerId,
                Action     = approved ? "Document Verified" : "Document Rejected",
                Status     = approved ? "VERIFIED" : "REJECTED",
                Remarks    = approved ? "Verified by KYC Officer" : $"Rejected: {rejectionNote}",
                Timestamp  = DateTime.Now
            });
        }

        public async Task<List<KycDocument>> GetKycDocuments(int customerId) =>
            await _kycRepo.GetByCustomerIdAsync(customerId);

        public async Task<List<KycDocument>> GetAllDocuments() =>
            await _kycRepo.GetAllAsync();

        public async Task<KycDocument?> GetDocumentById(int id) =>
            await _kycRepo.GetByIdAsync(id);

        // Fix 1: Get rejection note only from the LATEST document if it is REJECTED
        // Once a new document is uploaded (PENDING or VERIFIED), rejection note disappears
        public string? GetActiveRejectionNote(List<KycDocument> docs)
        {
            if (!docs.Any()) return null;

            var latest = docs.OrderByDescending(d => d.UploadedAt).First();

            // Show rejection note only if the latest document is REJECTED
            // If customer uploaded a new doc (PENDING/VERIFIED), note is gone
            if (latest.VerificationStatus == VerificationStatus.REJECTED)
                return latest.RejectionNote;

            return null;
        }
    }
}
