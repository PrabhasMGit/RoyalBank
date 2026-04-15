using System.ComponentModel.DataAnnotations;
namespace RoyalBank.ViewModels
{
    public class CreateAccountViewModel
    {
        [Required(ErrorMessage = "Please select an account type")]
        [Display(Name = "Account Type")]
        public string AccountType { get; set; } = string.Empty;
    }
}
