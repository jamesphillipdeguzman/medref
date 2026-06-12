using MedRef.Server.Services;
using MongoDB.Driver;
using MedRef.Server.Configurations;
using MedRef.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// MONGODB CONFIGURATION & SERVICE PATTERNS
// =========================================================================

// 1. Bind the configuration keys
// This grabs the dummy structure from appsettings.json and overlays your secret keys 
// from your local machine's user-secrets store.
var mongoDbSettings = builder.Configuration
    .GetSection("MongoDbSettings")
    .Get<MongoDbSettings>()
    ?? throw new InvalidOperationException("MongoDB settings are missing from configuration.");

// 2. Register IMongoClient as a Singleton
// This sets up the actual connection pool to your Atlas cluster. It's a heavy 
// process, so it only runs once for the lifecycle of your server.
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    return new MongoClient(mongoDbSettings.ConnectionString);
});

// 3. Register IMongoDatabase as a Singleton
// This pulls the active client factory we registered above and points it directly 
// at your 'MedRefDB' database context.
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDbSettings.DatabaseName);
});

// 4. Register your custom services that depend on IMongoDatabase
// This allows you to inject IMongoDatabase into any service that needs it, like your SavedCodeService.
builder.Services.AddScoped<SavedCodeService>();

// =========================================================================

// Your existing services continue below (e.g., builder.Services.AddControllers())
builder.Services.AddControllers();

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IMedlineService, MedlineService>();

builder.Services.AddHttpClient("MedlinePlus", client =>
{
    client.BaseAddress = new Uri("https://connect.medlineplus.gov/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// CORS
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
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowNetlify");

// API
app.MapGet("/api/medlineproxy",
async (string code, IMedlineService medlineService, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(code))
        return Results.BadRequest("Code is required.");

    var data = await medlineService.GetMedlineDataAsync(code, ct);

    return data is not null
        ? Results.Ok(data)
        : Results.NotFound($"No Medline data found for {code}");
});

// Endpoint to retrieve all saved codes
app.MapGet("/api/savedcodes", async (SavedCodeService savedCodeService) =>
{
    var codes = await savedCodeService.GetSavedCodesAsync();
    return Results.Ok(codes);
});

// Endpoint to add a new saved code
app.MapPost("/api/savedcodes/add", async (SavedCode newCode, SavedCodeService savedCodeService) =>
{
    if (string.IsNullOrWhiteSpace(newCode.CodeValue) || string.IsNullOrWhiteSpace(newCode.Description))
    {
        return Results.BadRequest("Both CodeValue and Description are required.");
    }

    var createdCode = await savedCodeService.AddSavedCodeAsync(newCode);
    return Results.Created($"/api/savedcodes/{createdCode.Id}", createdCode);
});

app.Run();