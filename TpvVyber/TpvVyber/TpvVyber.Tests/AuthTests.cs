using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using TpvVyber.Classes;
using TpvVyber.Data;
using TpvVyber.Endpoints.Admin;

namespace TpvVyber.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class CrudTests : EeTestClass
{
    [Test]
    public async Task TestWithCustomContextOptions()
    {
        // The following Page (and BrowserContext) instance has the custom colorScheme, viewport and baseURL set:
        await Page.GotoAsync("/login");
    }
}
