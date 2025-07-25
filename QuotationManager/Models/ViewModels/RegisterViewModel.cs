using System.ComponentModel.DataAnnotations;

namespace QuotationManager.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public IFormFile SignatureImage { get; set; }

        [Required]
        public IFormFile StampImage { get; set; }

        [Required]
        public string Designation { get; set; }
    }
}
