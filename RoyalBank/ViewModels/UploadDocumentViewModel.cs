using System.ComponentModel.DataAnnotations;
namespace RoyalBank.ViewModels
{
    public class UploadDocumentViewModel
    {
        [Required(ErrorMessage = "Please select a document type")]
        [Display(Name = "Document Type")]
        public string DocumentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a file")]
        [Display(Name = "Document File")]
        public IFormFile DocumentFile { get; set; } = null!;
    }
}
