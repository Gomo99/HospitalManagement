namespace HospitalManagement.ViewModel
{
    public class AdmissionWithPatientViewModel
    {
        public int AdmissionId { get; set; }
        public DateTime AdmissionDate { get; set; }
        public string? Notes { get; set; }

        public int PatientId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
    }
}
