using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using MedRef.Client;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


// resolve the API base address (checks configuration first, falls back to local server port)
var backendUrl = builder.Configuration["BackendUrl"] ?? "http://localhost:5035/";

// Register a single HttpClient instance configured to forward cookies seamlessly
builder.Services.AddScoped(sp =>
{
    return new HttpClient { BaseAddress = new Uri(backendUrl) };
});

// Register standard Blazor authentication state plumbing
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomCookieAuthenticationStateProvider>();

await builder.Build().RunAsync();