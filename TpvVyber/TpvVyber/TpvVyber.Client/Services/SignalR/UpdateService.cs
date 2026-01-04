using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace TpvVyber.Client.Classes;

public class ClientUpdateService : IUpdateService, IAsyncDisposable
{
    public event EventHandler<EventArgs>? UpdateReceived;
    private readonly NavigationManager _navigationManager;
    private HubConnection? _hub;

    public ClientUpdateService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public void RegisterFunction(EventHandler<EventArgs> handler)
    {
        if (handler is null)
            return;
        UpdateReceived += handler;
    }

    public void UnregisterFunction(EventHandler<EventArgs> handler)
    {
        if (handler is null)
            return;
        UpdateReceived -= handler;
    }

    public async Task InitializeAsync()
    {
        if (_hub is not null)
            return;

        _hub = new HubConnectionBuilder()
            .WithUrl(_navigationManager.ToAbsoluteUri("/update"))
            .WithAutomaticReconnect()
            .Build();

        _hub.On(
            "ReceiveUpdate",
            () =>
            {
                UpdateReceived?.Invoke(this, EventArgs.Empty);
            }
        );

        await _hub.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub is not null)
        {
            await _hub.DisposeAsync();
            _hub = null;
        }
    }

    public async Task UpdateAsync()
    {
        if (_hub is not null)
        {
            await _hub.SendAsync("SendUpdate");
        }
    }
}
