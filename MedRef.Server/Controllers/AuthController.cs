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
        // Challenges the browser using the Google OAuth registration scheme
        // and tells it to redirect back to the frontend homepage after successful login
        return Challenge(new AuthenticationProperties { RedirectUri = "http://localhost:5265/" }, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("http://localhost:5265/");
    }

    [HttpGet("user")]
    public IActionResult GetCurrentUser()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            // 1. Find the email claim or fall back to standard Name identifier
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value
                             ?? User.FindFirst(ClaimTypes.Name)?.Value
                             ?? "User";

            // 2. Clean up the email to pull just the username (e.g., jamesd@gmail.com -> jamesd)
            var username = emailClaim.Split('@')[0];

            // 3. Pass that specific claim down to your Blazor client
            var claims = new List<object>
        {
            new { Type = ClaimTypes.Name, Value = username }
        };

            return Ok(new { IsAuthenticated = true, Claims = claims });
        }

        return Ok(new { IsAuthenticated = false });
    }
}