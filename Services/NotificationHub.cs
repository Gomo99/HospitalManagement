using HospitalManagement.AppStatus;
using HospitalManagement.Data;
using HospitalManagement.Models;
using HospitalManagement.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationHubService _hubService;

        public NotificationService(ApplicationDbContext context, INotificationHubService hubService)
        {
            _context = context;
            _hubService = hubService;
        }


        public async Task CreatePatientAssignmentNotification(int patientId, int admissionId, int doctorId, int nurseId, int senderId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null) return;

            var admission = await _context.Admissions.FindAsync(admissionId);
            if (admission == null) return;

            var sender = await _context.Employees.FindAsync(senderId);
            var senderName = sender != null ? $"{sender.FirstName} {sender.LastName}" : "System";

            var notifications = new List<Notification>();

            // Notification for Doctor
            if (doctorId > 0)
            {
                var doctorNotification = new Notification
                {
                    Title = "New Patient Assignment",
                    Message = $"Patient {patient.FirstName} {patient.LastName} has been assigned to you by {senderName}.",
                    SenderId = senderId,
                    ReceiverId = doctorId,
                    Type = NotificationType.PatientAssignment,
                    Priority = NotificationPriority.High,
                    AdmissionId = admissionId,
                    PatientId = patientId,
                    ActionUrl = GenerateActionUrl(new Notification
                    {
                        Type = NotificationType.PatientAssignment,
                        PatientId = patientId,
                        AdmissionId = admissionId
                    })
                };

                notifications.Add(doctorNotification);
            }

            // Notification for Nurse
            if (nurseId > 0)
            {
                var nurseNotification = new Notification
                {
                    Title = "New Patient Assignment",
                    Message = $"Patient {patient.FirstName} {patient.LastName} has been assigned to you by {senderName}. " +
                             $"Admission Date: {admission.AdmissionDate.ToString("MMM dd, yyyy")}. " +
                             $"Please assist with patient care and monitoring.",
                    SenderId = senderId,
                    ReceiverId = nurseId,
                    Type = NotificationType.PatientAssignment,
                    Priority = NotificationPriority.High,
                    AdmissionId = admissionId,
                    PatientId = patientId,
                    ActionUrl = GenerateActionUrl(new Notification
                    {
                        Type = NotificationType.PatientAssignment,
                        PatientId = patientId,
                        AdmissionId = admissionId
                    })
                };

                notifications.Add(nurseNotification);
            }

            if (notifications.Any())
            {
                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();

                // Send real-time notifications after saving
                foreach (var notification in notifications)
                {
                    await SendRealTimeNotification(notification.ReceiverId, notification.NotificationId);
                }

                Console.WriteLine($"Created {notifications.Count} patient assignment notifications");
            }
        }



        private async Task SendRealTimeNotification(int receiverId, int notificationId)
        {
            var notification = await GetNotificationViewModel(notificationId);
            if (notification != null)
            {
                await _hubService.SendNotificationToUser(receiverId, notification);

                // Update notification count
                var unreadCount = await GetUnreadCountAsync(receiverId);
                await _hubService.UpdateNotificationCount(receiverId, unreadCount);
            }
        }

        private async Task<NotificationViewModel> GetNotificationViewModel(int notificationId)
        {
            return await _context.Notifications
                .Where(n => n.NotificationId == notificationId)
                .Include(n => n.Sender)
                .Include(n => n.Patient)
                .Select(n => new NotificationViewModel
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Message = n.Message,
                    SenderName = n.Sender != null ? $"{n.Sender.FirstName} {n.Sender.LastName}" : "System",
                    CreatedDate = n.CreatedDate,
                    IsRead = n.ReadDate.HasValue,
                    Type = n.Type,
                    Priority = n.Priority,
                    PatientName = n.Patient != null ? $"{n.Patient.FirstName} {n.Patient.LastName}" : null,
                    ActionUrl = NotificationService.GenerateActionUrl(n) // Call static method
                })
                .FirstOrDefaultAsync();
        }


















        // In NotificationService.cs - Update the CreateMessageNotification method
        public async Task CreateMessageNotification(int messageId, int senderId, int receiverId, string messageSubject, string messageContent = null, int? patientId = null)
        {
            var sender = await _context.Employees.FindAsync(senderId);
            var receiver = await _context.Employees.FindAsync(receiverId);

            if (sender == null || receiver == null) return;

            // Get the actual message content if not provided
            if (string.IsNullOrEmpty(messageContent))
            {
                var message = await _context.Messages.FindAsync(messageId);
                messageContent = message?.Content ?? "No content available";
            }

            // Get patient information if patientId is provided
            string patientInfo = "";
            if (patientId.HasValue)
            {
                var patient = await _context.Patients.FindAsync(patientId.Value);
                if (patient != null)
                {
                    patientInfo = $"\n\nRelated to Patient: {patient.FirstName} {patient.LastName}";
                }
            }

            var notification = new Notification
            {
                Title = "New Message Received",
                Message = $"You have received a new message from {sender.FirstName} {sender.LastName}.\n\nSubject: {messageSubject}\n\nMessage: {messageContent}{patientInfo}",
                SenderId = senderId,
                ReceiverId = receiverId,
                Type = NotificationType.MessageReceived,
                Priority = NotificationPriority.Normal,
                MessageId = messageId,
                PatientId = patientId, // Store patient ID if available
                ActionUrl = GenerateActionUrl(new Notification
                {
                    Type = NotificationType.MessageReceived,
                    MessageId = messageId
                }),
                CreatedDate = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }





        // Add a method to create different types of patient notifications
        public async Task CreatePatientAdmissionNotification(int patientId, int admissionId, int senderId, string additionalInfo = "")
        {
            var patient = await _context.Patients.FindAsync(patientId);
            var admission = await _context.Admissions.FindAsync(admissionId);
            var sender = await _context.Employees.FindAsync(senderId);

            if (patient == null || admission == null) return;

            var senderName = sender != null ? $"{sender.FirstName} {sender.LastName}" : "System";

            var notification = new Notification
            {
                Title = "Patient Admission",
                Message = $"Patient {patient.FirstName} {patient.LastName} has been admitted on {admission.AdmissionDate.ToString("MMM dd, yyyy")}. {additionalInfo}",
                SenderId = senderId,
                ReceiverId = admission.EmployeeID, // Notify the assigned doctor
                Type = NotificationType.AdmissionUpdate,
                Priority = NotificationPriority.Normal,
                AdmissionId = admissionId,
                PatientId = patientId,
                ActionUrl = GenerateActionUrl(new Notification
                {
                    Type = NotificationType.AdmissionUpdate,
                    AdmissionId = admissionId
                })
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }



        // In NotificationService.cs - Update GetUserNotificationsAsync method
        public async Task<List<NotificationViewModel>> GetUserNotificationsAsync(int employeeId, int count = 10)
        {
            var notifications = await _context.Notifications
                .Where(n => n.ReceiverId == employeeId && n.IsActive)
                .Include(n => n.Sender)
                .Include(n => n.Patient) // Make sure to include Patient
                .Include(n => n.Admission)
                .OrderByDescending(n => n.CreatedDate)
                .Take(count)
                .ToListAsync();

            return notifications.Select(n => new NotificationViewModel
            {
                NotificationId = n.NotificationId,
                Title = n.Title,
                Message = n.Message,
                SenderName = n.Sender != null ? $"{n.Sender.FirstName} {n.Sender.LastName}" : "System",
                CreatedDate = n.CreatedDate,
                IsRead = n.ReadDate.HasValue,
                ReadDate = n.ReadDate,
                Type = n.Type,
                Priority = n.Priority,
                AdmissionId = n.AdmissionId,
                PatientId = n.PatientId,
                PatientName = n.Patient != null ? $"{n.Patient.FirstName} {n.Patient.LastName}" : null,
                MessageId = n.MessageId,
                ActionUrl = GenerateActionUrl(n)
            }).ToList();
        }





        public async Task<int> GetUnreadCountAsync(int employeeId)
        {
            return await _context.Notifications
                .CountAsync(n => n.ReceiverId == employeeId &&
                               n.ReadDate == null &&
                               n.IsActive);
        }

        public async Task MarkAsReadAsync(int notificationId, int employeeId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.ReceiverId == employeeId);

            if (notification != null && !notification.IsRead)
            {
                notification.ReadDate = DateTime.Now;
                await _context.SaveChangesAsync();

                // Update notification count in real-time
                var unreadCount = await GetUnreadCountAsync(employeeId);
                await _hubService.UpdateNotificationCount(employeeId, unreadCount);
            }
        }

        public async Task MarkAllAsReadAsync(int employeeId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.ReceiverId == employeeId && n.ReadDate == null && n.IsActive)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.ReadDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task CreateAdmissionUpdateNotification(int admissionId, string message, int senderId, NotificationPriority priority = NotificationPriority.Normal)
        {
            var admission = await _context.Admissions
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AdmissionId == admissionId);

            if (admission == null) return;

            var notifications = new List<Notification>();

            if (admission.EmployeeID > 0)
            {
                notifications.Add(new Notification
                {
                    Title = "Admission Update",
                    Message = message,
                    SenderId = senderId,
                    ReceiverId = admission.EmployeeID,
                    Type = NotificationType.AdmissionUpdate,
                    Priority = priority,
                    AdmissionId = admissionId,
                    PatientId = admission.PatientId,
                    ActionUrl = GenerateActionUrl(new Notification
                    {
                        Type = NotificationType.AdmissionUpdate,
                        AdmissionId = admissionId
                    })
                });
            }

            if (admission.NurseID > 0)
            {
                notifications.Add(new Notification
                {
                    Title = "Admission Update",
                    Message = message,
                    SenderId = senderId,
                    ReceiverId = admission.NurseID,
                    Type = NotificationType.AdmissionUpdate,
                    Priority = priority,
                    AdmissionId = admissionId,
                    PatientId = admission.PatientId,
                    ActionUrl = GenerateActionUrl(new Notification
                    {
                        Type = NotificationType.AdmissionUpdate,
                        AdmissionId = admissionId
                    })
                });
            }

            if (notifications.Any())
            {
                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();
            }
        }

        public async Task CreatePatientDischargeNotification(int admissionId, int senderId)
        {
            var admission = await _context.Admissions
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AdmissionId == admissionId);

            if (admission == null) return;

            var message = $"Patient {admission.Patient.FirstName} {admission.Patient.LastName} has been discharged.";

            await CreateAdmissionUpdateNotification(admissionId, message, senderId, NotificationPriority.Normal);
        }

        public async Task CreateMessageNotification(int messageId, int senderId, int receiverId, string messageSubject)
        {
            var sender = await _context.Employees.FindAsync(senderId);
            var receiver = await _context.Employees.FindAsync(receiverId);

            if (sender == null || receiver == null) return;

            var notification = new Notification
            {
                Title = "New Message Received",
                Message = $"You have a new message from {sender.FirstName} {sender.LastName}: {messageSubject}",
                SenderId = senderId,
                ReceiverId = receiverId,
                Type = NotificationType.MessageReceived,
                Priority = NotificationPriority.Normal,
                MessageId = messageId,
                ActionUrl = GenerateActionUrl(new Notification
                {
                    Type = NotificationType.MessageReceived,
                    MessageId = messageId
                }),
                CreatedDate = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public static string GenerateActionUrl(Notification notification)
        {
            return notification.Type switch
            {
                NotificationType.MessageReceived =>
                    notification.MessageId.HasValue
                        ? $"/Message/ViewMessage/{notification.MessageId}"
                        : "/Message/Inbox",

                NotificationType.PatientAssignment =>
                    notification.PatientId.HasValue
                        ? $"/Patient/Details/{notification.PatientId}"
                        : "/Patient/MyPatients",

                NotificationType.AdmissionUpdate =>
                    notification.AdmissionId.HasValue
                        ? $"/Admission/Details/{notification.AdmissionId}"
                        : "/Admission",

                NotificationType.PatientDischarge =>
                    notification.PatientId.HasValue
                        ? $"/Patient/Details/{notification.PatientId}"
                        : "/Patient/Discharged",

                NotificationType.Emergency =>
                    notification.AdmissionId.HasValue
                        ? $"/Admission/Emergency/{notification.AdmissionId}"
                        : "/Admission/Emergency",

                NotificationType.System => "/Notification/Index",

                _ => "/Notification/Index"
            };
        }











        public async Task<Notification> GetNotificationByIdAsync(int id, int employeeId)
        {
            return await _context.Notifications
                .Include(n => n.Sender)
                .Include(n => n.Patient)
                .Include(n => n.Admission)
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.ReceiverId == employeeId && n.IsActive);
        }



        // Add these methods to NotificationService class
        public async Task CreateNotificationAsync(int receiverId, int senderId, string title, string message, NotificationPriority priority)
        {
            var notification = new Notification
            {
                Title = title,
                Message = message,
                SenderId = senderId,
                ReceiverId = receiverId,
                Type = NotificationType.System,
                Priority = priority,
                CreatedDate = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CreateDoctorInstructionsNotification(int patientId, int instructionId, int doctorId, int senderId, string title)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null) return;

            var notification = new Notification
            {
                Title = title,
                Message = $"New instructions from Nurse for patient {patient.FullName}",
                SenderId = senderId,
                ReceiverId = doctorId,
                Type = NotificationType.System,
                Priority = NotificationPriority.Normal,
                PatientId = patientId,
                CreatedDate = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

    }




}