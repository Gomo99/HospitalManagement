using HospitalManagement.AppStatus;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class Treatment
    {
        [Key]
        public int TreatmentId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int? AdministeredByEmployeeId { get; set; } // Nurse ID

        [Required]
        public DateTime TreatmentDateTime { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public TreatmentType TreatmentType { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Details { get; set; }

        // Medication specific (if applicable)
        public int? MedicationId { get; set; }
        public decimal? Dosage { get; set; }
        public string? DosageUnit { get; set; }

        // Status
        public Status Status { get; set; } = Status.Active;

        // Navigation Properties
        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        [ForeignKey("AdministeredByEmployeeId")]
        public virtual Employee? AdministeredBy { get; set; }

        [ForeignKey("MedicationId")]
        public virtual Medication? Medication { get; set; }
    }
}