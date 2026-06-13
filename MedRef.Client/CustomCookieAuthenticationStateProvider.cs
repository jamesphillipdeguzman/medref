using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace MedRef.Client;

public class CustomCookieAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;

    public CustomCookieAuthenticationStateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // 2. REPLACE GETFROMJSONASYNC WITH A MANUAL REQUEST LAYER:
            var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/user");

            // This forces the browser sandbox to pass your secure authentication cookies along
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var userInfo = await response.Content.ReadFromJsonAsync<UserDto>();

                if (userInfo != null && userInfo.IsAuthenticated)
                {
                    var claims = userInfo.Claims.Select(c => new Claim(c.Type, c.Value));
                    var identity = new ClaimsIdentity(claims, "CookieAuth");
                    return new AuthenticationState(new ClaimsPrincipal(identity));
                }
            }
        }
        catch
        {
            // If the server can't be reached or the session cookie is expired/absent
        }

        // Return an empty, unauthenticated principal state
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    private class UserDto
    {
        public bool IsAuthenticated { get; set; }
        public List<ClaimDto> Claims { get; set; } = new();
    }

    private class ClaimDto
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}