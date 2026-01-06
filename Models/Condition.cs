using HospitalManagement.AppStatus;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models
{
    public class Condition
    {
        [Key]
        public int ConditionId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } // Optional field for additional info

        [Required]
        public Status ConditionStatus { get; set; } = Status.Active;


    }
}
