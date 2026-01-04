using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using NUnit.Framework;
using Serilog;
using TpvVyber.Classes;
using TpvVyber.Data;
using TpvVyber.Endpoints.Admin;

namespace TpvVyber.Tests;

[NonParallelizable]
[TestFixture]
public class PickerPageTest : EeTestClass
{
    [Test]
    public async Task TestAdminPageIndex()
    {
        await Page.GotoAsync("/");
        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Kurzy" })
            .ClickAsync();
        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Admin" })
            .ClickAsync();
        _context.ChangeTracker.Clear();
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "delete-btn" })
            .ClickAsync();
        Thread.Sleep(4000);
        Assert.IsFalse(_context.Courses.Any());
        Assert.IsFalse(_context.Students.Any());
        Assert.IsFalse(_context.OrderCourses.Any());

        _context.ChangeTracker.Clear();
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "add-course-btn" })
            .ClickAsync();
        Thread.Sleep(4000);
        Assert.That(_context.Courses.Count() == 1);

        _context.ChangeTracker.Clear();
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "delete-btn" })
            .ClickAsync();
        Thread.Sleep(4000);
        Assert.IsFalse(_context.Courses.Any());

        _context.ChangeTracker.Clear();
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "add-student-btn" })
            .ClickAsync();
        Thread.Sleep(4000);
        Assert.AreEqual(1, _context.Students.Count());

        _context.ChangeTracker.Clear();
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "delete-btn" })
            .ClickAsync();
        Thread.Sleep(4000);
        Assert.IsFalse(_context.Courses.Any());

        await Expect(Page.GetByLabel("admin-rozhrani"))
            .ToContainTextAsync("Administrační rozhraní");
        await Expect(Page.GetByLabel("admin-paragraf"))
            .ToContainTextAsync(
                "Vítejte v administračním rozhraní aplikace Tpv Výběr. Zde můžete spravovat různé aspekty aplikace, včetně kurzů a žáků."
            );
        await Expect(Page.GetByLabel("delete-btn"))
            .ToContainTextAsync("Smazat všechna data (kurzy, žáky a pořadí kurzů)");

        await Expect(Page.GetByLabel("add-student-btn"))
            .ToContainTextAsync("Přidat Žáka s náhodně generovanými hodnotami a pořadím kurzů");
        await Expect(Page.GetByLabel("add-course-btn"))
            .ToContainTextAsync("Přidat kurz s náhodně generovanými hodnotami");

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "delete-btn" })
            .ClickAsync();
        await Expect(Page.GetByLabel("Správa kurzů"))
            .ToContainTextAsync("Správa kurzů (0) Kurzy TPV");
        await Expect(Page.GetByLabel("Správa žáků")).ToContainTextAsync("Správa žáků (0) Žáci");
        await Expect(Page.GetByLabel("Pořadí kurzů"))
            .ToContainTextAsync("Pořadí kurzů (0) Pořadí kurzů");
    }

    [Test]
    public async Task TestAdminPageResults()
    {
        await Page.GotoAsync("/admin");
        _context.ChangeTracker.Clear();
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "delete-btn" })
            .ClickAsync();
        Thread.Sleep(4000);
        Assert.IsFalse(_context.Courses.Any());
        Assert.IsFalse(_context.Students.Any());
        Assert.IsFalse(_context.OrderCourses.Any());

        _context.ChangeTracker.Clear();
        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Výsledky" })
            .ClickAsync();
        await Expect(Page.GetByLabel("vysledky-header", new PageGetByLabelOptions { Exact = true }))
            .ToContainTextAsync("Výsledky");
        await Expect(Page.GetByLabel("vysledky-header-konec")).ToContainTextAsync("Konec:");
        await Expect(Page.GetByLabel("vysledky-konec-sumbit").Locator("span"))
            .ToContainTextAsync("Ok");
        await Expect(Page.Locator("i"))
            .ToContainTextAsync("Kolik lidí si dalo kurz na kolikáté pořadí?");
        await Expect(Page.GetByLabel("vysledky-vysledky-header"))
            .ToContainTextAsync("Pořadí - Výsledky");
        await Expect(Page.GetByLabel("vysledky-vysledky-export")).ToContainTextAsync("Export CSV");
    }
}
