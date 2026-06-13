using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MongoDB.Driver;
using MedRef.Shared.Models;

namespace MedRef.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMongoCollection<User> _users;

    public AuthController(IMongoCollection<User> users)
    {
        _users = users;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var props = new AuthenticationProperties { RedirectUri = GetFrontendOrigin() };
        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect(GetFrontendOrigin());
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value
                            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(email)) return Ok(new { IsAuthenticated = false });

                var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

                // If user doesn't exist in DB yet, create a default record
                if (user == null)
                {
                    user = new User { Email = email, Role = "Unassigned" };
                    await _users.InsertOneAsync(user);
                }

                var username = email.Split('@')[0];

                var claims = new List<object>
                {
                    new { Type = ClaimTypes.Name, Value = username },
                    new { Type = "Role", Value = user.Role ?? "Unassigned" }
                };

                return Ok(new { IsAuthenticated = true, Claims = claims });
            }
            return Ok(new { IsAuthenticated = false });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetCurrentUser: {ex.Message}");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPut("update-role")]
    public async Task<IActionResult> UpdateRole([FromBody] RoleUpdateRequest request)
    {
        // 1. Validation check
        if (request == null || string.IsNullOrWhiteSpace(request.Role))
        {
            return BadRequest("Invalid role data provided.");
        }

        var email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(email)) return Unauthorized();

        // 2. Perform the update
        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        var update = Builders<User>.Update.Set(u => u.Role, request.Role);

        // 3. Use IsUpsert if you want to ensure the record exists
        var options = new UpdateOptions { IsUpsert = true };
        var result = await _users.UpdateOneAsync(filter, update, options);

        return result.IsAcknowledged ? Ok() : BadRequest("Role update failed.");
    }

    private string GetFrontendOrigin()
    {
        string referrer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referrer) && Uri.TryCreate(referrer, UriKind.Absolute, out var referrerUri))
        {
            return $"{referrerUri.Scheme}://{referrerUri.Authority}/";
        }

        bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        return isDevelopment ? "http://localhost:5265/" : "https://medreftool.netlify.app/";
    }
}

public class RoleUpdateRequest
{
    public string Role { get; set; } = string.Empty;
}