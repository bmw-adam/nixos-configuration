using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Added for Task
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using NUnit.Framework;
using Serilog;
using Serilog.Extensions.Logging;
using TpvVyber.Classes;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Data;
using TpvVyber.Endpoints.Admin;
using TpvVyber.Services;
using TpvVyber.Tests.BaseTest;

namespace TpvVyber.Tests;

[NonParallelizable]
[TestFixture]
public class ApiTestLoggingEndings : BaseApiTest
{
    [Test]
    public async Task TestAdminApi()
    {
        await TestInsert();
        await TestRead();
        await TestUpdate();
        await TestDelete();
    }

    public LoggingEndingCln TestLoggingEnding = new LoggingEndingCln { TimeEnding = DateTime.Now };

    private async Task TestInsert()
    {
        await using var dbContext = DbContextFactory.CreateDbContext();

        dbContext.LoggingEndings.Add(LoggingEnding.ToServer(TestLoggingEnding, dbContext, true));
        await dbContext.SaveChangesAsync();

        var addedLoggingEnding = dbContext.LoggingEndings.FirstOrDefault(c =>
            c.TimeEnding == TestLoggingEnding.TimeEnding
        );
        Assert.IsNotNull(addedLoggingEnding);
        TestLoggingEnding.Id = addedLoggingEnding.Id;
        Assert.That(addedLoggingEnding.TimeEnding, Is.EqualTo(TestLoggingEnding.TimeEnding));
    }

    private async Task TestRead()
    {
        var loggingEnding = await AdminService.GetLoggingEndings(reThrowError: true);

        Assert.That(loggingEnding, Is.Not.Null);
        Assert.That(loggingEnding!.TimeEnding, Is.EqualTo(TestLoggingEnding.TimeEnding));
    }

    private async Task TestUpdate()
    {
        var cr = await AdminService.GetLoggingEndings(reThrowError: true);
        var updatedTime = DateTime.Now.AddHours(1);

        Assert.IsNotNull(cr);
        cr.TimeEnding = updatedTime;

        await AdminService.UpdateLoggingEnding(cr, reThrowError: true);

        var newCr = await AdminService.GetLoggingEndings(reThrowError: true);

        Assert.IsNotNull(newCr);
        Assert.That(newCr.TimeEnding, Is.EqualTo(updatedTime));
    }

    private async Task TestDelete()
    {
        using var dbContext = DbContextFactory.CreateDbContext();
        dbContext.LoggingEndings.RemoveRange(dbContext.LoggingEndings);
        await dbContext.SaveChangesAsync();

        Assert.IsEmpty(
            dbContext.LoggingEndings.ToList(),
            "There should be no LoggingEndings left after deletion."
        );
    }
}
