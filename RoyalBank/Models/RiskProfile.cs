using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoyalBank.Models
{
    public enum RiskLevel { LOW, MEDIUM, HIGH }

    public class RiskProfile
    {
        [Key]
        public int RiskId { get; set; }

        public int CustomerId { get; set; }
        public int Score { get; set; }
        public RiskLevel RiskLevel { get; set; } = RiskLevel.LOW;
        public DateTime AssessedAt { get; set; } = DateTime.Now;

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
    }
}
