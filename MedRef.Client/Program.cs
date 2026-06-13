using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using MedRef.Client;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// =========================================================================
// ENVIRONMENT-AWARE API BASE ADDRESS CONFIGURATION
// =========================================================================
var apiBaseUrl = builder.HostEnvironment.IsDevelopment()
    ? "http://localhost:5035/"
    : "https://medreftool.netlify.app/";

// Register a single HttpClient instance configured to forward cookies seamlessly
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

// =========================================================================
// AUTHENTICATION & CORE PLUMBING
// =========================================================================
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomCookieAuthenticationStateProvider>();

await builder.Build().RunAsync();