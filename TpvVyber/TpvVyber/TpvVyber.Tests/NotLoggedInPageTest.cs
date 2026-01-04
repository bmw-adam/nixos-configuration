using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using NUnit.Framework;
using TpvVyber.Classes;
using TpvVyber.Data;
using TpvVyber.Endpoints.Admin;

namespace TpvVyber.Tests;

[NonParallelizable]
[TestFixture]
public class NotLoggedInPageTest : EeTestClass
{
    // [Test]
    public async Task TestWithCustomContextOptions()
    {
        // // await Page.GotoAsync(Settings.WebUrl);
        // await Page.Locator("html").ClickAsync();
        // await Expect(Page.Locator("h5")).ToContainTextAsync("Tpv Výběr");
        // await Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "navmenu-header" })
        //     .ClickAsync();
        // await Expect(Page.GetByLabel("navmenu-header", new PageGetByLabelOptions { Exact = true }))
        //     .ToContainTextAsync("Tpv Výběr");
        // await Expect(Page.GetByLabel("navmenu-header-login")).ToContainTextAsync("Přihlásit se");
        // await Expect(Page.GetByLabel("navmenu-home-link").Locator("div"))
        //     .ToContainTextAsync("Home");
        // await Expect(Page.GetByLabel("navmenu-tpvvyber-link").Locator("div"))
        //     .ToContainTextAsync("Výběr TPV");
        // await Expect(Page.GetByLabel("navmenu-login-link")).ToContainTextAsync("Login");
        // await Expect(Page.GetByLabel("loginBtn")).ToContainTextAsync("Přihlásit se");
        // await Expect(Page.GetByLabel("mudtable-notloggedpromo-Název")).ToContainTextAsync("Název");
        // await Expect(Page.GetByLabel("mudtable-notloggedpromo-Popis")).ToContainTextAsync("Popis");
        // await Expect(Page.GetByLabel("mudtable-notloggedpromo-Url")).ToContainTextAsync("Url");
        // await Expect(Page.GetByLabel("mudtable-notloggedpromo-Cena")).ToContainTextAsync("Cena");
        // await Expect(Page.GetByLabel("mudtable-notloggedpromo-Kapacita"))
        //     .ToContainTextAsync("Kapacita");
        // await Expect(Page.GetByLabel("mudtable-notloggedpromo-Obsazenost"))
        //     .ToContainTextAsync("Obsazenost");
    }
}
