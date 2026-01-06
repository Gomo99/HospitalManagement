using HospitalManagement.AppStatus;
using HospitalManagement.Data;
using HospitalManagement.Models;
using HospitalManagement.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Services
{
    public class MessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public MessageService(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<List<MessageViewModel>> GetInboxAsync(int employeeId)
        {
            return await _context.Messages
                .Where(m => m.ReceiverId == employeeId && !m.IsDeletedByReceiver)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentDate)
                .Select(m => new MessageViewModel
                {
                    MessageId = m.MessageId,
                    Subject = m.Subject,
                    Content = m.Content.Length > 100 ? m.Content.Substring(0, 100) + "..." : m.Content,
                    SenderName = m.Sender.FirstName + " " + m.Sender.LastName,
                    ReceiverName = m.Receiver.FirstName + " " + m.Receiver.LastName,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    SentDate = m.SentDate,
                    ReadDate = m.ReadDate,
                    IsRead = m.ReadDate.HasValue,
                    Priority = m.Priority, // This will now use your custom enum
                    IsCurrentUser = m.SenderId == employeeId,
                    IsCurrentUserSender = m.SenderId == employeeId
                })
                .ToListAsync();
        }

        public async Task<List<MessageViewModel>> GetSentMessagesAsync(int employeeId)
        {
            return await _context.Messages
                .Where(m => m.SenderId == employeeId && !m.IsDeletedBySender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.SentDate)
                .Select(m => new MessageViewModel
                {
                    MessageId = m.MessageId,
                    Subject = m.Subject,
                    Content = m.Content.Length > 100 ? m.Content.Substring(0, 100) + "..." : m.Content,
                    SenderName = m.Sender.FirstName + " " + m.Sender.LastName,
                    ReceiverName = m.Receiver.FirstName + " " + m.Receiver.LastName,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    SentDate = m.SentDate,
                    ReadDate = m.ReadDate,
                    IsRead = m.ReadDate.HasValue,
                    Priority = m.Priority,
                    IsCurrentUser = m.SenderId == employeeId,
                    IsCurrentUserSender = m.SenderId == employeeId// This will now use your custom enum
                })
                .ToListAsync();
        }

        public async Task<MessageViewModel> GetMessageAsync(int messageId, int employeeId)
        {
            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync(m => m.MessageId == messageId &&
                                        (m.SenderId == employeeId || m.ReceiverId == employeeId));

            if (message == null) return null;

            // Mark as read if receiver is viewing
            if (message.ReceiverId == employeeId && !message.ReadDate.HasValue)
            {
                message.ReadDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return new MessageViewModel
            {
                MessageId = message.MessageId,
                Subject = message.Subject,
                Content = message.Content,
                SenderName = message.Sender.FirstName + " " + message.Sender.LastName,
                ReceiverName = message.Receiver.FirstName + " " + message.Receiver.LastName,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                SentDate = message.SentDate,
                ReadDate = message.ReadDate,
                IsRead = message.ReadDate.HasValue,
                Priority = message.Priority,
                IsCurrentUser = message.SenderId == employeeId,
                IsCurrentUserSender = message.SenderId == employeeId// This will now use your custom enum

            };
        }

        // In MessageService.cs - Update SendMessageAsync method
        public async Task<bool> SendMessageAsync(SendMessageViewModel model, int senderId, int? patientId = null)
        {
            try
            {
                var message = new Message
                {
                    SenderId = senderId,
                    ReceiverId = model.ReceiverId,
                    Subject = model.Subject,
                    Content = model.Content,
                    Priority = model.Priority,
                    SentDate = DateTime.Now
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                // Create notification for the receiver with patient info if available
                await _notificationService.CreateMessageNotification(
                    message.MessageId,
                    senderId,
                    model.ReceiverId,
                    model.Subject,
                    model.Content,
                    patientId // Pass patient ID if available
                );

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                return false;
            }
        }



        public async Task<int> GetUnreadCountAsync(int employeeId)
        {
            return await _context.Messages
                .CountAsync(m => m.ReceiverId == employeeId &&
                               !m.ReadDate.HasValue &&
                               !m.IsDeletedByReceiver);
        }

        public async Task<bool> DeleteMessageAsync(int messageId, int employeeId)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.MessageId == messageId &&
                                        (m.SenderId == employeeId || m.ReceiverId == employeeId));

            if (message == null) return false;

            if (message.SenderId == employeeId)
                message.IsDeletedBySender = true;
            else
                message.IsDeletedByReceiver = true;

            // If both sender and receiver have deleted, remove from database
            if (message.IsDeletedBySender && message.IsDeletedByReceiver)
                _context.Messages.Remove(message);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SelectListItem>> GetEmployeeListAsync(int excludeEmployeeId)
        {
            return await _context.Employees
                .Where(e => e.EmployeeID != excludeEmployeeId && e.IsActive == Status.Active)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Select(e => new SelectListItem
                {
                    Value = e.EmployeeID.ToString(),
                    Text = $"{e.FirstName} {e.LastName} ({e.Role})"
                })
                .ToListAsync();
        }


        public async Task<List<MessageViewModel>> GetConversationThreadAsync(int currentUserId, int otherUserId)
        {
            return await _context.Messages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == otherUserId && !m.IsDeletedBySender) ||
                           (m.SenderId == otherUserId && m.ReceiverId == currentUserId && !m.IsDeletedByReceiver))
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderBy(m => m.SentDate)
                .Select(m => new MessageViewModel
                {
                    MessageId = m.MessageId,
                    Subject = m.Subject,
                    Content = m.Content,
                    SenderName = m.Sender.FirstName + " " + m.Sender.LastName,
                    ReceiverName = m.Receiver.FirstName + " " + m.Receiver.LastName,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    SentDate = m.SentDate,
                    ReadDate = m.ReadDate,
                    IsRead = m.ReadDate.HasValue,
                    Priority = m.Priority,
                    // Set the new properties
                    IsCurrentUser = m.SenderId == currentUserId,
                    IsCurrentUserSender = m.SenderId == currentUserId
                })
                .ToListAsync();
        }
    }
}
