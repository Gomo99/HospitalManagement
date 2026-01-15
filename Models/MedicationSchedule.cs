using HospitalManagement.AppStatus;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class MedicationSchedule
    {
        [Key]
        public int ScheduleId { get; set; }

        [Required]
        public int MedicationId { get; set; }

        [Required]
        public ScheduleType ScheduleType { get; set; }

        [Required]
        public int ScheduleLevel { get; set; } // 1-5

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        public Status Status { get; set; } = Status.Active;

        [ForeignKey("MedicationId")]
        public virtual Medication? Medication { get; set; }
    }

    
}