using HospitalManagement.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HospitalManagement.Services
{
    public interface INotificationHubService
    {
        Task SendNotificationToUser(int employeeId, object notification);
        Task SendNotificationToAll(object notification);
        Task UpdateNotificationCount(int employeeId, int unreadCount);
    }

    public class NotificationHubService : INotificationHubService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationHubService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationToUser(int employeeId, object notification)
        {
            await _hubContext.Clients.Group($"user-{employeeId}")
                .SendAsync("ReceiveNotification", notification);
        }

        public async Task SendNotificationToAll(object notification)
        {
            await _hubContext.Clients.All
                .SendAsync("ReceiveNotification", notification);
        }

        public async Task UpdateNotificationCount(int employeeId, int unreadCount)
        {
            await _hubContext.Clients.Group($"user-{employeeId}")
                .SendAsync("UpdateNotificationCount", unreadCount);
        }
    }
}
