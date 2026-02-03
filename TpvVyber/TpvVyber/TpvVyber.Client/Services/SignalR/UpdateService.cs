using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace TpvVyber.Client.Classes;

public class ClientUpdateService : IUpdateService, IAsyncDisposable
{
    public event EventHandler<TpvUpdateEventArgs>? UpdateReceived;
    private readonly NavigationManager _navigationManager;
    private HubConnection? _hub;

    public ClientUpdateService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public void RegisterFunction(EventHandler<TpvUpdateEventArgs> handler)
    {
        if (handler is null)
            return;
        UpdateReceived += handler;
    }

    public void UnregisterFunction(EventHandler<TpvUpdateEventArgs> handler)
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

        _hub.On<string>(
            "ReceiveUpdate",
            (string scopes) =>
            {
                UpdateReceived?.Invoke(
                    this,
                    new TpvUpdateEventArgs { Scopes = scopes.Split(';').ToList() }
                );
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

    public async Task UpdateAsync(string scopes)
    {
        if (_hub is not null)
        {
            await _hub.SendAsync("SendUpdate", scopes);
        }
    }
}
