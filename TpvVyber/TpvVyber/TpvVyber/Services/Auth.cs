using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Data;
using TpvVyber.Extensions;

namespace TpvVyber.Services;

public static class Auth
{
    public static void AddAuthService(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddBlazoredLocalStorage();

        builder.Services.AddCors(policy =>
        {
            policy.AddPolicy(
                "CorsPolicy",
                opt => opt.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
            );
        });

        var clientPath = builder.Configuration["OIDC_SECRET"];
        if (string.IsNullOrEmpty(clientPath))
        {
            throw new Exception("OIDC_SECRET is not set");
        }
        var oauth_key = System.IO.File.ReadAllText(clientPath).Trim();
        if (string.IsNullOrEmpty(oauth_key))
        {
            throw new Exception("OIDC_SECRET is empty");
        }

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
        };
        var httpClient = new HttpClient(handler);

        builder
            .Services.AddHttpClient("KeycloakClient")
            .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                }
            );

        builder
            .Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddOpenIdConnect(
                OpenIdConnectDefaults.AuthenticationScheme,
                options =>
                {
                    options.UseTokenLifetime = false;
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                    var backendIdpUrl = "https://sso.gasos.cz/realms/ucs";
                    var clientIdpUrl = "https://sso.gasos.cz/realms/ucs";

                    options.Configuration = new()
                    {
                        Issuer = backendIdpUrl,
                        AuthorizationEndpoint = $"{clientIdpUrl}/protocol/openid-connect/auth",
                        TokenEndpoint = $"{backendIdpUrl}/protocol/openid-connect/token",
                        JwksUri = $"{backendIdpUrl}/protocol/openid-connect/certs",
                        JsonWebKeySet = FetchJwks(
                            $"{backendIdpUrl}/protocol/openid-connect/certs",
                            httpClient
                        ),
                        EndSessionEndpoint = $"{clientIdpUrl}/protocol/openid-connect/logout",
                    };
                    foreach (var key in options.Configuration.JsonWebKeySet.GetSigningKeys())
                    {
                        options.Configuration.SigningKeys.Add(key);
                    }

                    options.ClientId = "tpv-vyber-01";
                    options.ClientSecret = oauth_key;

                    options.TokenValidationParameters.ValidIssuers = [clientIdpUrl, backendIdpUrl];
                    options.CallbackPath = "/signin-oauth";

                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;
                    options.MapInboundClaims = true;
                    options.ResponseMode = OpenIdConnectResponseMode.FormPost;

                    options.RequireHttpsMetadata = true;

                    // options.BackchannelHttpHandler = new HttpClientHandler
                    // {
                    //     ServerCertificateCustomValidationCallback =
                    //         HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                    // };

                    // options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("basic");
                    options.Scope.Add("acr");
                    options.Scope.Add("service_account");

                    // Roles
                    var roleScope = "description";
                    options.Scope.Add(roleScope);

                    // Email
                    var emailScope = "email";
                    var superUser = "baboraka@gasos-ro.cz";
                    options.Scope.Add(emailScope);

                    options.TokenValidationParameters.NameClaimType = "name";
                    options.TokenValidationParameters.RoleClaimType = roleScope;

                    options.Events = new OpenIdConnectEvents
                    {
                        OnTokenValidated = async context => // TODO create a student there
                        {
                            var identity = (ClaimsIdentity)context.Principal!.Identity!;

                            //    (Správce -> Admin)
                            var descriptionClaims = identity.FindAll(roleScope).ToList();

                            foreach (var claim in descriptionClaims)
                            {
                                string roleValue = claim.Value;
                                bool wasTranslated = false;

                                // Check for "Správce" and standardize to "Admin"
                                if (roleValue.Equals("Správce", StringComparison.OrdinalIgnoreCase))
                                {
                                    roleValue = "Admin";
                                    wasTranslated = true;
                                }

                                // Add the standardized Role claim (Required for [Authorize] attributes)
                                identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));

                                // If we translated the value, replace the original 'description' claim
                                if (wasTranslated)
                                {
                                    identity.RemoveClaim(claim);
                                    identity.AddClaim(new Claim(roleScope, roleValue));
                                }
                            }

                            // Try to find email in standard claim type or raw "email" key
                            var userEmail =
                                identity.FindFirst(ClaimTypes.Email)?.Value
                                ?? identity.FindFirst(emailScope)?.Value;

                            if (
                                !string.IsNullOrEmpty(userEmail)
                                && userEmail.Equals(superUser, StringComparison.OrdinalIgnoreCase)
                            )
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                                identity.AddClaim(new Claim(roleScope, "Admin"));
                            }

                            if (string.IsNullOrWhiteSpace(userEmail))
                                return;

                            var db =
                                context.HttpContext.RequestServices.GetRequiredService<TpvVyberContext>();

                            var adminService =
                                context.HttpContext.RequestServices.GetRequiredService<IAdminService>();

                            var user = db.Students.FirstOrDefault(u => u.Email == userEmail);

                            var userInfo = context.Principal.Claims.GetCurrentUser();

                            if (user == null)
                            {
                                var newStudent = new StudentCln
                                {
                                    Class = string.Join(";", userInfo.UserRoles),
                                    Email = userEmail,
                                    Name = userInfo.UserName,
                                };

                                var newStudentDb = await adminService.AddStudentAsync(
                                    newStudent,
                                    false
                                );

                                if (newStudentDb != null)
                                {
                                    var newUsersOrderCourses = db.OrderCourses.Where(e =>
                                        e.StudentId == newStudentDb.Id
                                    );
                                }
                            }
                            return;
                        },
                    };
                }
            )
            .AddCookie(
                CookieAuthenticationDefaults.AuthenticationScheme,
                options =>
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.HttpOnly = false;
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.ExpireTimeSpan = TimeSpan.FromDays(2);
                    options.SlidingExpiration = false;
                }
            );

        builder.Services.AddAuthorization();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddAuthorization();

        builder.Services.AddHttpContextAccessor();
    }

    private static JsonWebKeySet FetchJwks(string url, HttpClient httpClient)
    {
        var result = httpClient.GetAsync(url).Result;
        if (!result.IsSuccessStatusCode || result.Content is null)
        {
            throw new Exception(
                $"Getting token issuers (Keycloaks) JWKS from {url} failed. Status code {result.StatusCode}"
            );
        }

        var jwks = result.Content.ReadAsStringAsync().Result;
        return new JsonWebKeySet(jwks);
    }
}
