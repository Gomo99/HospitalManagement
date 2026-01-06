using HospitalManagement.AppStatus;

namespace HospitalManagement.ViewModel
{
    public class MessageViewModel
    {
        public int MessageId { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public DateTime SentDate { get; set; }
        public DateTime? ReadDate { get; set; }
        public bool IsRead { get; set; }
        public MessagePriority Priority { get; set; }
        public string PriorityClass => Priority switch
        {
            MessagePriority.Low => "text-muted",
            MessagePriority.Normal => "text-primary",
            MessagePriority.High => "text-warning",
            MessagePriority.Urgent => "text-danger",
            _ => "text-primary"
        };

        public bool IsCurrentUser { get; set; }
        public bool IsCurrentUserSender { get; set; }
    }
}
