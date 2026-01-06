using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.ViewModel
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; }  // The reset token from email

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
