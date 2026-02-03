using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TpvVyber.Hubs;
using TpvVyber.Services;

namespace TpvVyber.Data;

public class DatabaseUpdateInterceptor : SaveChangesInterceptor
{
    private readonly IHubContext<UpdateHub> _hubContext;
    private readonly ServerUpdateService _serverUpdateService;

    // Thread-safe storage to map a specific DbContext instance to its modified tables.
    // This ensures safety even if the Interceptor is registered as a Singleton.
    private readonly ConditionalWeakTable<DbContext, List<string>> _affectedTables = new();

    public DatabaseUpdateInterceptor(
        IHubContext<UpdateHub> hubContext,
        ServerUpdateService serverUpdateService
    )
    {
        _hubContext = hubContext;
        _serverUpdateService = serverUpdateService;
    }

    // Capture the modified tables BEFORE the save occurs (so we can see Deleted entities)
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context != null)
        {
            // Identify all tables that have Added, Modified, or Deleted entities
            var tableNames = eventData
                .Context.ChangeTracker.Entries()
                .Where(e =>
                    e.State == EntityState.Added
                    || e.State == EntityState.Modified
                    || e.State == EntityState.Deleted
                )
                .Select(e => e.Metadata.GetTableName())
                .Where(t => t != null)
                .Distinct()
                .Cast<string>()
                .ToList();

            if (tableNames.Any())
            {
                _affectedTables.AddOrUpdate(eventData.Context, tableNames);
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default
    )
    {
        if (
            result > 0
            && eventData.Context != null
            && _affectedTables.TryGetValue(eventData.Context, out var tableNames)
        )
        {
            System.Console.WriteLine(
                "Changes detected in tables: " + string.Join(", ", tableNames)
            );
            await _hubContext.Clients.All.SendAsync(
                "ReceiveUpdate",
                string.Join(";", tableNames.Distinct()),
                cancellationToken
            );

            await _serverUpdateService.ReceiveUpdate(string.Join(";", tableNames.Distinct()));

            // Cleanup memory for this context
            _affectedTables.Remove(eventData.Context);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}
