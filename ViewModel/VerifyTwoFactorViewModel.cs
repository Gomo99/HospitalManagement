using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.ViewModel
{
    public class VerifyTwoFactorViewModel
    {
        [Required]
        [Display(Name = "Verification Code")]
        public string Code { get; set; }

        [Display(Name = "Remember This Device")]
        public bool RememberDevice { get; set; }
    }
}
