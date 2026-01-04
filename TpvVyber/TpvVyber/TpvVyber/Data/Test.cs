using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;
using System;
using System.Threading.Tasks;

// Suppress warning about using internal EF Core APIs
#pragma warning disable EF1001

namespace TpvVyber.Data;

public class YugabyteHistoryRepository : NpgsqlHistoryRepository
{
    public YugabyteHistoryRepository(HistoryRepositoryDependencies dependencies) 
        : base(dependencies)
    {
    }

    // UPDATED: Now returns IMigrationsDatabaseLock instead of void
    public override IMigrationsDatabaseLock AcquireDatabaseLock()
    {
        // Return a "fake" lock that does nothing
        return new NoOpMigrationsDatabaseLock();
    }

    // Internal class to satisfy the interface requirement
    private class NoOpMigrationsDatabaseLock : IMigrationsDatabaseLock
    {
        // The interface requires a HistoryRepository property
        public IHistoryRepository HistoryRepository => null!;

        // Dispose is called when the lock should be released. 
        // We do nothing because we never took a lock.
        public void Dispose()
        {
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
