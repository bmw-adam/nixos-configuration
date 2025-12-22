using System;
using Bunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using Serilog;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Client.Services.Select;
using TpvVyber.Data;
using TpvVyber.Endpoints.Admin;
using TpvVyber.Endpoints.Select;
using TpvVyber.Tests.Constants;

namespace TpvVyber.Tests;

public abstract class BunitbaseClass : BunitContext
{
    public HttpClient HttpClient { get; set; } = null!;
    protected SqliteConnection _connection = null!;
    protected TpvVyberContext _context = null!;

    [SetUp]
    public void Setup()
    {
        // 1. Create a connection to SQLite (In-Memory)
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // reuse these options for both the _context and the _factory
        var options = new DbContextOptionsBuilder<TpvVyberContext>()
            .UseSqlite(_connection) // Critical: Use the shared connection!
            .LogTo(message => NUnit.Framework.TestContext.WriteLine(message), LogLevel.Information)
            .EnableSensitiveDataLogging()
            .Options;

        _context = new TpvVyberContext(options);

        // Create Database
        _context.Database.EnsureCreated();

        Services.AddDbContextFactory<TpvVyberContext>(builder =>
        {
            builder
                .UseSqlite(_connection)
                .LogTo(
                    message => NUnit.Framework.TestContext.WriteLine(message),
                    LogLevel.Information
                )
                .EnableSensitiveDataLogging();
        });

        Services.AddLogging();

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddSerilog();
        });

        var logger = loggerFactory.CreateLogger("Notification service");

        AddBunitPersistentComponentState();
        AddAuthorization();

        HttpClient = new HttpClient() { BaseAddress = new Uri(Settings.WebUrl) };

        Services.AddScoped<IAdminService, ServerAdminService>();
        Services.AddScoped<ISelectService, ServerSelectService>();

        var notificationService = new NotificationService();
        notificationService.OnNotify += (UiNotification a) => logger.LogInformation(a.Message);
        Services.AddScoped(r => notificationService);
    }

    [TearDown]
    public void TearDown()
    {
        // 1. Dispose the context first
        _context?.Dispose();

        // 2. Safely disconnect and dispose the database connection
        // This will also wipe the in-memory database
        if (_connection != null)
        {
            _connection.Close();
            _connection.Dispose();
        }
    }

    // --- Helpers ---

    // 1. Simple implementation of IDbContextFactory for tests
    public class TestDbContextFactory : IDbContextFactory<TpvVyberContext>
    {
        private readonly DbContextOptions<TpvVyberContext> _options;

        public TestDbContextFactory(DbContextOptions<TpvVyberContext> options)
        {
            _options = options;
        }

        public TpvVyberContext CreateDbContext()
        {
            return new TpvVyberContext(_options);
        }
    }
}
