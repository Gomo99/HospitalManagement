using HospitalManagement.AppStatus;

namespace HospitalManagement.ViewModel
{
    public class WardListItemVm
    {
        public int WardId { get; set; }
        public string WardName { get; set; }
        public string? Description { get; set; }
        public int Capacity { get; set; }
        public int ActiveBeds { get; set; }  // Calculated from Beds collection
        public Status WardStatus { get; set; }
    }
}
