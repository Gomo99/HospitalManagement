using HospitalManagement.AppStatus;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Services
{
    // Services/ThemeService.cs
    public class ThemeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ThemeService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ThemeType> GetUserThemePreference(string username)
        {
            if (string.IsNullOrEmpty(username))
                return ThemeType.System;

            var user = await _context.Employees
                .Include(e => e.Preferences)
                .FirstOrDefaultAsync(e => e.UserName == username);

            return user?.Preferences?.ThemePreference ?? ThemeType.System;
        }

        public async Task SetUserThemePreference(string username, ThemeType theme)
        {
            var user = await _context.Employees
                .Include(e => e.Preferences)
                .FirstOrDefaultAsync(e => e.UserName == username);

            if (user == null) return;

            if (user.Preferences == null)
            {
                user.Preferences = new UserPreference
                {
                    EmployeeId = user.EmployeeID,
                    ThemePreference = theme,
                    LastUpdated = DateTime.Now
                };
                _context.UserPreferences.Add(user.Preferences);
            }
            else
            {
                user.Preferences.ThemePreference = theme;
                user.Preferences.LastUpdated = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public string GetActiveThemeClass(ThemeType theme)
        {
            // Check system preference if theme is set to "System"
            if (theme == ThemeType.System)
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null && httpContext.Request.Headers.ContainsKey("Sec-CH-Prefers-Color-Scheme"))
                {
                    var systemPref = httpContext.Request.Headers["Sec-CH-Prefers-Color-Scheme"].ToString();
                    return systemPref.Contains("dark") ? "dark-mode" : "light-mode";
                }
                return "light-mode"; // Default fallback
            }

            return theme == ThemeType.Dark ? "dark-mode" : "light-mode";
        }
    }
}
