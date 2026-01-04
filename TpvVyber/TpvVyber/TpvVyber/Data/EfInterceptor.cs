using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TpvVyber.Hubs;
using TpvVyber.Services;

namespace TpvVyber.Data;

public class DatabaseUpdateInterceptor : SaveChangesInterceptor
{
    private readonly IHubContext<UpdateHub> _hubContext;
    private readonly ServerUpdateService _serverUpdateService;

    public DatabaseUpdateInterceptor(
        IHubContext<UpdateHub> hubContext,
        ServerUpdateService serverUpdateService
    )
    {
        _hubContext = hubContext;
        _serverUpdateService = serverUpdateService;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default
    )
    {
        if (result > 0)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveUpdate");
            await _serverUpdateService.ReceiveUpdate();
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}
