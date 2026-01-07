using HospitalManagement.AppStatus;
using HospitalManagement.Data;
using HospitalManagement.Models;
using HospitalManagement.Services;
using HospitalManagement.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HospitalManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly TwoFactorAuthService _twoFactorService;
        private readonly ThemeService _themeService;
        public AccountController(ApplicationDbContext context, EmailService emailService,
           TwoFactorAuthService twoFactorService, ThemeService themeService)
        {
            _context = context;
            _emailService = emailService;
            _twoFactorService = twoFactorService;
            _themeService = themeService;
        }


        



        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // In AccountController.cs - Update the Login method
        [HttpPost]
        public async Task<IActionResult> Login(Login model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid input.";
                return View(model);
            }

            var user = _context.Employees
                .FirstOrDefault(u =>
                    (u.UserName.ToLower() == model.UserNameorEmail.ToLower() ||
                     u.Email.ToLower() == model.UserNameorEmail.ToLower()));

            if (user == null)
            {
                TempData["ErrorMessage"] = $"No user found with username/email '{model.UserNameorEmail}'";
                return View(model);
            }

            // Check if account is locked
            if (user.IsLockedOut)
            {
                var timeRemaining = user.LockoutEnd.Value - DateTime.Now;
                TempData["ErrorMessage"] = $"Account is locked. Please try again in {timeRemaining.Minutes} minutes and {timeRemaining.Seconds} seconds.";
                return View(model);
            }

            // Check if account is inactive
            if (user.IsActive != Status.Active)
            {
                TempData["ErrorMessage"] = "Your account is not active. Please contact administrator.";
                return View(model);
            }

            bool isPasswordValid;
            if (user.PasswordHash.StartsWith("$2"))
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
            }
            else
            {
                isPasswordValid = model.Password == user.PasswordHash;
            }

            if (!isPasswordValid)
            {
                // Increment failed login attempts
                user.FailedLoginAttempts++;

                // Check if should lock account (after 5 failed attempts)
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEnd = DateTime.Now.AddMinutes(15);
                    TempData["ErrorMessage"] = "Too many failed login attempts. Your account has been locked for 15 minutes.";
                }
                else
                {
                    int attemptsRemaining = 5 - user.FailedLoginAttempts;
                    TempData["ErrorMessage"] = $"Password does not match. {attemptsRemaining} attempt(s) remaining before lockout.";
                }

                _context.Update(user);
                await _context.SaveChangesAsync();
                return View(model);
            }

            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            _context.Update(user);

            // Generate device ID for trust checking
            var userAgent = Request.Headers["User-Agent"].ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var deviceId = _twoFactorService.GenerateDeviceId(userAgent, ipAddress);

            // Check if 2FA is enabled and device is not trusted
            if (user.IsTwoFactorEnabled && !await _twoFactorService.IsDeviceTrusted(user.EmployeeID, deviceId))
            {
                // Store login info in session for 2FA verification
                HttpContext.Session.SetString("TempUserId", user.EmployeeID.ToString());
                HttpContext.Session.SetString("TempDeviceId", deviceId);
                HttpContext.Session.SetString("TempRememberDevice", model.RememberDevice.ToString()); // ✅ Pass RememberDevice choice
                HttpContext.Session.SetString("TempRememberMe", model.RememberMe.ToString());

                // Redirect to 2FA verification
                return RedirectToAction("VerifyTwoFactor");
            }

            // Complete login (either no 2FA or trusted device)
            await CompleteLogin(user, deviceId, model.RememberDevice, model.RememberMe);
            TempData["SuccessMessage"] = $"Welcome back, {user.FirstName} {user.LastName}!";
            return RedirectToAction("ViewProfile", "Account");
        }





        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["SuccessMessage"] = "You have been successfully logged out.";
            TempData["MessageType"] = "success";

            return RedirectToAction(nameof(Login));
        }


        public IActionResult AccessDenied()
        {
            return View();
        }


        public IActionResult StatusCode(int code)
        {
            ViewBag.Code = code;
            return View();
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var username = User.Identity.Name;

            var user = await _context.Employees.FirstOrDefaultAsync(e => e.UserName == username);
            if (user == null) return NotFound();

            var model = new EditProfileViewModel
            {
                EmployeeID = user.EmployeeID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                Gender = user.Gender
            };


            ViewBag.GenderList = new SelectList(Enum.GetValues(typeof(GenderType))
                            .Cast<GenderType>()
                            .Select(g => new { Id = g, Name = g.ToString() }),
                            "Id", "Name", model.Gender);

            return View(model);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeeID);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Gender = model.Gender;
            user.UserName = model.UserName;

            _context.Employees.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Dashboard));
        }


        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var username = User.Identity.Name;
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            // Verify the current password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
            {
                TempData["ErrorMessage"] = "Current password is incorrect.";
                return View(model);
            }

            // Hash the new password using BCrypt
            user.PasswordHash = model.NewPassword;

            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password changed successfully.";
            return user.Role switch
            {
                UserRole.ADMINISTRATOR => RedirectToAction("AdminDashboard", "Admin"),
                UserRole.NURSE => RedirectToAction("NurseDashboard", "Nurse"),
                UserRole.DOCTOR => RedirectToAction("DoctorDashboard", "Doctor"),
                UserRole.SCRIPTMANAGER => RedirectToAction("ScriptManagerDashboard", "ScriptManager"),
                UserRole.CONSUMABLESMANAGER => RedirectToAction("ConsumablesManagerDashboard", "ConsumablesManager"),
                UserRole.WARDADMIN => RedirectToAction("WardAdminDashboard", "WardAdmin"),
                UserRole.NURSINGSISTER => RedirectToAction("NursingSisterDashboard", "NursingSister"),
                _ => RedirectToAction("Index", "Home"),
            };
        }



        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DeactivateAccount()
        {
            var username = User.Identity.Name;
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateAccountConfirmed()
        {
            var username = User.Identity.Name;
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            // Set the status to Deactivated
            user.IsActive = Status.Deactivated;
            _context.Update(user);
            await _context.SaveChangesAsync();

            // Sign out the user
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["SuccessMessage"] = "Your account has been deactivated.";
            return RedirectToAction("Login");
        }





        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                string resetPin = new Random().Next(100000, 999999).ToString();

                // Hash the reset PIN before saving it
                string hashedResetPin = BCrypt.Net.BCrypt.HashPassword(resetPin);

                // Prepare placeholders for dynamic content
                var placeholders = new Dictionary<string, string>
        {
            { "Name", user.FirstName + "" + user.LastName },
            { "Email", email },
            { "ResetPin", resetPin }  // Send the plain PIN to the user in the email
        };

                // Send email using the template
                var subject = "Password Reset Request - Future_Med Solution";
                await _emailService.SendEmailWithTemplateAsync(email, subject, "password-reset-template.html", placeholders);

                // Save the hashed PIN and expiration time in the database
                user.ResetPin = hashedResetPin;
                user.ResetPinExpiration = DateTime.Now.AddMinutes(5);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "A reset PIN has been sent to your email.";
                return View("ResetPassword");
            }

            TempData["ErrorMessage"] = "The email address you entered is not associated with any account.";
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(string pin, string newPassword)
        {
            // Validate the PIN: it should be 6 digits long
            if (pin.Length != 6 || !pin.All(char.IsDigit))
            {
                TempData["ErrorMessage"] = "PIN must be a 6-digit code.";
                return View();
            }

            // Validate the PIN from the database (assuming you store it as a hashed value)
            var user = _context.Employees.FirstOrDefault(a => a.ResetPinExpiration > DateTime.Now);

            if (user != null && BCrypt.Net.BCrypt.Verify(pin, user.ResetPin))  // Verify the hashed PIN
            {
                // If the PIN is correct, hash the new password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

                // Update the user's password with the new hashed password
                user.PasswordHash = hashedPassword;

                // Clear the ResetPin after successful password change (for security reasons)
                user.ResetPin = null;

                // Save changes to the database
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Password reset successfully. Please log in.";
                return RedirectToAction("Login");

            }

            // If PIN is invalid, return an error message
            TempData["ErrorMessage"] = "Invalid PIN.";
            return View();
        }



        [HttpGet]
        public async Task<IActionResult> VerifyEmail(int userId, string token)
        {
            // Fetch user, ignoring any global query filters (e.g., IsActive filter)
            var user = await _context.Employees
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.EmployeeID == userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            // If already active, just redirect to login
            if (user.IsActive == Status.Active)
            {
                TempData["SuccessMessage"] = "Email already verified. Please log in.";
                return RedirectToAction("Login");
            }

            // Validate token
            if (string.IsNullOrEmpty(user.EmailVerificationTokenHash) ||
                user.EmailVerificationTokenHash != token ||
                user.EmailVerificationTokenExpires < DateTime.UtcNow)
            {
                TempData["ErrorMessage"] = "Invalid or expired verification link. You can request a new one.";
                return RedirectToAction("ResendVerification");
            }

            // Token is valid → check registration source
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                // User was created by admin → go to SetPassword
                HttpContext.Session.SetString("SetupEmployeeId", user.EmployeeID.ToString());
                HttpContext.Session.SetString("SetupToken", token);
                HttpContext.Session.SetInt32("SetupExpiry", (int)DateTime.UtcNow.AddMinutes(30).Ticks);

                return RedirectToAction("SetPassword");
            }
            else
            {
                // User registered themselves → activate and go to Login
                user.IsActive = Status.Active;
                user.EmailVerificationTokenHash = null;
                user.EmailVerificationTokenExpires = null;
                await _context.SaveChangesAsync();
                await SendEmployeeDetailsEmail(user);
                TempData["SuccessMessage"] = "Email verified successfully! Please log in.";
                return RedirectToAction("Login");
            }
        }




        [HttpGet]
        public async Task<IActionResult> SetPassword()
        {
            var employeeIdStr = HttpContext.Session.GetString("SetupEmployeeId");
            var token = HttpContext.Session.GetString("SetupToken");

            if (string.IsNullOrEmpty(employeeIdStr) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Session expired. Please start the verification process again.";
                return RedirectToAction("Login");
            }

            var employeeId = int.Parse(employeeIdStr);

            var employee = await _context.Employees
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.EmployeeID == employeeId);

            if (employee == null || employee.EmailVerificationTokenHash != token)
            {
                TempData["ErrorMessage"] = "Invalid or expired session.";
                return RedirectToAction("Login");
            }

            // 🚨 Prevent access if user already has a password
            if (!string.IsNullOrEmpty(employee.PasswordHash))
            {
                TempData["InfoMessage"] = "Your account already has a password. Please log in.";
                return RedirectToAction("Login");
            }

            return View(new SetPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var employeeIdStr = HttpContext.Session.GetString("SetupEmployeeId");
            var token = HttpContext.Session.GetString("SetupToken");

            if (string.IsNullOrEmpty(employeeIdStr) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Session expired. Please start the verification process again.";
                return RedirectToAction("Login");
            }

            var employeeId = int.Parse(employeeIdStr);

            var employee = await _context.Employees
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.EmployeeID == employeeId);

            if (employee == null || employee.EmailVerificationTokenHash != token)
            {
                TempData["ErrorMessage"] = "Invalid verification token.";
                return RedirectToAction("Login");
            }

            // 🚨 Extra safeguard: if password already exists, block setup
            if (!string.IsNullOrEmpty(employee.PasswordHash))
            {
                TempData["InfoMessage"] = "Your account already has a password. Please log in.";
                return RedirectToAction("Login");
            }

            // Hash and save password
            employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            employee.IsActive = Status.Active;
            employee.EmailVerificationTokenHash = null;
            employee.EmailVerificationTokenExpires = null;

            await _context.SaveChangesAsync();

            // Clear session
            HttpContext.Session.Remove("SetupEmployeeId");
            HttpContext.Session.Remove("SetupToken");

            // Optional: send welcome email
            await SendEmployeeDetailsEmail(employee);

            TempData["SuccessMessage"] = "Password set successfully! You can now login.";
            return RedirectToAction("Login");
        }









        [HttpGet]
        public IActionResult ResendVerification()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerification(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Please enter a valid email.";
                return View();
            }

            // Fetch user ignoring global query filters (in case Inactive users are filtered out)
            var user = await _context.Employees
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                TempData["ErrorMessage"] = "No account associated with this email.";
                return View();
            }

            if (user.IsActive == Status.Active)
            {
                TempData["SuccessMessage"] = "Your email is already verified. Please log in.";
                return RedirectToAction("Login");
            }

            // Generate a new verification token (plain text)
            string token = Guid.NewGuid().ToString();
            user.EmailVerificationTokenHash = token;
            user.EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24);

            await _context.SaveChangesAsync();

            // Build verification link
            string verificationLink = Url.Action("VerifyEmail", "Account", new { userId = user.EmployeeID, token = token }, Request.Scheme);

            // Prepare placeholders for email template
            var placeholders = new Dictionary<string, string>
{
    { "EmployeeName", user.FullName }, // matches template {{EmployeeName}}
    { "VerificationLink", verificationLink } // matches template {{VerificationLink}}
};


            var subject = "Email Verification - Future_Med Solution";
            await _emailService.SendEmailWithTemplateAsync(user.Email, subject, "EmployeeWelcomeTemplate.html", placeholders);

            TempData["SuccessMessage"] = "Verification email has been resent. Please check your inbox.";
            return RedirectToAction("Login");
        }





        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ViewProfile()
        {
            // Get the currently logged-in user's username
            var username = User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            // Fetch the employee from the database by username
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            // Map to ViewModel
            var model = new ViewProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role.ToString(),
                Gender = user.Gender,
                HireDate = user.HireDate,
                IsActive = user.IsActive,
                IsTwoFactorEnabled = user.IsTwoFactorEnabled
            };

            return View(model);
        }






        [Authorize]
        [HttpGet]
        public async Task<IActionResult> SetupTwoFactor()
        {
            var username = User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            var user = await _context.Employees.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            if (user.IsTwoFactorEnabled)
            {
                TempData["ErrorMessage"] = "Two-factor authentication is already enabled.";
                return RedirectToAction("ViewProfile");
            }

            var secretKey = _twoFactorService.GenerateSecretKey();
            var setupCode = _twoFactorService.GenerateQrCode(user.Email, secretKey);

            var viewModel = new TwoFactorSetupViewModel
            {
                QrCodeImageUrl = setupCode.QrCodeSetupImageUrl,
                ManualEntryKey = secretKey
            };

            HttpContext.Session.SetString("TempSecretKey", secretKey);
            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetupTwoFactor(TwoFactorSetupViewModel model)
        {
            var username = User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            // Fetch user by username asynchronously
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            // ✅ FIXED: Check if 2FA is ALREADY enabled (should prevent setup)
            if (user.IsTwoFactorEnabled) // Changed from !user.IsTwoFactorEnabled
            {
                TempData["ErrorMessage"] = "Two-factor authentication is already enabled.";
                return RedirectToAction("ViewProfile");
            }

            var secretKey = HttpContext.Session.GetString("TempSecretKey");
            if (string.IsNullOrEmpty(secretKey))
            {
                TempData["ErrorMessage"] = "Session expired. Please try again.";
                return RedirectToAction("SetupTwoFactor");
            }

            if (!_twoFactorService.ValidatePin(secretKey, model.VerificationCode))
            {
                TempData["ErrorMessage"] = "Invalid verification code. Please try again.";
                var setupCode = _twoFactorService.GenerateQrCode(user.Email, secretKey);
                model.QrCodeImageUrl = setupCode.QrCodeSetupImageUrl;
                model.ManualEntryKey = secretKey;
                return View(model);
            }

            var recoveryCodes = _twoFactorService.GenerateRecoveryCodes();
            user.IsTwoFactorEnabled = true;
            user.TwoFactorSecretKey = secretKey;
            user.TwoFactorRecoveryCodes = string.Join(",", recoveryCodes);

            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("TempSecretKey");

            // Send recovery codes via email
            await SendRecoveryCodesEmail(user, recoveryCodes);

            model.RecoveryCodes = recoveryCodes;
            TempData["SuccessMessage"] = "Two-factor authentication has been enabled successfully! Recovery codes were emailed to you.";
            return View("TwoFactorRecoveryCodes", model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DisableTwoFactor()
        {
            var username = User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            // Fetch user by username asynchronously
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            return View(new DisableTwoFactorViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableTwoFactor(DisableTwoFactorViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var username = User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            // Fetch user by username asynchronously
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            if (user.PasswordHash != model.CurrentPassword)
            {
                TempData["ErrorMessage"] = "Current password is incorrect.";
                return View(model);
            }

            if (!_twoFactorService.ValidatePin(user.TwoFactorSecretKey, model.VerificationCode))
            {
                TempData["ErrorMessage"] = "Invalid verification code.";
                return View(model);
            }

            user.IsTwoFactorEnabled = false;
            user.TwoFactorSecretKey = null;
            user.TwoFactorRecoveryCodes = null;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Two-factor authentication has been disabled successfully.";
            return RedirectToAction("ViewProfile");
        }






        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DownloadProfilePdf()
        {
            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            var user = await _context.Employees.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            // Create PDF
            using (var memoryStream = new MemoryStream())
            {
                var doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 25, 25, 30, 30);
                var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, memoryStream);
                doc.Open();

                // Title
                var titleFont = iTextSharp.text.FontFactory.GetFont("Arial", 18, iTextSharp.text.Font.BOLD);
                doc.Add(new iTextSharp.text.Paragraph("Profile Information", titleFont));
                doc.Add(new iTextSharp.text.Paragraph(" ")); // empty line

                // Profile details
                var normalFont = iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL);
                doc.Add(new iTextSharp.text.Paragraph($"Full Name: {user.FirstName} {user.LastName}", normalFont));
                doc.Add(new iTextSharp.text.Paragraph($"Username: {user.UserName}", normalFont));
                doc.Add(new iTextSharp.text.Paragraph($"Email: {user.Email}", normalFont));
                doc.Add(new iTextSharp.text.Paragraph($"Role: {user.Role}", normalFont));
                doc.Add(new iTextSharp.text.Paragraph($"Gender: {user.Gender}", normalFont));
                doc.Add(new iTextSharp.text.Paragraph($"Hire Date: {user.HireDate?.ToString("yyyy-MM-dd") ?? "Not specified"}", normalFont));
                doc.Add(new iTextSharp.text.Paragraph($"Account Status: {user.IsActive}", normalFont));
                doc.Add(new iTextSharp.text.Paragraph(" ")); // empty line
                doc.Add(new iTextSharp.text.Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}", normalFont));

                doc.Close();

                byte[] bytes = memoryStream.ToArray();
                return File(bytes, "application/pdf", $"Profile_{user.UserName}.pdf");
            }
        }






        // In AccountController.cs
        [HttpGet]
        public IActionResult VerifyTwoFactor()
        {
            var userIdStr = HttpContext.Session.GetString("TempUserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login");
            }

            return View(new VerifyTwoFactorViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyTwoFactor(VerifyTwoFactorViewModel model)
        {
            var userIdStr = HttpContext.Session.GetString("TempUserId");
            var deviceId = HttpContext.Session.GetString("TempDeviceId");
            var rememberDeviceStr = HttpContext.Session.GetString("TempRememberDevice");
            var rememberMeStr = HttpContext.Session.GetString("TempRememberMe");

            if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(deviceId))
            {
                return RedirectToAction("Login");
            }

            var userId = int.Parse(userIdStr);
            var rememberDevice = bool.Parse(rememberDeviceStr);
            var rememberMe = bool.Parse(rememberMeStr);
            var user = await _context.Employees.FindAsync(userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            // Validate 2FA code
            if (!_twoFactorService.ValidatePin(user.TwoFactorSecretKey, model.Code))
            {
                // Check recovery codes
                var recoveryCodes = user.TwoFactorRecoveryCodes?.Split(',') ?? Array.Empty<string>();
                if (!recoveryCodes.Contains(model.Code))
                {
                    TempData["ErrorMessage"] = "Invalid verification code.";
                    return View(model);
                }

                // Remove used recovery code
                var updatedCodes = recoveryCodes.Where(code => code != model.Code).ToList();
                user.TwoFactorRecoveryCodes = string.Join(",", updatedCodes);
                _context.Update(user);
                await _context.SaveChangesAsync();
            }

            // ✅ Use the RememberDevice from the form (model.RememberDevice) instead of session
            await CompleteLogin(user, deviceId, model.RememberDevice, rememberMe);

            // Clear session
            HttpContext.Session.Remove("TempUserId");
            HttpContext.Session.Remove("TempDeviceId");
            HttpContext.Session.Remove("TempRememberDevice");
            HttpContext.Session.Remove("TempRememberMe");

            TempData["SuccessMessage"] = "Two-factor authentication successful!";
            return RedirectToAction("ViewProfile", "Account");
        }

        [HttpGet]
        public IActionResult RequestUnlock()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestUnlock(string email)
        {
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.Email == email && u.IsLockedOut);

            if (user != null)
            {
                // Generate unlock token
                string unlockToken = Guid.NewGuid().ToString();
                user.ResetPin = BCrypt.Net.BCrypt.HashPassword(unlockToken);
                user.ResetPinExpiration = DateTime.Now.AddHours(1);

                await _context.SaveChangesAsync();

                // Send unlock email
                var unlockLink = Url.Action("UnlockWithToken", "Account",
                    new { email = email, token = unlockToken }, Request.Scheme);

                var placeholders = new Dictionary<string, string>
        {
            { "Name", user.FullName },
            { "UnlockLink", unlockLink }
        };

                await _emailService.SendEmailWithTemplateAsync(
                    email,
                    "Account Unlock Request - Future Med",
                    "account-unlock-template.html",
                    placeholders
                );
            }

            // Always show success message for security (don't reveal if account exists)
            TempData["SuccessMessage"] = "If your account is locked, you will receive an unlock instructions email shortly.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> UnlockWithToken(string email, string token)
        {
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null &&
                user.ResetPinExpiration > DateTime.Now &&
                BCrypt.Net.BCrypt.Verify(token, user.ResetPin))
            {
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                user.ResetPin = null;
                user.ResetPinExpiration = null;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Your account has been unlocked. You can now login.";
                return RedirectToAction("Login");
            }

            TempData["ErrorMessage"] = "Invalid or expired unlock link.";
            return RedirectToAction("Login");
        }


        [Authorize]
        [HttpGet]
        public IActionResult ChangeEmail()
        {
            return View(new ChangeEmailViewModel());
        }



        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var username = User.Identity.Name;
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
            {
                TempData["ErrorMessage"] = "Incorrect password.";
                return View(model);
            }

            // Prevent using the same email
            if (user.Email.Equals(model.NewEmail, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "New email cannot be the same as the current email.";
                return View(model);
            }

            // Prevent duplicates
            if (await _context.Employees.AnyAsync(e => e.Email == model.NewEmail))
            {
                TempData["ErrorMessage"] = "This email is already associated with another account.";
                return View(model);
            }

            // Generate verification token
            string token = Guid.NewGuid().ToString();
            user.EmailVerificationTokenHash = token;
            user.EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24);

            // Temporarily store the new email
            HttpContext.Session.SetString("PendingNewEmail", model.NewEmail);

            await _context.SaveChangesAsync();

            // Build verification link
            var verificationLink = Url.Action("ConfirmChangeEmail", "Account",
                new { userId = user.EmployeeID, token = token, newEmail = model.NewEmail },
                Request.Scheme);

            var placeholders = new Dictionary<string, string>
    {
        { "EmployeeName", $"{user.FirstName} {user.LastName}" },
        { "VerificationLink", verificationLink },
        { "NewEmail", model.NewEmail }
    };

            await _emailService.SendEmailWithTemplateAsync(
                model.NewEmail,
                "Confirm Your New Email - Future Med",
                "ConfirmChangeEmailTemplate.html", // create this template
                placeholders
            );

            TempData["SuccessMessage"] = "A confirmation link has been sent to your new email address. Please verify to complete the change.";
            return RedirectToAction("ViewProfile");
        }


        [HttpGet]
        public async Task<IActionResult> ConfirmChangeEmail(int userId, string token, string newEmail)
        {
            var user = await _context.Employees
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.EmployeeID == userId);

            if (user == null || string.IsNullOrEmpty(newEmail))
            {
                TempData["ErrorMessage"] = "Invalid request.";
                return RedirectToAction("Login");
            }

            if (user.EmailVerificationTokenHash != token || user.EmailVerificationTokenExpires < DateTime.UtcNow)
            {
                TempData["ErrorMessage"] = "Invalid or expired token.";
                return RedirectToAction("Login");
            }

            // Update email
            user.Email = newEmail;
            user.EmailVerificationTokenHash = null;
            user.EmailVerificationTokenExpires = null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your email has been updated successfully!";
            return RedirectToAction("ViewProfile");
        }





        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ManageTrustedDevices()
        {
            var username = User.Identity.Name;
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            var trustedDevices = await _twoFactorService.GetTrustedDevices(user.EmployeeID);
            var viewModel = trustedDevices.Select(td => new TrustedDeviceViewModel
            {
                Id = td.Id,
                DeviceName = td.DeviceName,
                CreatedDate = td.CreatedDate,
                LastUsed = td.LastUsed,
                ExpiryDate = td.ExpiryDate
            }).ToList();

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveTrustedDevice(int deviceId)
        {
            var username = User.Identity.Name;
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            await _twoFactorService.RemoveTrustedDevice(deviceId, user.EmployeeID);
            TempData["SuccessMessage"] = "Trusted device removed successfully.";
            return RedirectToAction("ManageTrustedDevices");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAllTrustedDevices()
        {
            var username = User.Identity.Name;
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            await _twoFactorService.RemoveAllTrustedDevices(user.EmployeeID);
            TempData["SuccessMessage"] = "All trusted devices have been removed.";
            return RedirectToAction("ManageTrustedDevices");
        }












        private async Task SendEmployeeDetailsEmail(Employee employee)
        {
            var placeholders = new Dictionary<string, string>
    {
        { "EmployeeName", $"{employee.FirstName} {employee.LastName}" },
        { "EmployeeID", employee.EmployeeID.ToString() },
        { "Email", employee.Email },
        { "Password", employee.PasswordHash },
        { "Role", employee.Role.ToString() },
        { "Gender", employee.Gender.ToString() },
        { "HireDate", employee.HireDate?.ToString("yyyy-MM-dd") ?? "Not specified" },
        { "VerificationDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm") },
        { "LoginUrl", Url.Action("Login", "Account", null, HttpContext.Request.Scheme) }
    };

            await _emailService.SendEmailWithTemplateAsync(
                employee.Email,
                "Your Account Details - Future Med",
                "EmployeeDetailsTemplates.html", // Create this template
                placeholders
            );
        }



        private async Task SendRecoveryCodesEmail(Employee user, IEnumerable<string> recoveryCodes)
        {
            var codesFormatted = string.Join("<br/>", recoveryCodes);

            var placeholders = new Dictionary<string, string>
    {
        { "EmployeeName", $"{user.FirstName} {user.LastName}" },
        { "RecoveryCodes", codesFormatted },
        { "SupportEmail", "support@futuremed.com" },
        { "LoginUrl", Url.Action("Login", "Account", null, HttpContext.Request.Scheme) }
    };

            await _emailService.SendEmailWithTemplateAsync(
                user.Email,
                "Your Two-Factor Authentication Recovery Codes",
                "TwoFactorRecoveryCodesTemplate.html", // you’ll create this template
                placeholders
            );
        }








        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "Account");

            return role switch
            {
                nameof(UserRole.ADMINISTRATOR) => RedirectToAction("AdminDashboard", "Admin"),
                nameof(UserRole.WARDADMIN) => RedirectToAction("WardAdminDashboard", "WardAdmin"),
                nameof(UserRole.NURSE) => RedirectToAction("NurseDashboard", "Nurse"),
                nameof(UserRole.NURSINGSISTER) => RedirectToAction("NursingSisterDashboard", "NursingSister"),
                nameof(UserRole.DOCTOR) => RedirectToAction("DoctorDashboard", "Doctor"),
                nameof(UserRole.SCRIPTMANAGER) => RedirectToAction("ScriptManagerDashboard", "ScriptManager"),
                nameof(UserRole.CONSUMABLESMANAGER) => RedirectToAction("ConsumablesManagerDashboard", "Consumables"),
                _ => RedirectToAction("Login", "Account")
            };
        }



        private async Task CompleteLogin(Employee user, string deviceId, bool rememberDevice, bool rememberMe)
        {
            HttpContext.Session.SetString("UserRole", user.Role.ToString());

            // Build claims
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Role, user.Role.ToString()),
        new Claim("EmployeeID", user.EmployeeID.ToString())



    }; 
            
            
            
            var themePreference = await _themeService.GetUserThemePreference(user.UserName);
            HttpContext.Session.SetString("ThemePreference", themePreference.ToString());





            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe
                    ? DateTime.UtcNow.AddDays(14)
                    : DateTime.UtcNow.AddMinutes(60)
            };

            // Audit log
            _context.LoginAudits.Add(new LoginAudit
            {
                Username = user.UserName,
                LoginTime = DateTime.Now,
                Success = true,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
            });

            await _context.SaveChangesAsync();

            // ✅ Handle device trust - only if 2FA is enabled
            if (rememberDevice && user.IsTwoFactorEnabled)
            {
                var deviceName = GetDeviceName(Request.Headers["User-Agent"].ToString());
                await _twoFactorService.AddTrustedDevice(user.EmployeeID, deviceId, deviceName);

                // Optional: Add success message
                TempData["InfoMessage"] = $"This device ({deviceName}) will be trusted for 30 days.";
            }
            else if (await _twoFactorService.IsDeviceTrusted(user.EmployeeID, deviceId))
            {
                // Update last used time for trusted device
                await _twoFactorService.UpdateDeviceUsage(user.EmployeeID, deviceId);
            }

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
        private string GetDeviceName(string userAgent)
        {
            if (userAgent.Contains("Windows"))
                return "Windows Device";
            if (userAgent.Contains("Mac"))
                return "Mac Device";
            if (userAgent.Contains("Linux"))
                return "Linux Device";
            if (userAgent.Contains("Android"))
                return "Android Device";
            if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
                return "iOS Device";

            return "Unknown Device";
        }


        // Theme Management Actions
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ThemeSettings()
        {
            var username = User.Identity.Name;
            var currentTheme = await _themeService.GetUserThemePreference(username);

            var model = new ThemeViewModel
            {
                SelectedTheme = currentTheme
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemeSettings(ThemeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var username = User.Identity.Name;
            await _themeService.SetUserThemePreference(username, model.SelectedTheme);

            // Update session
            HttpContext.Session.SetString("ThemePreference", model.SelectedTheme.ToString());

            TempData["SuccessMessage"] = "Theme preference updated successfully!";
            return RedirectToAction(nameof(ThemeSettings));
        }

        // Add this method to apply theme globally
        private void ApplyThemeToViewBag()
        {
            var theme = HttpContext.Session.GetString("ThemePreference");
            if (string.IsNullOrEmpty(theme) && User.Identity.IsAuthenticated)
            {
                // Fallback to system if not in session
                theme = ThemeType.System.ToString();
            }

            ViewBag.ThemePreference = theme ?? ThemeType.System.ToString();
            ViewBag.IsDarkMode = (theme == ThemeType.Dark.ToString() ||
                                 (theme == ThemeType.System.ToString() && IsSystemDarkMode()));
        }

        private bool IsSystemDarkMode()
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            // Simple detection - in production, use JavaScript or proper header checking
            return false; // Default to light
        }

        // Override OnActionExecuting to apply theme to all views
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            ApplyThemeToViewBag();
        }



        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveThemePreference([FromBody] ThemeRequest request)
        {
            if (!User.Identity.IsAuthenticated)
                return Json(new { success = false });

            if (!Enum.TryParse<ThemeType>(request.Theme, out var themeType))
                return Json(new { success = false });

            var username = User.Identity.Name;
            await _themeService.SetUserThemePreference(username, themeType);

            // Update session
            HttpContext.Session.SetString("ThemePreference", themeType.ToString());

            return Json(new { success = true });
        }

        public class ThemeRequest
        {
            public string Theme { get; set; }
        }
    }






}
