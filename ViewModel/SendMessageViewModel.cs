using HospitalManagement.AppStatus;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.ViewModel
{
    public class SendMessageViewModel
    {
        [Required(ErrorMessage = "Please select a recipient")]
        [Display(Name = "To")]
        public int ReceiverId { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Message content is required")]
        [StringLength(5000, ErrorMessage = "Message cannot exceed 5000 characters")]
        [Display(Name = "Message")]
        public string Content { get; set; }

        [Display(Name = "Priority")]
        public MessagePriority Priority { get; set; } = MessagePriority.Normal;

        [BindNever]
        public List<SelectListItem> Employees { get; set; }


        public bool IsCurrentUser { get; set; }
        public bool IsCurrentUserSender { get; set; }

    }
}
