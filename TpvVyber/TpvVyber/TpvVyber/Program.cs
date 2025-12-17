using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using BlazorBootstrap;
using Microsoft.AspNetCore.HttpOverrides;
using MudBlazor.Services;
using Serilog;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Client.Services.Select;
using TpvVyber.Components;
using TpvVyber.Data;
using TpvVyber.Endpoints.Admin;
using TpvVyber.Endpoints.Select;
using TpvVyber.Services;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddHttpClient();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder
    .Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);
;

builder.Services.AddRazorPages();

builder.Services.AddControllers();

builder.Services.AddAntiforgery();

builder.ConfigureTls();
builder.AddLoggingService();
builder.AddDatabaseService();

builder.AddAuthService();

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IAdminService, ServerAdminService>();
builder.Services.AddScoped<ISelectService, ServerSelectService>();

builder.Services.AddBlazorBootstrap();

builder.Services.AddScoped<NotificationService>();

var redirectUri =
    builder.Configuration["RedirectUri"] ?? throw new Exception("Redirect uri was not set.");


var app = builder.Build();

app.Use(
    (context, next) =>
    {
        // Force the app to believe it is running on HTTPS.
        // This allows Secure cookies to be read even if the proxy sends "http"
        context.Request.Scheme = "https";
        return next();
    }
);
app.UseForwardedHeaders();

app.UseBlazorFrameworkFiles();

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

// app.MapStaticAssets();

app.UseCors("CorsPolicy");

app.UseRouting();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(o =>  o.DisableWebSocketCompression = true)
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(TpvVyber.Client._Imports).Assembly);

app.MapRazorPages();

await app.UseDatabaseService();

// Minimal login endpoint
app.UseLoginService(redirectUri);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map Endpoints
CoursesAdminEndpoints.MapAdminEndpoints(app);
StudentsAdminEndpoints.MapAdminEndpoints(app);
OrderCourseAdminEndpoints.MapAdminEndpoints(app);

// Select
SelectEndpoints.MapSelectEndpoints(app);

var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Running.");

app.Run();
