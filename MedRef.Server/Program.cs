var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enforce HTTPS and serve static files (if needed for any frontend assets)
// app.UseHttpsRedirection();

// app.UseStaticFiles();

app.UseRouting();

// Define the API endpoint for proxying requests to the MedlinePlus Web Service
app.MapGet("/api/medlineproxy",
    async (string code, IHttpClientFactory httpClientFactory) =>
{
    if (string.IsNullOrWhiteSpace(code))
        return Results.BadRequest("Code is required.");

    try
    {
        var client = httpClientFactory.CreateClient();

        // Construct the MedlinePlus Web Service URL with the provided code
        string medlineUrl =
            $"https://connect.medlineplus.gov/service?mainSearchCriteria.v.cs=2.16.840.1.113883.6.90&mainSearchCriteria.v.c={Uri.EscapeDataString(code)}&knowledgeResponseType=application/json";

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (compatible; MedRefApp/1.0)");

        var response = await client.GetAsync(medlineUrl);

        if (!response.IsSuccessStatusCode)
            return Results.StatusCode((int)response.StatusCode);

        var jsonPayload = await response.Content.ReadAsStringAsync();

        return Results.Content(jsonPayload, "application/json");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Proxy error: {ex.Message}");
    }
});


app.Run();