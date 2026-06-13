using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedRef.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpGet("login")]
    public IActionResult Login()
    {
        string frontendOrigin = GetFrontendOrigin();

        // Dynamically tell Google to redirect back to the true calling frontend
        var props = new AuthenticationProperties
        {
            RedirectUri = frontendOrigin
        };

        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        string frontendOrigin = GetFrontendOrigin();
        return Redirect(frontendOrigin);
    }

    [HttpGet("user")]
    public IActionResult GetCurrentUser()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value
                             ?? User.FindFirst(ClaimTypes.Name)?.Value
                             ?? "User";

            var username = emailClaim.Split('@')[0];

            var claims = new List<object>
            {
                new { Type = ClaimTypes.Name, Value = username }
            };

            return Ok(new { IsAuthenticated = true, Claims = claims });
        }

        return Ok(new { IsAuthenticated = false });
    }

    /// <summary>
    /// Helper method to safely isolate production Netlify URLs from local development ports
    /// </summary>
    private string GetFrontendOrigin()
    {
        // Use only the origin (scheme + host), never the full referrer path.
        // Returning /authentication/login caused an OAuth redirect loop.
        string referrer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referrer) && Uri.TryCreate(referrer, UriKind.Absolute, out var referrerUri))
        {
            return $"{referrerUri.Scheme}://{referrerUri.Authority}/";
        }

        string origin = Request.Headers["Origin"].ToString();
        if (!string.IsNullOrEmpty(origin) && Uri.TryCreate(origin, UriKind.Absolute, out var originUri))
        {
            return $"{originUri.Scheme}://{originUri.Authority}/";
        }

        bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        return isDevelopment
            ? "http://localhost:5265/"
            : "https://medreftool.netlify.app/";
    }
}