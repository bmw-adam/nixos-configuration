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

            // await using var db = factory.CreateDbContext();
            var maxWaitTime = TimeSpan.FromMinutes(5); // maximum total wait
            var delay = TimeSpan.FromSeconds(15); // wait between retries
            var startTime = DateTime.UtcNow;
            var migrationApplied = false;

            var logger = app.Services.GetRequiredService<ILogger<Program>>();

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
                    logger.LogError(
                        $"Database not ready yet: {ex.Message}. Retrying in {delay.TotalSeconds}s..."
                    );
                    Thread.Sleep(delay);
                }
            }

            Thread.Sleep(delay);

            try
            {
                var loggingEnd = db.LoggingEndings.FirstOrDefault();

                if (loggingEnd == null)
                {
                    db.LoggingEndings.Add(
                        new Classes.LoggingEnding
                        {
                            TimeEnding = DateTime.Now.AddDays(2).ToUniversalTime(),
                        }
                    );
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    $"Nepodařilo se nastavit konec přihlašování v databázi: {ex.Message}"
                );
            }

            if (!migrationApplied)
            {
                throw new Exception(
                    "Failed to connect to the database and apply migrations within the timeout."
                );
            }
            else
            {
                logger.LogInformation("Database migrations applied successfully.");
            }
        }
    }
}
