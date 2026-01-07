using HospitalManagement.AppStatus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class Admission
    {
        [Key]
        public int AdmissionId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime AdmissionDate { get; set; }


        [Required]
        public int PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public virtual Patient Patient { get; set; }

        [Required]
        public int EmployeeID { get; set; } // Doctor
        [ForeignKey(nameof(EmployeeID))]
        public Employee Doctor { get; set; }

        [Required]
        public int NurseID { get; set; } // 👈 New Nurse relation
        [ForeignKey(nameof(NurseID))]
        public Employee Nurse { get; set; }


        [StringLength(100)]
        public string? Notes { get; set; } = ""; // ✅ Optional

        public DateTime? DischargeDate { get; set; } // ✅ Optional
        public Status AdmissionStatus { get; set; } = Status.Active;
    }
}
