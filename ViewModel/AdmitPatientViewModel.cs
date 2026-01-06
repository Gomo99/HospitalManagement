using HospitalManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.ViewModel
{
    public class AdmitPatientViewModel
    {
        [Required]
        [Display(Name = "Patient")]
        public int SelectedPatientId { get; set; }
        public IEnumerable<Patient> PatientOptions { get; set; } = new List<Patient>();

        [Display(Name = "Allergies")]
        public List<int> SelectedAllergyIds { get; set; } = new();

        [Display(Name = "Medications")]
        public List<int> SelectedMedicationIds { get; set; } = new();

        [Display(Name = "Conditions")]
        public List<int> SelectedConditionIds { get; set; } = new();

        [Required]
        [Display(Name = "Doctor")]
        public int SelectedDoctorId { get; set; }
        public IEnumerable<Employee> DoctorOptions { get; set; } = new List<Employee>();

        [Required]
        [Display(Name = "Nurse")]
        public int SelectedNurseId { get; set; }
        public IEnumerable<Employee> NurseOptions { get; set; } = new List<Employee>();

        public IEnumerable<Allergy> AllergyOptions { get; set; } = new List<Allergy>();
        public IEnumerable<Medication> MedicationOptions { get; set; } = new List<Medication>();
        public IEnumerable<Condition> ConditionOptions { get; set; } = new List<Condition>();
    }

}
