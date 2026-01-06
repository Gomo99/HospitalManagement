using HospitalManagement.AppStatus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class Discharge
    {
        [Key]
        public int DischargeId { get; set; }

        // ✅ Reference Employee by EmployeeID instead of ConsumableManagerId
        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public Employee Employee { get; set; }

        [Required(ErrorMessage = "Ward Admin ID is required")]
        public int WardAdminId { get; set; }

        [Required(ErrorMessage = "Discharge date is required")]

        public DateOnly DischargeDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required(ErrorMessage = "Discharge summary is required")]
        [StringLength(2000, ErrorMessage = "Discharge notes cannot exceed 2000 characters")]
        public string Notes { get; set; }


        public Status DischargeStatus { get; set; } = Status.Active;


        [Required]
        public int PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public virtual Patient Patient { get; set; }




    }
}
