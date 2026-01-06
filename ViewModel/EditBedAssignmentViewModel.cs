using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.ViewModel
{
    public class EditBedAssignmentViewModel
    {
        public int AssignmentId { get; set; }

        public int PatientId { get; set; }

        [Display(Name = "Patient Name")]
        public string PatientName { get; set; } = "";

        [Display(Name = "Current Bed")]
        public int CurrentBedId { get; set; }

        [Display(Name = "Current Bed")]
        public string CurrentBedInfo { get; set; } = "";

        [Required(ErrorMessage = "Please select a bed")]
        [Display(Name = "New Bed")]
        public int NewBedId { get; set; }

        [Required]
        [Display(Name = "Assignment Date")]
        [DataType(DataType.DateTime)]
        public DateTime AssignedDate { get; set; } = DateTime.Now;

        [Display(Name = "Available Beds")]
        public IEnumerable<SelectListItem> AvailableBeds { get; set; } = new List<SelectListItem>();


        // Helper property to check if bed is changing
        public bool IsBedChanging => NewBedId != CurrentBedId;
    }
}
