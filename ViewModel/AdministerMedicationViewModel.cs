using HospitalManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.ViewModel
{
    public class AdministerMedicationViewModel
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public int MedicationId { get; set; }

        [Required]
        public decimal Dosage { get; set; }

        [Required]
        [MaxLength(50)]
        public string DosageUnit { get; set; } = "mg";

        [Required]
        [MaxLength(200)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Patient info for display
        public string? PatientName { get; set; }

        // Medication info
        public string? MedicationName { get; set; }
        public int? ScheduleLevel { get; set; }

        // Available medications (filtered by schedule)
        public List<Medication>? AvailableMedications { get; set; }
    }
}
