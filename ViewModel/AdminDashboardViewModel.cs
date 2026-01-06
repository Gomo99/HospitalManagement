namespace HospitalManagement.ViewModel
{
    public class LoginActivityData
    {
        public string Date { get; set; } // e.g., "Mon", "Tue", etc.
        public int Count { get; set; }   // logins on that day
    }

    public class AdminDashboardViewModel
    {
        public string UserName { get; set; }

        public int TotalBeds { get; set; }
        public int TotalWards { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalConsumables { get; set; }
        public int TotalMedications { get; set; }
        public int ActiveAllergies { get; set; }

        public List<LoginActivityData> WeeklyLogins { get; set; } = new();
    }
}
