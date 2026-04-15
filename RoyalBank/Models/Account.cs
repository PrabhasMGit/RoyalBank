using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoyalBank.Models
{
    public enum AccountStatus { PENDING_APPROVAL, ACTIVE, REJECTED, INACTIVE }

    public class Account
    {
        [Key]
        public int AccountId { get; set; }

        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Account type is required")]
        [StringLength(50)]
        [Display(Name = "Account Type")]
        public string AccountType { get; set; } = string.Empty;

        public AccountStatus AccountStatus { get; set; } = AccountStatus.PENDING_APPROVAL;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? RejectionNote { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
    }
}
