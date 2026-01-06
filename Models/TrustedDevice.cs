using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models
{
    public class TrustedDevice
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        [Required]
        public string DeviceId { get; set; } // Hashed device identifier

        [Required]
        public string DeviceName { get; set; } // User-friendly device name

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastUsed { get; set; } = DateTime.Now;
        public DateTime ExpiryDate { get; set; } // When the trust expires

        public bool IsExpired => ExpiryDate < DateTime.Now;
    }
}
