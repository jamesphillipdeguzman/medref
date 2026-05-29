using System.Net.Http.Json;
using System.Text.Json;
using MedRef.Shared.Models;

namespace MedRef.Server.Services;

public interface IMedlineService
{
    Task<MedlineRoot?> GetMedlineDataAsync(string icdCode, CancellationToken cancellationToken = default);
}

public class MedlineService : IMedlineService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MedlineService> _logger;

    public MedlineService(IHttpClientFactory httpClientFactory, ILogger<MedlineService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<MedlineRoot?> GetMedlineDataAsync(string icdCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(icdCode))
            throw new ArgumentException("ICD code is required.", nameof(icdCode));

        try
        {
            var client = _httpClientFactory.CreateClient();

            // Construct the MedlinePlus Web Service URL with the provided ICD code
            string medlineUrl =
                $"https://connect.medlineplus.gov/service?mainSearchCriteria.v.cs=2.16.840.1.113883.6.90&mainSearchCriteria.v.c={Uri.EscapeDataString(icdCode)}&knowledgeResponseType=application/json";

            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (compatible; MedRefApp/1.0)");

            var response = await client.GetAsync(medlineUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("MedlinePlus API returned non-success status code {StatusCode} for ICD code {IcdCode}",
                    response.StatusCode, icdCode);
                return null;
            }

            var jsonPayload = await response.Content.ReadAsStringAsync(cancellationToken);


            var medlineData = await response.Content.ReadFromJsonAsync<MedlineRoot>(
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
    cancellationToken);

            return medlineData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data from MedlinePlus API for ICD code {IcdCode}", icdCode);
            return null;
        }
    }
}