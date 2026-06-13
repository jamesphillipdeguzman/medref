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
// Production uses the Netlify origin so auth cookies are first-party (same-site).
// The Netlify _redirects file proxies /api/* and /signin-google to Render.
var apiBaseUrl = builder.HostEnvironment.IsDevelopment()
    ? "http://localhost:5035/"
    : "https://medreftool.netlify.app/";

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