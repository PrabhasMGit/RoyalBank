using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoyalBank.Models
{
    public enum VerificationStatus { PENDING, VERIFIED, REJECTED }

    public class KycDocument
    {
        [Key]
        public int DocumentId { get; set; }

        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Document type is required")]
        [StringLength(50)]
        [Display(Name = "Document Type")]
        public string DocumentType { get; set; } = string.Empty;

        [StringLength(255)]
        public string FilePath { get; set; } = string.Empty;

        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.PENDING;

        [StringLength(500)]
        public string? RejectionNote { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
    }
}
