using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
// using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
// using Keycloak.Net;
using OpenTelemetry.Extensions.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using TpvVyber.Client.Classes.Client;
using TpvVyber.Client.Classes.Interfaces;
using TpvVyber.Client.Pages;
using TpvVyber.Components;
using TpvVyber.Data;
using TpvVyber.Services;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureTls();
builder.AddLoggingService();
builder.AddDatabaseService();

#region Auth
builder.Services.AddHttpContextAccessor();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddCors(policy =>
{
    policy.AddPolicy("CorsPolicy", opt => opt.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var clientPath = builder.Configuration["OAUTH_CLIENT"];
if (string.IsNullOrEmpty(clientPath))
{
    throw new Exception("OAUTH_CLIENT is not set");
}
var client = System.IO.File.ReadAllText(clientPath).Trim();
if (string.IsNullOrEmpty(client))
{
    throw new Exception("OAUTH_CLIENT is empty");
}

Client? decodedClient = null;

try
{
    decodedClient = JsonSerializer.Deserialize<Client>(client);
}
catch (Exception ex)
{
    throw new Exception("OAUTH_CLIENT is not valid", ex);
}

if (
    decodedClient == null
    || string.IsNullOrEmpty(decodedClient.ClientId)
    || string.IsNullOrEmpty(decodedClient.Secret)
)
{
    throw new Exception("OAUTH_CLIENT is missing required fields");
}

// builder
//     .Services.AddAuthentication(options =>
//     {
//         options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//         options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
//     })
//     .AddCookie()
//     .AddOpenIdConnect(
//         "Keycloak",
//         options =>
//         {
//             options.Authority = "https://sso.gasos.cz/realms/ucs";
//             options.ClientId = decodedClient.ClientId;
//             options.ClientSecret = decodedClient.Secret;
//             options.ResponseType = "code";
//             options.SaveTokens = true;
//         }
//     );
// builder.Services.AddScoped<
//     AuthenticationStateProvider,
//     PersistingRevalidatingAuthenticationStateProvider
// >();
// builder
//     .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//     .AddCookie();

builder.Services.AddCascadingAuthenticationState();

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "Keycloak";
    })
    .AddCookie()
    .AddOAuth(
        "Keycloak",
        options =>
        {
            options.ClientId = decodedClient.ClientId;
            options.ClientSecret = decodedClient.Secret;

            options.CallbackPath = "/auth/callback";
            options.AuthorizationEndpoint =
                "https://sso.gasos.cz/realms/ucs/protocol/openid-connect/auth";
            options.TokenEndpoint = "https://sso.gasos.cz/realms/ucs/protocol/openid-connect/token";
            options.UserInformationEndpoint =
                "https://sso.gasos.cz/realms/ucs/protocol/openid-connect/userinfo";

            options.SaveTokens = true;

            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");

            options.ClaimActions.MapJsonKey(ClaimTypes.Name, "preferred_username");

            options.Events.OnCreatingTicket = async ctx =>
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    ctx.Options.UserInformationEndpoint
                );
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer",
                        ctx.AccessToken
                    );

                var response = await ctx.Backchannel.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var user = System.Text.Json.JsonDocument.Parse(json);

                ctx.RunClaimActions(user.RootElement);
            };
        }
    );

#endregion

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder
    .Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddAntiforgery();

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseBlazorFrameworkFiles();

app.UseRouting();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(TpvVyber.Client._Imports).Assembly);

// Wait for the database to be ready and apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TpvVyberContext>();
    var maxWaitTime = TimeSpan.FromMinutes(5); // maximum total wait
    var delay = TimeSpan.FromSeconds(15); // wait between retries
    var startTime = DateTime.UtcNow;
    var migrationApplied = false;

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
            Console.WriteLine(
                $"Database not ready yet: {ex.Message}. Retrying in {delay.TotalSeconds}s..."
            );
            Thread.Sleep(delay);
        }
    }

    if (!migrationApplied)
    {
        throw new Exception(
            "Failed to connect to the database and apply migrations within the timeout."
        );
    }
    else
    {
        Console.WriteLine("Database migrations applied successfully.");
    }
}

// Use authentication & authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapGet(
    "/signin-oauth",
    async context =>
    {
        await context.ChallengeAsync(
            "Keycloak",
            new AuthenticationProperties { RedirectUri = "/counter" }
        );
    }
);

app.MapGet(
    "/auth/logout",
    async context =>
    {
        await context.SignOutAsync();
        context.Response.Redirect("/");
    }
);

app.Run();
