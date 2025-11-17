using Microsoft.EntityFrameworkCore;
using TpvVyber.Data;

namespace TpvVyber.Services;

public static class Database
{
    public static void AddDatabaseService(this WebApplicationBuilder builder)
    {
        var ysqlPasswordPath = builder.Configuration["YSQL_PASSWORD"];
        if (string.IsNullOrEmpty(ysqlPasswordPath))
        {
            throw new Exception("YSQL_PASSWORD is not set");
        }
        var ysqlPassword = System.IO.File.ReadAllText(ysqlPasswordPath).Trim();
        if (string.IsNullOrEmpty(ysqlPassword))
        {
            throw new Exception("YSQL_PASSWORD is empty");
        }

        var connectionString = builder
            .Configuration.GetConnectionString("TpvVyberDb")
            .Replace("<YSQL_PASSWORD>", ysqlPassword);

        builder.Services.AddDbContext<TpvVyberContext>(options =>
            options.UseNpgsql(
                connectionString,
                o =>
                {
                    // Explicitly place the history table in your schema
                    o.MigrationsHistoryTable("__EFMigrationsHistory", "tpv_schema");
                }
            )
        );
    }
}
