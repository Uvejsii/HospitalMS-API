using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace HospitalMS_API.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _userConnections = new();

        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            var email = user?.Identity?.Name;

            if (!string.IsNullOrEmpty(email))
            {
                _userConnections[email] = Context.ConnectionId;
            }

            if (user != null)
            {
                if (user.IsInRole("Admin"))
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");

                if (user.IsInRole("Doctor"))
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Doctors");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = Context.User;
            var email = user?.Identity?.Name;

            if (!string.IsNullOrEmpty(email))
            {
                _userConnections.TryRemove(email, out _);
            }

            if (user != null)
            {
                if (user.IsInRole("Admin"))
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");

                if (user.IsInRole("Doctor"))
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Doctors");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public static string? GetConnectionId(string email)
        {
            _userConnections.TryGetValue(email, out var connectionId);
            return connectionId;
        }
    }
}
