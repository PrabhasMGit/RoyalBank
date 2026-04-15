using System.ComponentModel.DataAnnotations;
namespace RoyalBank.ViewModels
{
    public class UpdateProfileViewModel
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be 3 to 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name must contain letters only")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^[6-9][0-9]{9}$", ErrorMessage = "Must start with 6-9 and be 10 digits")]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(255, MinimumLength = 10, ErrorMessage = "Address must be at least 10 characters")]
        public string Address { get; set; } = string.Empty;
    }
}
