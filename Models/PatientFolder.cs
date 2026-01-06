using HospitalManagement.AppStatus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class PatientFolder
    {
        [Key]
        public int FolderId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public virtual Patient Patient { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Notes { get; set; }

        public Status Status { get; set; } = Status.Active;


        public string OpenedBy { get; set; }
    }
}
