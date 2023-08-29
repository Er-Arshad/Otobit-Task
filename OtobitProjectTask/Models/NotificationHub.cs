using Microsoft.AspNetCore.SignalR;

namespace OtobitProjectTask.Models
{
    public class NotificationHub : Hub
    {
        public string Id = "";
      
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            Id = connectionId;
            // Store the connection ID as needed (e.g., in-memory cache or a database).
            await base.OnConnectedAsync();
        }
        public async Task SendMessageToClient(string connectionId, string message)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        public string GetId()
        {
            return Id;
        }
    }
}
