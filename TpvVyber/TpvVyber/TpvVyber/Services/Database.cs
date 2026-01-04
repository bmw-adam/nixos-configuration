using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using TpvVyber.Data;
using TpvVyber.Hubs;

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
            ?.Replace("<YSQL_PASSWORD>", ysqlPassword);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new NullReferenceException("Connection String Was Null");
        }

        builder.Services.AddSingleton<DatabaseUpdateInterceptor>();

        builder.Services.AddDbContextFactory<TpvVyberContext>(
            (sp, options) =>
            {
                var interceptor = sp.GetRequiredService<DatabaseUpdateInterceptor>();

                options
                    .UseNpgsql(
                        connectionString,
                        o =>
                        {
                            o.MigrationsHistoryTable("__EFMigrationsHistory", "tpv_schema");
                        }
                    )
                    .AddInterceptors(interceptor)
                    .ReplaceService<IHistoryRepository, YugabyteHistoryRepository>();
            }
        );
    }
}
