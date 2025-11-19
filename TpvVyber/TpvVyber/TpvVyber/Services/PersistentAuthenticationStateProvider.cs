using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using TpvVyber.Client.Classes;

namespace TpvVyber.Services;

public class PersistingRevalidatingAuthenticationStateProvider
    : RevalidatingServerAuthenticationStateProvider
{
    private readonly PersistentComponentState state;
    private readonly PersistingComponentStateSubscription subscription;
    private Task<AuthenticationState>? authenticationStateTask;

    public PersistingRevalidatingAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        PersistentComponentState persistentComponentState
    )
        : base(loggerFactory)
    {
        this.state = persistentComponentState;
        AuthenticationStateChanged += OnAuthenticationStateChanged;
        subscription = state.RegisterOnPersisting(
            OnPersistingAsync,
            RenderMode.InteractiveWebAssembly
        );
    }

    private async Task OnPersistingAsync()
    {
        if (authenticationStateTask is null)
        {
            throw new UnreachableException(
                $"Authentication state not set in {nameof(OnPersistingAsync)}()."
            );
        }

        var authenticationState = await authenticationStateTask;
        var principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated == true)
        {
            var userId = principal.Identity.Name;

            if (userId != null)
            {
                state.PersistAsJson(nameof(LoginModel), new LoginModel { UserName = userId });
            }
        }
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        authenticationStateTask = task;
    }

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    protected override Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken
    )
    {
        var user = authenticationState.User;
        if (user is null)
        {
            return Task.FromResult(false);
        }
        else
        {
            return Task.FromResult(true);
            ;
        }
    }
}
