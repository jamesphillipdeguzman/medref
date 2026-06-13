using MedRef.Server.Services;
using MongoDB.Driver;
using MedRef.Server.Configurations;
using MedRef.Shared.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// MONGODB CONFIGURATION & SERVICE PATTERNS
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

// =========================================================================
// GOOGLE OAUTH 2.0 & BFF COOKIE CONFIGURATION
// =========================================================================

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "__Host-MedRef-BFF";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
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
    });

builder.Services.AddAuthorization();

// =========================================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IMedlineService, MedlineService>();

builder.Services.AddHttpClient("MedlinePlus", client =>
{
    client.BaseAddress = new Uri("https://connect.medlineplus.gov/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// CORS configuration to allow requests from Netlify frontend and local development
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowNetlify");

// CRITICAL PIPELINE ORDER: Authenticate identity BEFORE executing endpoints
app.UseAuthentication();
app.UseAuthorization();

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

// James' Polished Saved Codes Endpoints
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

// CRITICAL FIX LOCATION: Must be explicitly declared right before app.Run()
app.MapControllers();

app.Run();
