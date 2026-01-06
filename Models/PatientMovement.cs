using HospitalManagement.AppStatus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class PatientMovement
    {

        [Key]
        public int MovementId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public virtual Patient? Patient { get; set; }

        [Required]
        [StringLength(100)]
        public string MovementType { get; set; } // e.g. "X-Ray", "Surgery", "Returned to Ward"

        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        [Required]
        public DateTime MovementTime { get; set; } = DateTime.Now;

        public Status MovementStatus { get; set; } = Status.Active;
    }
}
