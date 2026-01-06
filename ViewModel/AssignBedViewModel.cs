using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.ViewModel
{
    public class AssignBedViewModel
    {
        public int PatientId { get; set; }

        [Display(Name = "Patient Name")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Please select a bed")]
        [Display(Name = "Bed")]
        public int BedId { get; set; }

        public int AssignmentId { get; set; } // AdmissionId from your controller



        [Display(Name = "Available Beds")]
        public IEnumerable<SelectListItem> AvailableBeds { get; set; } = new List<SelectListItem>();

        // Additional properties you might need
        public DateTime AssignmentDate { get; set; } = DateTime.Now;

    }

}
