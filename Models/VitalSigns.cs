using HospitalManagement.AppStatus;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class VitalSigns
    {
        [Key]
        public int VitalSignsId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int? TakenByEmployeeId { get; set; } // Nurse ID

        [Required]
        public DateTime RecordedDateTime { get; set; } = DateTime.Now;

        // Blood Pressure
        public string? BloodPressureSystolic { get; set; }
        public string? BloodPressureDiastolic { get; set; }

        // Temperature
        public decimal? Temperature { get; set; }
        public TemperatureUnit? TemperatureUnit { get; set; }

        // Heart Rate
        public int? HeartRate { get; set; }

        // Respiratory Rate
        public int? RespiratoryRate {  get; set; }

        // Oxygen Saturation
        public decimal? OxygenSaturation { get; set; }

        // Blood Sugar/Glucose
        public decimal? BloodSugar { get; set; }
        public GlucoseUnit? GlucoseUnit { get; set; }

        // Pain Level
        [Range(0, 10)]
        public int? PainLevel { get; set; }

        // Additional Notes
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Status
        public Status Status { get; set; } = Status.Active;

        // Navigation Properties
        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        [ForeignKey("TakenByEmployeeId")]
        public virtual Employee? TakenBy { get; set; }

        // Computed Properties
        [NotMapped]
        public string BloodPressure =>
            !string.IsNullOrEmpty(BloodPressureSystolic) && !string.IsNullOrEmpty(BloodPressureDiastolic)
                ? $"{BloodPressureSystolic}/{BloodPressureDiastolic}"
                : "N/A";

        [NotMapped]
        public string TemperatureWithUnit =>
            Temperature.HasValue && TemperatureUnit.HasValue
                ? $"{Temperature.Value}°{TemperatureUnit}"
                : "N/A";

        [NotMapped]
        public string BloodSugarWithUnit =>
            BloodSugar.HasValue && GlucoseUnit.HasValue
                ? $"{BloodSugar.Value} {GlucoseUnit}"
                : "N/A";
    }
}