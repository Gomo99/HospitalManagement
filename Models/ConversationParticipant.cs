namespace HospitalManagement.Models
{
    public class ConversationParticipant
    {
        public int ConversationParticipantId { get; set; }
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public DateTime JoinedDate { get; set; } = DateTime.Now;
        public DateTime? LastReadDate { get; set; }
    }
}
