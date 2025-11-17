using OpenTelemetry.Extensions.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace TpvVyber.Services;

public static class Logging
{
    public static void AddLoggingService(this WebApplicationBuilder builder)
    {
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
        builder.Host.UseSerilog(
            (context, config) =>
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
            }
        );

        // --- 4. Configure OpenTelemetry SDK ---
        builder
            .Services.AddOpenTelemetry()
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
    }
}
