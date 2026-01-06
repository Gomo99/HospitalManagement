using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.ViewModel
{
    public class LogPatientMovementViewModel
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }

        public int OldBedId { get; set; }
        public string OldBedNumber { get; set; }
        public string OldWardName { get; set; }

        [Required]
        public int NewBedId { get; set; }

        public List<SelectListItem> AvailableBeds { get; set; }

        [Required]
        public string MovementType { get; set; }

        public string? Notes { get; set; }

        public DateTime MovementTime { get; set; } = DateTime.Now;
    }
}
