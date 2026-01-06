namespace HospitalManagement.ViewModel
{
    public class NotificationSummaryViewModel
    {
        public int UnreadCount { get; set; }
        public List<NotificationViewModel> RecentNotifications { get; set; }

        // Add constructor to initialize the list
        public NotificationSummaryViewModel()
        {
            RecentNotifications = new List<NotificationViewModel>();
        }
    }
}
