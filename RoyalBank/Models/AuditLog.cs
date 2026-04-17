using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoyalBank.Models
{
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }
        public int? CustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Remarks { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
    }
}
