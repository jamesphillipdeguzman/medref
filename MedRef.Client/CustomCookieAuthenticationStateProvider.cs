using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace MedRef.Client; // Make sure this matches your project namespace

public class CustomCookieAuthenticationStateProvider : AuthenticationStateProvider
{
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
            // Calls your AuthController via the Netlify proxy path
            var response = await _httpClient.GetFromJsonAsync<UserSessionResult>("api/auth/user");

            if (response == null || !response.IsAuthenticated)
            {
                return _anonymousSession;
            }

            // Using a standard placeholder string for the production auth type
            var identity = new ClaimsIdentity("CookieAuth");

            if (response.Claims != null)
            {
                foreach (var claim in response.Claims)
                {
                    identity.AddClaim(new Claim(claim.Type, claim.Value));
                }
            }

            var principal = new ClaimsPrincipal(identity);
            return new AuthenticationState(principal);
        }
        catch
        {
            return _anonymousSession;
        }
    }

    // =========================================================================
    // ADD THIS MISSING METHOD HERE TO FIX THE COMPILER ERROR
    // =========================================================================
    public void VerifyUserSessionExplicitly()
    {
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