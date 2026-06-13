using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http; // Crucial for cookie extension methods
using MedRef.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// =========================================================================
// ENVIRONMENT-AWARE API BASE ADDRESS CONFIGURATION
// =========================================================================
// Production must call the backend directly so the auth cookie (set on onrender.com
// during Google OAuth) is included on credentialed cross-origin API requests.
var apiBaseUrl = builder.HostEnvironment.IsDevelopment()
    ? "http://localhost:5035/"
    : "https://medref-backend-565n.onrender.com/";

// Register HttpClient using a delegating handler to inject credentials natively
builder.Services.AddScoped(sp =>
{
    return new HttpClient(new CookieHandler())
    {
        BaseAddress = new Uri(apiBaseUrl)
    };
});

// =========================================================================
// AUTHENTICATION & CORE PLUMBING
// =========================================================================
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomCookieAuthenticationStateProvider>();

await builder.Build().RunAsync();

// =========================================================================
// NATIVE BROWSER FETCH COOKIE INTERCEPTOR
// =========================================================================
public class CookieHandler : DelegatingHandler
{
    public CookieHandler()
    {
        InnerHandler = new HttpClientHandler();
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // This tells the browser's fetch API to include secure SameSite/BFF cookies
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return base.SendAsync(request, cancellationToken);
    }
}