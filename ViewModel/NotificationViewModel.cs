using HospitalManagement.AppStatus;

namespace HospitalManagement.ViewModel
{
    public class NotificationViewModel
    {
        public int NotificationId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string SenderName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; }
        public int? AdmissionId { get; set; }
        public int? PatientId { get; set; }
        public string PatientName { get; set; }
        public string ActionUrl { get; set; }
        public int? MessageId { get; set; }

        // Helper properties for display
        public bool IsMessageNotification => Type == NotificationType.MessageReceived;
        public bool IsPatientAssignmentNotification => Type == NotificationType.PatientAssignment;
        public bool IsAdmissionNotification => Type == NotificationType.AdmissionUpdate;
        public bool IsDischargeNotification => Type == NotificationType.PatientDischarge;

        public string PriorityClass => Priority switch
        {
            NotificationPriority.Low => "text-muted",
            NotificationPriority.Normal => "text-primary",
            NotificationPriority.High => "text-warning",
            NotificationPriority.Urgent => "text-danger",
            _ => "text-primary"
        };

        public string TypeIcon => Type switch
        {
            NotificationType.PatientAssignment => "bi-person-plus",
            NotificationType.PatientDischarge => "bi-person-check",
            NotificationType.AdmissionUpdate => "bi-clipboard-pulse",
            NotificationType.Emergency => "bi-exclamation-triangle",
            NotificationType.System => "bi-gear",
            NotificationType.MessageReceived => "bi-envelope", // Fixed: removed extra "bi"
            _ => "bi-bell" // Fixed: removed extra "bi"
        };

        public string TypeBadgeColor => Type switch
        {
            NotificationType.MessageReceived => "bg-info",
            NotificationType.PatientAssignment => "bg-success",
            NotificationType.AdmissionUpdate => "bg-primary",
            NotificationType.PatientDischarge => "bg-secondary",
            NotificationType.Emergency => "bg-danger",
            _ => "bg-secondary"
        };
    }
}
