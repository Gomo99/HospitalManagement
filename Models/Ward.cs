using HospitalManagement.AppStatus;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models
{
    public class Ward
    {
        [Key]
        public int WardId { get; set; }

        [Required]
        [StringLength(100)]
        public string WardName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public Status WardStatus { get; set; } = Status.Active;  // Soft delete status
        [Required]
        public int Capacity { get; set; }  // Maximum number of beds allowed

        // Navigation property for related beds
        public virtual ICollection<Bed> Beds { get; set; } = new List<Bed>();  // Initialize to avoid null reference issues
    
    }
}
