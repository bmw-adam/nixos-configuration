using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TpvVyber.Client.Classes;

namespace TpvVyber.Services;

public class ServerUpdateService() : IUpdateService, IDisposable
{
    public event EventHandler<EventArgs>? UpdateReceived;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public Task InitializeAsync() => Task.CompletedTask;

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

    public Task ReceiveUpdate()
    {
        // System.Console.WriteLine("Test");
        UpdateReceived?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public async Task UpdateAsync()
    {
        await ReceiveUpdate();
    }
}
