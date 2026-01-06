using HospitalManagement.AppStatus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public GenderType Gender { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Id Number")]
        public string IdNumber { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Contact")]
        public String Cellphone { get; set; }

        public Status GetStatus { get; set; } = Status.Active;

        // ✅ Use join table instead of direct collection
        public virtual ICollection<PatientAllergy> PatientAllergies { get; set; }
        public virtual ICollection<PatientMedication> PatientMedications { get; set; }
        public virtual ICollection<PatientCondition> PatientConditions { get; set; }
        public virtual ICollection<PatientMovement> PatientMovements { get; set; }


        public virtual ICollection<Admission> Admissions { get; set; }
        public virtual ICollection<PatientFolder> PatientFolders { get; set; } = new List<PatientFolder>();
        
        public virtual ICollection<Discharge> Discharges { get; set; }


        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

    }
}