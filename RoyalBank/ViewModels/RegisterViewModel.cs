using System.ComponentModel.DataAnnotations;
namespace RoyalBank.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be 3 to 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name must contain letters only")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^[6-9][0-9]{9}$", ErrorMessage = "Must start with 6-9 and be 10 digits")]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(255, MinimumLength = 10, ErrorMessage = "Address must be at least 10 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-20);

        [Required(ErrorMessage = "Income is required")]
        [Range(1000, 100000000, ErrorMessage = "Income must be between Rs.1,000 and Rs.10,00,00,000")]
        [Display(Name = "Annual Income (Rs.)")]
        public decimal Income { get; set; }
    }
}
