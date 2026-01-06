namespace HospitalManagement.ViewModel
{
    public class InboxViewModel
    {
        public List<MessageViewModel> Messages { get; set; }
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
    }
}
