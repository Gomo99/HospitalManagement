using HospitalManagement.AppStatus;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.ViewModel
{
    // ViewModels/ThemeViewModel.cs
    public class ThemeViewModel
    {
        [Required]
        public ThemeType SelectedTheme { get; set; }
    }
}
