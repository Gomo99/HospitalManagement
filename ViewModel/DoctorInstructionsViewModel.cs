using HospitalManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.ViewModel
{
    public class DoctorInstructionsViewModel
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Instructions { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? FollowUpActions { get; set; }

        public DateTime? FollowUpDate { get; set; }

        // Patient info for display
        public string? PatientName { get; set; }

        // Available doctors
        public List<Employee>? AvailableDoctors { get; set; }
    }

}
