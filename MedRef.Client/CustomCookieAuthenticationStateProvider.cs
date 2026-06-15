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

                var claims = new List<Claim>();

                if (response.Claims != null)
                {
                    foreach (var c in response.Claims)
                    {
                        // Map the base claim coming from the API
                        claims.Add(new Claim(c.Type, c.Value));

                        // FIX: If the claim type matches "Role" or the .NET long-form role claim URI,
                        // duplicate it to ensure both standard literal string matching and native .NET IsInRole() function checks pass.
                        if (c.Type.Equals("Role", StringComparison.OrdinalIgnoreCase) ||
                            c.Type.Equals(ClaimTypes.Role, StringComparison.OrdinalIgnoreCase))
                        {
                            // Add literal string fallback "Role"
                            if (!claims.Any(existing => existing.Type == "Role" && existing.Value == c.Value))
                            {
                                claims.Add(new Claim("Role", c.Value));
                            }

                            // Add official .NET long-form type fallback "http://schemas.microsoft.com/..."
                            if (!claims.Any(existing => existing.Type == ClaimTypes.Role && existing.Value == c.Value))
                            {
                                claims.Add(new Claim(ClaimTypes.Role, c.Value));
                            }
                        }
                    }
                }

                // Explicitly provide the claim mapping structure type so IsInRole() checks evaluate correctly
                var identity = new ClaimsIdentity(claims, "CookieAuth", ClaimTypes.Name, ClaimTypes.Role);
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