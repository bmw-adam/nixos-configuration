using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Layout;
using TpvVyber.Client.Services;
using TpvVyber.Client.Services.Admin;
using TpvVyber.Client.Services.Select;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddScoped(sp => new HttpClient()
{
    BaseAddress = new Uri(sp.GetRequiredService<NavigationManager>().BaseUri),
});

builder.Services.AddScoped<IAdminService, ClientAdminService>();
builder.Services.AddScoped<ISelectService, ClientSelectService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddSingleton<ClientUpdateService>();
builder.Services.AddSingleton<IUpdateService>(sp => sp.GetRequiredService<ClientUpdateService>());

builder.Services.AddBlazorBootstrap();

var app = builder.Build();
await app.RunAsync();
