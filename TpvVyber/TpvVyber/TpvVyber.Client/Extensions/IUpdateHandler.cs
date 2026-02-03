using TpvVyber.Client.Classes;

namespace TpvVyber.Client.Extensions;

public interface IClientUpdateHandler
{
    void StateHasChanged();
    Task InvokeAsync(Func<Task> work);

    // CHANGED: Returns EventHandler to match your _updateHandler field
    public EventHandler<TpvUpdateEventArgs> FactoryUpdateHandler(
        List<string> scopes,
        Func<Task> updateCallback
    )
    {
        // CHANGED: Lambda signature matches (sender, e)
        return (sender, e) =>
        {
            if (e.Scopes is not null && e.Scopes.Any(s => scopes.Contains(s)))
            {
                // Fire and forget the async callback
                _ = InvokeAsync(async () =>
                {
                    await updateCallback();
                    StateHasChanged();
                });
            }
        };
    }

    // Overload for bool callback
    public EventHandler<TpvUpdateEventArgs> FactoryUpdateHandler(
        List<string> scopes,
        Func<bool, Task> updateCallback
    )
    {
        return (sender, e) =>
        {
            if (e.Scopes is not null && e.Scopes.Any(s => scopes.Contains(s)))
            {
                _ = InvokeAsync(async () =>
                {
                    await updateCallback(true);
                    StateHasChanged();
                });
            }
        };
    }
}
