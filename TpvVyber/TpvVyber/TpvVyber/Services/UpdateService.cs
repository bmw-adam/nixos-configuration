using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TpvVyber.Client.Classes;

namespace TpvVyber.Services;

public class ServerUpdateService() : IUpdateService, IDisposable
{
    public event EventHandler<TpvUpdateEventArgs>? UpdateReceived;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public Task InitializeAsync() => Task.CompletedTask;

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

    public Task ReceiveUpdate(string scopes)
    {
        // System.Console.WriteLine("Test");
        UpdateReceived?.Invoke(
            this,
            new TpvUpdateEventArgs { Scopes = scopes.Split(';').ToList() }
        );
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(string scopes)
    {
        await ReceiveUpdate(scopes);
    }
}
