using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MedRef.Client;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Auth0", options.ProviderOptions);

    options.ProviderOptions.ResponseType = "code";
});

// resolve the API base address (checks configuration first, falls back to local server port)
var backendUrl = builder.Configuration["BackendUrl"] ?? "http://localhost:5035/";

// register a single HttpClient instance using that backend destination
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(backendUrl)
});

await builder.Build().RunAsync();