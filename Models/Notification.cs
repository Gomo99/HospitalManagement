using HospitalManagement.AppStatus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public int? SenderId { get; set; } // WardAdmin who sent the notification
        public Employee Sender { get; set; }

        public int ReceiverId { get; set; } // Doctor or Nurse who receives
        public Employee Receiver { get; set; }

        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ReadDate { get; set; }
        [NotMapped]
        public bool IsRead => ReadDate.HasValue;

        // Link to the related admission
        public int? AdmissionId { get; set; }
        public Admission Admission { get; set; }

        public int? PatientId { get; set; }
        public Patient Patient { get; set; }

        public string ActionUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public int? MessageId { get; set; } // Link to the actual message

    }

}
