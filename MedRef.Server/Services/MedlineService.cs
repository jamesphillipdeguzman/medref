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
            return null;

        try
        {
            var client = _httpClientFactory.CreateClient("MedlinePlus");

            var url =
                $"service?mainSearchCriteria.v.cs=2.16.840.1.113883.6.90" +
                $"&mainSearchCriteria.v.c={Uri.EscapeDataString(icdCode)}" +
                $"&knowledgeResponseType=application/json";

            _logger.LogInformation("Calling MedlinePlus for {Code}", icdCode);

            var response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("MedlinePlus failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogWarning("Empty MedlinePlus response for {Code}", icdCode);
                return null;
            }

            return JsonSerializer.Deserialize<MedlineRoot>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("MedlinePlus request timed out for {Code}", icdCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error for {Code}", icdCode);
            return null;
        }
    }
}