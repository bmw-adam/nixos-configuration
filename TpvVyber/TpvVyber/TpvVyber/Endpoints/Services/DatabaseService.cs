using Microsoft.EntityFrameworkCore;
using TpvVyber.Data;

namespace TpvVyber.Services;

public static class DatabaseService
{
    public static async Task UseDatabaseService(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TpvVyberContext>();
            var maxWaitTime = TimeSpan.FromMinutes(5); // maximum total wait
            var delay = TimeSpan.FromSeconds(15); // wait between retries
            var startTime = DateTime.UtcNow;
            var migrationApplied = false;

            while (!migrationApplied && DateTime.UtcNow - startTime < maxWaitTime)
            {
                try
                {
                    db.Database.EnsureCreated();
                    await db.SaveChangesAsync();
                    db.Database.Migrate();
                    await db.SaveChangesAsync();

                    migrationApplied = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Database not ready yet: {ex.Message}. Retrying in {delay.TotalSeconds}s..."
                    );
                    Thread.Sleep(delay);
                }
            }

            if (!migrationApplied)
            {
                throw new Exception(
                    "Failed to connect to the database and apply migrations within the timeout."
                );
            }
            else
            {
                Console.WriteLine("Database migrations applied successfully.");
            }
        }
    }
}
