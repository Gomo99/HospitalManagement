using HospitalManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.ViewModel
{
    public class EditAdmissionViewModel
    {
        public int AdmissionId { get; set; }
        public int PatientId { get; set; }

        // Selected IDs
        [Display(Name = "Allergies")]
        public List<int> SelectedAllergyIds { get; set; } = new();

        [Display(Name = "Medications")]
        public List<int> SelectedMedicationIds { get; set; } = new();

        [Display(Name = "Conditions")]
        public List<int> SelectedConditionIds { get; set; } = new();

        [Display(Name = "Doctor")]
        public int SelectedDoctorId { get; set; }

        [Display(Name = "Nurse")]
        public int SelectedNurseId { get; set; }

        // For dropdown lists
        public IEnumerable<Allergy> AllergyOptions { get; set; } = new List<Allergy>();
        public IEnumerable<Medication> MedicationOptions { get; set; } = new List<Medication>();
        public IEnumerable<Condition> ConditionOptions { get; set; } = new List<Condition>();

        public IEnumerable<Employee> DoctorOptions { get; set; } = new List<Employee>();
        public IEnumerable<Employee> NurseOptions { get; set; } = new List<Employee>();
    }
}
