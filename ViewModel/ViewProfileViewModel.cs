using HospitalManagement.AppStatus;

namespace HospitalManagement.ViewModel
{
    public class ViewProfileViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public GenderType Gender { get; set; }
        public DateTime? HireDate { get; set; }
        public Status IsActive { get; set; }

        public bool IsTwoFactorEnabled { get; set; } = false;
        public string ThemePreference { get; set; }
    }
}
