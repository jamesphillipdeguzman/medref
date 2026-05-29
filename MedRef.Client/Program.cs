using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MedRef.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 1. Resolve the API base address (checks configuration first, falls back to local server port)
var backendUrl = builder.Configuration["BackendUrl"] ?? "http://localhost:5035/";

// 2. Register a single HttpClient instance using that backend destination
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(backendUrl)
});

// 3. Build and execute the WebAssembly application thread
await builder.Build().RunAsync();