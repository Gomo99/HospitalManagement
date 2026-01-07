using HospitalManagement.AppStatus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class UserPreference
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }

        // ✅ Change this from Employee to EmployeeId navigation
        public virtual Employee Employee { get; set; }

        public ThemeType ThemePreference { get; set; } = ThemeType.System;
        public DateTime? LastUpdated { get; set; }
    }
}