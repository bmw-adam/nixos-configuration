using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using BlazorBootstrap;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using MudBlazor.Services;
using Serilog;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Layout;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Client.Services.Select;
using TpvVyber.Components;
using TpvVyber.Data;
using TpvVyber.Endpoints.Admin;
using TpvVyber.Endpoints.Select;
using TpvVyber.Hubs;
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

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRazorPages();

builder.Services.AddControllers();

builder.Services.AddAntiforgery();

builder.Services.AddSignalR();

// builder.Services.AddScoped<IHubContext<UpdateHub>>(sp =>
// {
//     return sp.GetRequiredService<IHubContext<UpdateHub>>();
// });

builder.Services.AddSingleton<ServerUpdateService>();

builder.ConfigureTls();
builder.AddLoggingService();
builder.AddDatabaseService();

builder.AddAuthService();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAdminService, ServerAdminService>();
builder.Services.AddScoped<ISelectService, ServerSelectService>();
builder.Services.AddScoped<IUpdateService>(sp => sp.GetRequiredService<ServerUpdateService>());

builder.Services.AddBlazorBootstrap();

builder.Services.AddScoped<NotificationService>();

var app = builder.Build();

var forwardedHeaderOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};
forwardedHeaderOptions.KnownNetworks.Clear();
forwardedHeaderOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeaderOptions);

// app.UseBlazorFrameworkFiles();

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
    .AddInteractiveServerRenderMode(o => o.DisableWebSocketCompression = true)
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(TpvVyber.Client._Imports).Assembly);

app.MapRazorPages();

await app.UseDatabaseService();

// Minimal login endpoint

app.UseLoginService();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

if (app.Configuration.GetValue<bool?>("Testing") ?? false)
{
    logger.LogWarning("Running as Testing project.");

    app.Use(
        async (ctx, next) =>
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "Admin"),
                new Claim(ClaimTypes.Email, "Tester"),
                new Claim(ClaimTypes.Role, "Admin"),
            };

            ctx.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

            await next();
        }
    );
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map Endpoints
CoursesAdminEndpoints.MapAdminEndpoints(app);
StudentsAdminEndpoints.MapAdminEndpoints(app);
OrderCourseAdminEndpoints.MapAdminEndpoints(app);
StudentHistoryAdminEndpoints.MapAdminEndpoints(app);
SelectEndpoints.MapSelectEndpoints(app);

// app.MapBlazorHub();
app.MapHub<UpdateHub>("/update");

logger.LogInformation("Running.");

app.Run();
