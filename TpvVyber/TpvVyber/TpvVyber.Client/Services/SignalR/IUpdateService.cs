using Microsoft.JSInterop;

namespace TpvVyber.Client.Classes;

public interface IUpdateService
{
    public void RegisterFunction(EventHandler<TpvUpdateEventArgs> handler);
    public void UnregisterFunction(EventHandler<TpvUpdateEventArgs> handler);
    public Task InitializeAsync();
    public Task UpdateAsync(string scopes);
}

public class TpvUpdateEventArgs : EventArgs
{
    public List<string> Scopes { get; set; } = [];
}
