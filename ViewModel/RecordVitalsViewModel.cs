using HospitalManagement.AppStatus;
using HospitalManagement.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.ViewModel
{
    public class RecordVitalsViewModel
    {
        [Required]
        public int PatientId { get; set; }

        public string? BloodPressureSystolic { get; set; }

        public string? BloodPressureDiastolic { get; set; }

        public decimal? Temperature { get; set; }

        public TemperatureUnit? TemperatureUnit { get; set; }

        public int? HeartRate { get; set; }

        public int? RespiratoryRate { get; set; }

        public decimal? OxygenSaturation { get; set; }

        public decimal? BloodSugar { get; set; }

        public GlucoseUnit? GlucoseUnit { get; set; }

        [Range(0, 10)]
        public int? PainLevel { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Patient info for display
        public string? PatientName { get; set; }
    }

   

  

    
   



}