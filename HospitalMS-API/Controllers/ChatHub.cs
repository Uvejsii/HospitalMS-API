using Microsoft.AspNetCore.SignalR;

namespace HospitalMS_API.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string senderId, string receiverId, string message)
        {
            await Clients.Users(senderId, receiverId).SendAsync("ReceiveMessage", senderId, receiverId, message);
        }
    }
}
