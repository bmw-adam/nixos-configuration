using Microsoft.AspNetCore.DataProtection;

namespace TpvVyber.Services;

public static class Tls
{
    public static void ConfigureTls(this WebApplicationBuilder builder)
    {
        builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();

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
            options.ListenAnyIP(
                1234,
                listenoptions =>
                {
                    listenoptions.UseConnectionLogging();
                }
            // listenOptions =>
            // {
            // listenOptions.UseHttps(pfxFile, pfxKeyPassword);
            // }
            );
        });
    }
}
