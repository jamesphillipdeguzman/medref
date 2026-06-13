using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace MedRef.Client; // Make sure this matches your project namespace

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
            var response = await _httpClient.GetFromJsonAsync<UserSessionResult>("api/auth/user", JsonOptions);

            if (response == null || !response.IsAuthenticated)
            {
                return _anonymousSession;
            }

            var claims = response.Claims?
                .Select(c => new Claim(c.Type, c.Value))
                .ToList() ?? [];

            var identity = new ClaimsIdentity(claims, authenticationType: "CookieAuth");
            return new AuthenticationState(new ClaimsPrincipal(identity));
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