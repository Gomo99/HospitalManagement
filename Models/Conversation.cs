namespace HospitalManagement.Models
{
    public class Conversation
    {
        public int ConversationId { get; set; }
        public string Title { get; set; }
        public bool IsGroup { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastMessageDate { get; set; } = DateTime.Now;

        // Navigation properties
        public ICollection<ConversationParticipant> Participants { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
