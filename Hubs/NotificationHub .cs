using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace HospitalManagement.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinNotificationGroup()
        {
            // Users join their personal notification group based on employee ID
            var employeeId = Context.User?.FindFirst("EmployeeID")?.Value;
            if (!string.IsNullOrEmpty(employeeId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{employeeId}");
            }
        }

        public async Task LeaveNotificationGroup()
        {
            var employeeId = Context.User?.FindFirst("EmployeeID")?.Value;
            if (!string.IsNullOrEmpty(employeeId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{employeeId}");
            }
        }

        public override async Task OnConnectedAsync()
        {
            await JoinNotificationGroup();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await LeaveNotificationGroup();
            await base.OnDisconnectedAsync(exception);
        }
    }
}
