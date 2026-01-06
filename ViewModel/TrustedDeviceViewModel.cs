namespace HospitalManagement.ViewModel
{
    public class TrustedDeviceViewModel
    {
        public int Id { get; set; }
        public string DeviceName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUsed { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
