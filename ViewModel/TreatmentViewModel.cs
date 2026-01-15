using HospitalManagement.AppStatus;
using HospitalManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.ViewModel
{
    public class TreatmentViewModel
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public TreatmentType TreatmentType { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Details { get; set; }

        // Medication specific
        public int? MedicationId { get; set; }
        public decimal? Dosage { get; set; }
        public string? DosageUnit { get; set; }

        // Patient info for display
        public string? PatientName { get; set; }

        // Available medications
        public List<Medication>? AvailableMedications { get; set; }
    }
}
