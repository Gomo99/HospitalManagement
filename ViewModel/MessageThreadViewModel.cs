namespace HospitalManagement.ViewModel
{
    public class MessageThreadViewModel
    {
        public int CurrentUserId { get; set; }
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public List<MessageViewModel> Messages { get; set; } = new List<MessageViewModel>();
        public SendMessageViewModel NewMessage { get; set; } = new SendMessageViewModel();

        // Add these properties to avoid validation issues
        public int OriginalMessageId { get; set; }
        public string LastSubject { get; set; }
    }
}
