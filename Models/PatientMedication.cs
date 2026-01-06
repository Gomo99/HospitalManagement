using HospitalManagement.AppStatus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class PatientMedication
    {
        [Key]
        public int PatientMedicationId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public virtual Patient Patient { get; set; } //Patient Relationship

        [Required]
        public int MedicationId { get; set; }

        [ForeignKey(nameof(MedicationId))]
        public virtual Medication Medication { get; set; }

        public Status Status { get; set; } = Status.Active;


    }
}
