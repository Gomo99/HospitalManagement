namespace HospitalManagement.Models
{
    public class LoginAudit
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public DateTime LoginTime { get; set; }

        public bool Success { get; set; }
        public string IpAddress { get; set; }

        public string UserAgent { get; set; }
    }
}
