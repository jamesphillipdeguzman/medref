using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace MedRef.Client
{
    public class CustomCookieAuthenticationStateProvider : AuthenticationStateProvider
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private readonly AuthenticationState _anonymousSession = new(new ClaimsPrincipal(new ClaimsIdentity()));

        public CustomCookieAuthenticationStateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Add a cache-busting query parameter to ensure we always get fresh data from the server
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var url = $"api/auth/user?_={timestamp}";

                var response = await _httpClient.GetFromJsonAsync<UserSessionResult>(url, JsonOptions);

                if (response == null || !response.IsAuthenticated)
                {
                    return _anonymousSession;
                }

                var claims = response.Claims?
                    .Select(c => new Claim(c.Type, c.Value))
                    .ToList() ?? new List<Claim>();

                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch (Exception ex)
            {
                // Log the exception so you can see if the API call is failing
                Console.WriteLine($"Auth State Error: {ex.Message}");
                return _anonymousSession;
            }
        }

        public void NotifyUserAuthenticationChanged()
        {
            // Trigger the re-fetch
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }

    public class UserSessionResult
    {
        public bool IsAuthenticated { get; set; }
        public List<ClaimData>? Claims { get; set; }
    }

    public class ClaimData
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}