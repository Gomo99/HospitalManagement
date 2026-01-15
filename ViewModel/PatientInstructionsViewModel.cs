using HospitalManagement.Models;

namespace HospitalManagement.ViewModel
{
    public class PatientInstructionsViewModel
    {
        public int PatientId { get; set; }
        public string? PatientName { get; set; }
        public List<DoctorInstructions>? Instructions { get; set; }
    }

}
