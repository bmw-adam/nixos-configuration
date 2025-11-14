using MudBlazor.Services;
using TpvVyber.Client.Pages;
using TpvVyber.Components;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

#region TLS
var pfxKey = builder.Configuration["TLS_PFX_KEY"];
if (string.IsNullOrEmpty(pfxKey))
{
    throw new Exception("TLS_PFX_KEY is not set");
}

var pfxKeyPassword = System.IO.File.ReadAllText(pfxKey).Trim();
if (string.IsNullOrEmpty(pfxKeyPassword))
{
    throw new Exception("TLS_PFX_KEY is empty");
}

var pfxFile = builder.Configuration["TLS_PFX_FILE"];
if (string.IsNullOrEmpty(pfxFile))
{
    throw new Exception("TLS_PFX_FILE is not set");
}

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(1235, listenOptions =>
    {
        listenOptions.UseHttps(pfxFile, pfxKeyPassword);
    });
});

// builder.Services.AddDataProtection()
//     .UseEphemeralDataProtectionProvider();
#endregion

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddAntiforgery();

var app = builder.Build();

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

app.Run();
