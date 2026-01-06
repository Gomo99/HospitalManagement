using HospitalManagement.AppStatus;

namespace HospitalManagement.ViewModel
{
    public class MyPatientViewModel
    {
        public int PatientId { get; set; }
        public string FullName { get; set; }
        public DateTime DOB { get; set; }
        public GenderType Gender { get; set; }
        public DateTime AdmissionDate { get; set; }
        public List<string> Allergies { get; set; } = new List<string>();
        public List<string> Medications { get; set; } = new List<string>();
        public List<string> Conditions { get; set; } = new List<string>();
        public bool IsMyPatient { get; set; }
    }
}
