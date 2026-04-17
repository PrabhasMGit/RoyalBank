using System.ComponentModel.DataAnnotations;

namespace RoyalBank.Models
{
    public enum UserRole 
    { 
        Admin, Compliance, KYC, Customer 
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string HashPassword { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Customer;

        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }
    }
}
