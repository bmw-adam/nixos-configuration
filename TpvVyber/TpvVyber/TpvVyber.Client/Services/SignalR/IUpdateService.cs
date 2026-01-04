using Microsoft.JSInterop;

namespace TpvVyber.Client.Classes;

public interface IUpdateService
{
    public void RegisterFunction(EventHandler<EventArgs> handler);
    public void UnregisterFunction(EventHandler<EventArgs> handler);
    public Task InitializeAsync();
    public Task UpdateAsync();
}
