using System;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
using TpvVyber.Services;

namespace TpvVyber.Tests;

[TestClass]
public abstract class EeTestClass : PageTest
{
    protected TpvVyberContext _context = null!;
    protected IAdminService adminService = null!;
    protected ISelectService selectService = null!;

    [SetUp]
    public async Task Setup()
    {
        var options = new DbContextOptionsBuilder<TpvVyberContext>()
            .UseNpgsql(
                Environment.GetEnvironmentVariable("ConnectionStrings__TpvVyberDbUnlocked")
                    ?? throw new Exception("Nenašel jsem ConnectionStrings__TpvVyberDbUnlocked"),
                o =>
                {
                    o.MigrationsHistoryTable("__EFMigrationsHistory", "tpv_schema");
                }
            )
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

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
        };

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(Settings.WebUrl) };

        var notificationService = new NotificationService();
        notificationService.OnNotify += (UiNotification uiNotification) =>
            logger.LogInformation(uiNotification.Message);

        adminService = new ClientAdminService(httpClient, notificationService);
        selectService = new ClientSelectService(httpClient, notificationService);
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions()
        {
            ColorScheme = ColorScheme.Light,
            ViewportSize = new() { Width = 1920, Height = 1080 },
            IgnoreHTTPSErrors = true,
            BaseURL = "https://localhost:1234",
        };
    }

    [TearDown]
    public async Task TearDown()
    {
        // 1. Dispose the context first
        _context?.Dispose();
    }
}
