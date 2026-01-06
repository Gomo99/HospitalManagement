using HospitalManagement.AppStatus;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.ViewModel
{
    public class ReplyMessageViewModel
    {
        [Required]
        public int ReceiverId { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Content { get; set; }

        public MessagePriority Priority { get; set; } = MessagePriority.Normal;

        [Required]
        public int OriginalMessageId { get; set; }
    }
}
