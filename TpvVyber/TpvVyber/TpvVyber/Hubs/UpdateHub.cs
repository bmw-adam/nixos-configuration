using Microsoft.AspNetCore.SignalR;

namespace TpvVyber.Hubs;

public class UpdateHub : Hub
{
    public async Task SendUpdate()
    {
        // Broadcast to everyone
        await Clients.All.SendAsync("ReceiveUpdate");
    }
}
