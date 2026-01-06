using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.ViewModel
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
