using HospitalManagement.AppStatus;

namespace HospitalManagement.ViewModel
{
    public class AdmittedPatientListViewModel
    {
        public int PatientId { get; set; }
        public int AdmissionId { get; set; } // ✅ Add this
        public string FullName { get; set; }
        public DateTime DOB { get; set; }
        public GenderType Gender { get; set; }
        public string IdNumber { get; set; }
        public string Cellphone { get; set; }

        public Status AdmissionStatus { get; set; }
        public DateTime AdmissionDate { get; set; }

        public string DoctorName { get; set; }
        public string NurseName { get; set; }

        public List<string> Allergies { get; set; } = new();
        public List<string> Medications { get; set; } = new();
        public List<string> Conditions { get; set; } = new();
    }
}
