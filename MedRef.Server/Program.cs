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

builder.Services.AddSingleton(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<SavedRecord>("SavedRecords");
});

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

app.MapPost("/api/savedrecords",
async (
    SavedRecord record,
    IMongoCollection<SavedRecord> collection) =>
{
    if (string.IsNullOrWhiteSpace(record.Id))
    {
        record.Id = Guid.NewGuid().ToString();
    }

    await collection.InsertOneAsync(record);

    return Results.Ok(record);
});

app.MapGet("/api/savedrecords",
async (IMongoCollection<SavedRecord> collection) =>
{
    var records = await collection
        .Find(_ => true)
        .ToListAsync();

    return Results.Ok(records);
});

app.MapPut("/api/savedrecords/{id}",
async (
    string id,
    SavedRecord updatedRecord,
    IMongoCollection<SavedRecord> collection) =>
{
    updatedRecord.Id = id;

    var filter = Builders<SavedRecord>.Filter.Eq(x => x.Id, id);

    await collection.ReplaceOneAsync(
        filter,
        updatedRecord);

    return Results.Ok(updatedRecord);
});

app.MapDelete("/api/savedrecords/{id}",
async (
    string id,
    IMongoCollection<SavedRecord> collection) =>
{
    var filter =
        Builders<SavedRecord>.Filter.Eq(
            x => x.Id,
            id);

    await collection.DeleteOneAsync(filter);

    return Results.Ok();
});

app.Run();