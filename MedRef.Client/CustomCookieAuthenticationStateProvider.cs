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
                // 1. Fetch data from the server FIRST
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var url = $"api/auth/user?_={timestamp}";

                var response = await _httpClient.GetFromJsonAsync<UserSessionResult>(url, JsonOptions);

                if (response == null || !response.IsAuthenticated)
                {
                    return _anonymousSession;
                }

                // 2. Populate the claims list now that we have data
                var claims = new List<Claim>();
                if (response.Claims != null)
                {
                    foreach (var c in response.Claims)
                    {
                        claims.Add(new Claim(c.Type, c.Value));

                        // Keep your existing Role-mapping logic here
                        if (c.Type.Equals("Role", StringComparison.OrdinalIgnoreCase) ||
                            c.Type.Equals(ClaimTypes.Role, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!claims.Any(existing => existing.Type == "Role" && existing.Value == c.Value))
                                claims.Add(new Claim("Role", c.Value));

                            if (!claims.Any(existing => existing.Type == ClaimTypes.Role && existing.Value == c.Value))
                                claims.Add(new Claim(ClaimTypes.Role, c.Value));
                        }
                    }
                }

                // 3. Construct the Identity and Principal using the populated claims
                var identity = new ClaimsIdentity(claims, "CookieAuth", ClaimTypes.Name, ClaimTypes.Role);
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch (Exception ex)
            {
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