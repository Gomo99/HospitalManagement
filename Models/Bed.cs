using HospitalManagement.AppStatus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class Bed
    {
        [Key]
        public int BedId { get; set; }

        [Required]
        [StringLength(50)]
        public string BedNumber { get; set; }

        [Required]
        public BedStatus Status { get; set; } 

        [Required]
        public int WardId { get; set; }

        [ForeignKey(nameof(WardId))]
        public virtual Ward Ward { get; set; }

        public Status BedStatus { get; set; } 

    }
}
