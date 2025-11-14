using MudBlazor.Services;
using TpvVyber.Client.Pages;
using TpvVyber.Components;
using Microsoft.AspNetCore.DataProtection;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Extensions.Hosting;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region TLS
builder.Services.AddDataProtection()
    .UseEphemeralDataProtectionProvider();

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
#endregion

#region Logging
var otelEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
if (string.IsNullOrEmpty(otelEndpoint))
{
    throw new Exception("OTEL_EXPORTER_OTLP_ENDPOINT is not set");
}

var grafanaHeadersPath = builder.Configuration["GRAFANA_OTEL_HEADERS_PATH"];
if (string.IsNullOrEmpty(grafanaHeadersPath))
{
    throw new Exception("GRAFANA_OTEL_HEADERS_PATH is not set");
}
var grafanaHeaders = System.IO.File.ReadAllText(grafanaHeadersPath).Trim();
if (string.IsNullOrEmpty(grafanaHeaders))
{
    throw new Exception("GRAFANA_OTEL_HEADERS_PATH is empty");
}

// --- 3. Configure Serilog ---
builder.Host.UseSerilog((context, config) =>
{
    // Read base config from appsettings.json (for Console, MinimumLevel)
    config.ReadFrom.Configuration(context.Configuration);
    
    // If we have auth, add the OpenTelemetry sink
    // This sends Serilog logs to the OpenTelemetry SDK
    if (grafanaHeaders != null)
    {
        config.WriteTo.OpenTelemetry(opts =>
        {
            // We configure the *real* exporter in the main OTEL setup
        });
    }
});

// --- 4. Configure OpenTelemetry SDK ---
builder.Services.AddOpenTelemetry()
    .ConfigureResource(builder => builder.AddService(serviceName: "TpvVyber-dev"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation(); // Auto-instrument ASP.NET Core
        tracing.AddHttpClientInstrumentation(); // Auto-instrument HttpClients

        if (grafanaHeaders != null)
        {
            // Add the OTLP exporter for Traces
            tracing.AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri(otelEndpoint!);
                opts.Headers = $"{grafanaHeaders}";
            });
        }
    });
    // Note: We don't configure .WithLogging() here because Serilog is our logging provider.
    // The Serilog.Sinks.OpenTelemetry bridge handles sending logs to the SDK.
    // The SDK then picks up the exporter configuration from the Tracing setup.
    // To be explicit, you can also configure the OTLP exporter for logs:

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;

    if (grafanaHeaders != null)
    {
        // Add the OTLP exporter for Logs
        logging.AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri(otelEndpoint!);
            opts.Headers = $"{grafanaHeaders}";
        });
    }
});

#endregion

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
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

app.Run();
