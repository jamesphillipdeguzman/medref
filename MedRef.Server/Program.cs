using MedRef.Server.Services;
using MongoDB.Driver;
using MedRef.Server.Configurations;
using MedRef.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// MONGODB CONFIGURATION & SERVICE PATTERNS
// =========================================================================

// 1. Bind the configuration keys
var mongoDbSettings = builder.Configuration
    .GetSection("MongoDbSettings")
    .Get<MongoDbSettings>()
    ?? throw new InvalidOperationException("MongoDB settings are missing from configuration.");

// 2. Register IMongoClient as a Singleton
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    return new MongoClient(mongoDbSettings.ConnectionString);
});

// 3. Register IMongoDatabase as a Singleton
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDbSettings.DatabaseName);
});

// 4. Register your custom services that depend on IMongoDatabase
builder.Services.AddScoped<SavedCodeService>();

// 5. Register Jeremy's collection dependency so his endpoints can resolve it
builder.Services.AddScoped<IMongoCollection<SavedRecord>>(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<SavedRecord>("SavedRecords");
});

// =========================================================================

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

// =========================================================================
// API ENDPOINTS
// =========================================================================

// Medline Proxy
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

// Jeremy's Saved Records Endpoints
app.MapPost("/api/savedrecords",
async (SavedRecord record, IMongoCollection<SavedRecord> collection) =>
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
async (string id, SavedRecord updatedRecord, IMongoCollection<SavedRecord> collection) =>
{
    updatedRecord.Id = id;
    var filter = Builders<SavedRecord>.Filter.Eq(x => x.Id, id);

    await collection.ReplaceOneAsync(filter, updatedRecord);
    return Results.Ok(updatedRecord);
});

app.MapDelete("/api/savedrecords/{id}",
async (string id, IMongoCollection<SavedRecord> collection) =>
{
    var filter = Builders<SavedRecord>.Filter.Eq(x => x.Id, id);

    await collection.DeleteOneAsync(filter);
    return Results.Ok();
});

// Your Polished Saved Codes Endpoints
app.MapGet("/api/savedcodes", async (SavedCodeService savedCodeService) =>
{
    var codes = await savedCodeService.GetSavedCodesAsync();
    return Results.Ok(codes);
});

app.MapPost("/api/savedcodes/add", async (SavedCode newCode, SavedCodeService savedCodeService) =>
{
    if (string.IsNullOrWhiteSpace(newCode.CodeValue) || string.IsNullOrWhiteSpace(newCode.DiseaseName))
    {
        return Results.BadRequest("Both CodeValue and DiseaseName are required.");
    }

    var createdCode = await savedCodeService.AddSavedCodeAsync(newCode);
    return Results.Created($"/api/savedcodes/{createdCode.Id}", createdCode);
});

app.Run();