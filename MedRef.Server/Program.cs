using MedRef.Server.Services;
using MongoDB.Driver;
using MedRef.Server.Configurations;
using MedRef.Shared.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using MedRef.Server.Data;


var builder = WebApplication.CreateBuilder(args);

// Register MongoDB contexts and data abstraction layers
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<ProfileRepository>();

// =========================================================================
// 1. REGISTER ALL SERVICES & CONFIGURATIONS (MUST BE BEFORE builder.Build)
// =========================================================================

var mongoDbSettings = builder.Configuration
    .GetSection("MongoDbSettings")
    .Get<MongoDbSettings>()
    ?? throw new InvalidOperationException("MongoDB settings are missing from configuration.");

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    return new MongoClient(mongoDbSettings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDbSettings.DatabaseName);
});

builder.Services.AddScoped<SavedCodeService>();

builder.Services.AddScoped<IMongoCollection<SavedRecord>>(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<SavedRecord>("SavedRecords");
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "MedRef.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    })
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
            ?? throw new InvalidOperationException("Google ClientId is missing.");
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
            ?? throw new InvalidOperationException("Google ClientSecret is missing.");
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    });

builder.Services.AddAuthorization();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.Secure = CookieSecurePolicy.Always;
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IMedlineService, MedlineService>();

builder.Services.AddHttpClient("MedlinePlus", client =>
{
    client.BaseAddress = new Uri("https://connect.medlineplus.gov/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNetlify", policy =>
    {
        policy.WithOrigins(
                "https://medreftool.netlify.app",
                "http://localhost:5124",
                "http://localhost:5265"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


// Make the Users collection available for injection in controllers (like UserController)
builder.Services.AddSingleton<IMongoCollection<User>>(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<User>("Users"); // Make sure this matches your DB collection name
});

// =========================================================================
// 2. BUILD THE APP (CALL THIS EXACTLY ONCE HERE)
// =========================================================================
var app = builder.Build();

// CRITICAL PROXY FIX: Forwarded headers must execute before any routing or auth middleware.
// Render's reverse proxy is not in KnownProxies by default, so clear the lists to trust it.
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

// Netlify proxies API/OAuth traffic to Render. Force the public origin so Google OAuth
// uses https://medreftool.netlify.app/signin-google (not the onrender.com host).
string? publicBaseUrl = app.Configuration["Authentication:PublicBaseUrl"];
if (!string.IsNullOrWhiteSpace(publicBaseUrl)
    && Uri.TryCreate(publicBaseUrl, UriKind.Absolute, out Uri? publicUri))
{
    app.Use(async (context, next) =>
    {
        context.Request.Scheme = publicUri.Scheme;
        context.Request.Host = publicUri.IsDefaultPort
            ? new HostString(publicUri.Host)
            : new HostString(publicUri.Host, publicUri.Port);
        await next();
    });
}

// =========================================================================  
// 3. CONFIGURE MIDDLEWARE PIPELINE & ENDPOINTS
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCookiePolicy();
app.UseCors("AllowNetlify");

// CRITICAL PIPELINE ORDER: Authenticate identity BEFORE executing endpoints
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints (must be after auth middleware)
app.MapControllers();


app.Run();