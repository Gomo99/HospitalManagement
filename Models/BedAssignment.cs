using HospitalManagement.AppStatus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class BedAssignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public virtual Patient Patient { get; set; }

        [Required]
        public int BedId { get; set; }

        [ForeignKey(nameof(BedId))]
        public virtual Bed Bed { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime AssignedDate { get; set; } = DateTime.Now;

        public Status AssignmentStatus { get; set; } = Status.Active;
    }
}
