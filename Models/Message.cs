using HospitalManagement.AppStatus;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Content { get; set; }

        public int SenderId { get; set; }
        public Employee Sender { get; set; }

        public int ReceiverId { get; set; }
        public Employee Receiver { get; set; }

        public DateTime SentDate { get; set; } = DateTime.Now;
        public DateTime? ReadDate { get; set; }

        private bool _isRead;
        public bool IsRead
        {
            get => ReadDate.HasValue;
            set => _isRead = value;
        }
        public bool IsDeletedBySender { get; set; }
        public bool IsDeletedByReceiver { get; set; }

        // For group messages (optional enhancement)
        public bool IsGroupMessage { get; set; }
        public string? AttachmentUrl { get; set; } // For file attachments
        public MessagePriority Priority { get; set; } = MessagePriority.Normal;
    }
}
