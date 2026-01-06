using HospitalManagement.AppStatus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class PatientCondition
    {
        [Key]
        public int PatientConditionId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public virtual Patient Patient { get; set; } //Patient Relationship

        [Required]
        public int ConditionId { get; set; }

        [ForeignKey(nameof(ConditionId))]
        public virtual Condition Condition { get; set; }

        public Status Status { get; set; } = Status.Active;
    }
}
