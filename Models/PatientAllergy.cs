using HospitalManagement.AppStatus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class PatientAllergy
    {
        [Key]
        public int PatientAllergyId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public virtual Patient Patient { get; set; }

        [Required]
        public int AllergyId { get; set; }

        [ForeignKey(nameof(AllergyId))]
        public virtual Allergy Allergy { get; set; }

        public Status Status { get; set; } = Status.Active;
    }
}
