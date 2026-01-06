using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.ViewModel
{
    public class DischargeViewModel
    {
        [Key]
        public int PatientId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime AdmissionDate { get; set; }


        [Required]
        [StringLength(100)]
        public string Ward { get; set; }
    }
}
