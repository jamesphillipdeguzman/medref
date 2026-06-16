using System.Text.Json;
using MedRef.Shared.Models;

namespace MedRef.Server.Services;
// This class provides functionality for retrieving medical information from the MedlinePlus API.
public interface IMedlineService
{
    Task<MedlineRoot?> GetMedlineDataAsync(string icdCode, CancellationToken cancellationToken = default);
}
// The MedlineService implements the IMedlineService interface and uses an HttpClient to call the MedlinePlus API. It handles the construction of the API request, error handling, and deserialization of the response into a MedlineRoot object.
public class MedlineService : IMedlineService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MedlineService> _logger;
    // The constructor takes an IHttpClientFactory for creating HttpClient instances and an ILogger for logging information and errors related to the API calls.
    public MedlineService(IHttpClientFactory httpClientFactory, ILogger<MedlineService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    // The GetMedlineDataAsync method takes an ICD code as input, constructs the appropriate API request, and returns a MedlineRoot object containing the response data. It includes error handling for unsuccessful responses and exceptions that may occur during the API call.
    public async Task<MedlineRoot?> GetMedlineDataAsync(string icdCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(icdCode))
            return null;
        // The method constructs the API request URL using the provided ICD code and logs the request. It then sends the request to the MedlinePlus API and checks for a successful response. If the response is successful, it reads the JSON content and deserializes it into a MedlineRoot object, which is returned to the caller. If any errors occur during this process, they are logged, and null is returned.
        try
        {
            var client = _httpClientFactory.CreateClient("MedlinePlus");
            // Construct the API request URL with the appropriate query parameters for the ICD code and response format
            var url =
                $"service?mainSearchCriteria.v.cs=2.16.840.1.113883.6.90" +
                $"&mainSearchCriteria.v.c={Uri.EscapeDataString(icdCode)}" +
                $"&knowledgeResponseType=application/json";
            // Log the API call for debugging purposes
            _logger.LogInformation("Calling MedlinePlus for {Code}", icdCode);
            // Send the GET request to the MedlinePlus API and await the response
            var response = await client.GetAsync(url, cancellationToken);
            // Check if the response indicates a successful status code (2xx). If not, log a warning and return null.
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("MedlinePlus failed: {StatusCode}", response.StatusCode);
                return null;
            }
            // Read the response content as a string and check if it is empty. If it is empty, log a warning and return null.
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            // If the response content is empty, log a warning and return null
            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogWarning("Empty MedlinePlus response for {Code}", icdCode);
                return null;
            }
            // Deserialize the JSON response into a MedlineRoot object and return it
            return JsonSerializer.Deserialize<MedlineRoot>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        //  Handle specific exceptions such as TaskCanceledException for timeouts and log the error details. For any other unexpected exceptions, log the error and return null.
        catch (TaskCanceledException)
        {
            _logger.LogError("MedlinePlus request timed out for {Code}", icdCode);
            return null;
        }
        //  Catch any other exceptions that may occur during the API call, log the error details, and return null to indicate that the data could not be retrieved.
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error for {Code}", icdCode);
            return null;
        }
    }
}