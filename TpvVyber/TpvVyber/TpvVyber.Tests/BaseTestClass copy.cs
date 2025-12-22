using System;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Serilog;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Client.Services.Select;
using TpvVyber.Data;
using TpvVyber.Endpoints.Admin;
using TpvVyber.Endpoints.Select;
using TpvVyber.Tests.Constants;

namespace TpvVyber.Tests;

[TestClass]
public abstract class EeTestClass : PageTest
{
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

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddSerilog();
        });

        var logger = loggerFactory.CreateLogger("Notification service");
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions()
        {
            ColorScheme = ColorScheme.Light,
            ViewportSize = new() { Width = 1920, Height = 1080 },
            BaseURL = Settings.WebUrl,
        };
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
}
