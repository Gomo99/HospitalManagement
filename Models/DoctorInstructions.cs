using HospitalManagement.AppStatus;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class DoctorInstructions
    {
        [Key]
        public int InstructionId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public int? RecordedByEmployeeId { get; set; } // Nurse ID who recorded

        [Required]
        public DateTime InstructionDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Instructions { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? FollowUpActions { get; set; }

        public DateTime? FollowUpDate { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedDate { get; set; }

        public int? CompletedByEmployeeId { get; set; } // Nurse who completed

        public Status Status { get; set; } = Status.Active;

        // Navigation Properties
        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Employee? Doctor { get; set; }

        [ForeignKey("RecordedByEmployeeId")]
        public virtual Employee? RecordedBy { get; set; }

        [ForeignKey("CompletedByEmployeeId")]
        public virtual Employee? CompletedBy { get; set; }
    }
}