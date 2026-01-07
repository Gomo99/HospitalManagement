using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace HospitalManagement.AppStatus
{
    public enum Status
    {
        Active,
        Delete,
        Deactivated,
        Admitted,
        Discharged,
        Inactive,
        Pending,
        Cancelled
    }
    
    // Specific status for patient admissions. Keeps Admission states isolated from generic Status enum.
    public enum AdmissionStatus
    {
        [Display(Name = "Admitted")]
        Admitted,

        [Display(Name = "Discharged")]
        Discharged,

        [Display(Name = "Transferred")]
        Transferred,

        [Display(Name = "On Hold")]
        OnHold,

        [Display(Name = "Pending Admission")]
        PendingAdmission,

        [Display(Name = "Cancelled")]
        Cancelled
    }
    public enum MedicationType
    {
        [Display(Name = "Prescription Medication")]
        Prescription,

        [Display(Name = "Over the Counter")]
        OverTheCounter,

        [Display(Name = "Supplement")]
        Supplement,

        [Display(Name = "Other")]
        Other,

        [Display(Name = "Tablet")]
        Tablet,

        [Display(Name = "Syrup")]
        Syrup,

        [Display(Name = "Injection")]
        Injection
    }

    public enum BedStatus
    {
        [Display(Name = "Available")]
        Available,

        [Display(Name = "Occupied")]
        Occupied,

        [Display(Name = "Under Maintenance")]
        UnderMaintenance
    }


    public enum ConsumableType
    {
        [Display(Name = "Medical Supply")]
        MedicalSupply,

        [Display(Name = "Office Supply")]
        OfficeSupply,

        [Display(Name = "Cleaning Supply")]
        CleaningSupply,

        [Display(Name = "Other")]
        Other,

        [Display(Name = "Medication")]
        Medication,

        [Display(Name = "Diagnostic")]
        Diagnosticher,

        [Display(Name = "Surgical")]
        Surgical,


        [Display(Name = "Diagnostic")]
        Diagnostic,



    }
    public enum ConsumableOrderStatus
    {
        [Display(Name = "Requsted")]
        Requsted,

        [Display(Name = "Delivered")]
        Delivered,

        [Display(Name = "Received")]
        Received,



    }
    public enum PrescriptionStatus
    {
        [Display(Name = "New")]
        New,


        [Display(Name = "Processed")]
        Processed,

        [Display(Name = "ForwardedToPharmacy")]
        ForwardedToPharmacy,

        [Display(Name = "DeliveredToWard")]
        DeliveredToWard,


        [Display(Name = "Verified")]
        Verified,


    }



    public enum UserRole
    {
        [Display(Name = "Administrator")]
        ADMINISTRATOR,

        [Display(Name = "Ward Administrator")]
        WARDADMIN,


        [Display(Name = "Nurse")]
        NURSE,

        [Display(Name = "Nursing Sister")]
        NURSINGSISTER,

        [Display(Name = "Doctor")]
        DOCTOR,

        [Display(Name = "Script Manager")]
        SCRIPTMANAGER,

        [Display(Name = "Consumables Manager")]
        CONSUMABLESMANAGER,
    }


    public enum GenderType
    {
        Male,
        Female,
        PreferNotToSay,
    }


    public enum MessagePriority
    {
        Low,
        Normal,
        High,
        Urgent
    }


    public enum NotificationType
    {
        PatientAssignment,
        PatientDischarge,
        AdmissionUpdate,
        Emergency,
        System,
        MessageReceived
    }

    public enum NotificationPriority
    {
        Low,
        Normal,
        High,
        Urgent
    }

    // Models/ThemeType.cs
    public enum ThemeType
    {
        Light,      // Default
        Dark,
        System     // Follow system preference
    }

    public static class EnumExtensions
    {
        public static string GetDisplayName<T>(this T enumValue) where T : struct, System.Enum
        {
            var memberInfo = typeof(T).GetMember(enumValue.ToString());
            if (memberInfo.Length == 0) return enumValue.ToString();
            var displayAttr = memberInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false)
                .OfType<DisplayAttribute>()
                .FirstOrDefault();
            return displayAttr?.GetName() ?? enumValue.ToString();
        }
    }


}
