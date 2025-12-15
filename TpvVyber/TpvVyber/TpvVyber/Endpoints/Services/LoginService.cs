using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace TpvVyber.Services;

public static class LoginService
{
    public static void UseLoginService(this WebApplication app, string redirectUri)
    {
        app.MapGet(
            "/login",
            (HttpContext context) =>
            {
                return Results.Challenge(
                    new AuthenticationProperties { RedirectUri = redirectUri },
                    new[] { OpenIdConnectDefaults.AuthenticationScheme }
                );
            }
        );

        // Minimal logout endpoint
        app.MapGet(
            "/logout",
            async (HttpContext context) =>
            {
                await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.Redirect("/");
            }
        );
    }
}
