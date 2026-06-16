using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MongoDB.Driver;
using MedRef.Shared.Models;
using MedRef.Server.Services; // Ensure this matches your ProfileRepository path

namespace MedRef.Server.Controllers;

// This controller handles user authentication and role management, including dynamic profile schema creation based on user roles.

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMongoCollection<User> _users;
    private readonly ProfileRepository _profileRepository; // Injected to manage Nurse/Patient metadata profiles

    public AuthController(IMongoCollection<User> users, ProfileRepository profileRepository)
    {
        _users = users;
        _profileRepository = profileRepository;
    }
    // This endpoint initiates the Google authentication process. It sets the redirect URI to the frontend application, allowing users to be redirected back after successful authentication.
    [HttpGet("login")]
    public IActionResult Login()
    {
        var props = new AuthenticationProperties { RedirectUri = GetFrontendOrigin() };
        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }
    // This endpoint logs the user out by signing them out of the cookie authentication scheme and then redirects them back to the frontend application.
    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect(GetFrontendOrigin());
    }
    // This endpoint retrieves the current authenticated user's information, including their email and role. If the user is authenticated but does not have a corresponding record in the database, it creates a new user record with a default "Unassigned" role. The response includes the user's authentication status and claims for use on the frontend.
    [HttpGet("user")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {   // Check if the user is authenticated
            if (User.Identity?.IsAuthenticated ?? false)
            {
                // Extract the user's email from the authentication claims
                var email = User.FindFirst(ClaimTypes.Email)?.Value
                            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                // If email is not found in claims, return an unauthenticated response
                if (string.IsNullOrEmpty(email)) return Ok(new { IsAuthenticated = false });
                // Check if the user already exists in the database
                var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

                // If user doesn't exist in DB yet, create a default identity record
                if (user == null)
                {
                    user = new User { Email = email, Role = "Unassigned" };
                    await _users.InsertOneAsync(user);
                }
                // Extract the username from the email for display purposes
                var username = email.Split('@')[0];
                // Build a list of claims to return to the frontend, including the user's name and role
                var claims = new List<object>
                {
                    new { Type = ClaimTypes.Name, Value = username },
                    new { Type = "Role", Value = user.Role ?? "Unassigned" }
                };
                // Return the authentication status and claims to the frontend
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
    // This endpoint allows an authenticated user to update their role. It retrieves the user's email from the authentication claims, then updates the corresponding User document in MongoDB with the new role provided in the request body. If the user does not have an existing profile, it creates a new profile with schema fields relevant to their new role (e.g., Nurse or Patient) and saves it to MongoDB.
    [HttpPut("update-role")]
    public async Task<IActionResult> UpdateRole([FromBody] RoleUpdateRequest request)
    {
        // 1. Validation check
        if (request == null || string.IsNullOrWhiteSpace(request.Role))
        {
            return BadRequest("Invalid role data provided.");
        }
        // 2. Extract the user's email from the authentication claims to identify their record in the database
        var email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // If email is not found in claims, return an unauthorized response
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        // Fetch the user record from the database to ensure it exists before updating
        var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        if (user == null)
        {
            return NotFound("User record could not be found to associate profile settings.");
        }

        // 3. Perform the authorization role update on the identity record
        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        var update = Builders<User>.Update.Set(u => u.Role, request.Role);
        var options = new UpdateOptions { IsUpsert = true };
        var result = await _users.UpdateOneAsync(filter, update, options);
        // Check if the update was acknowledged by MongoDB
        if (!result.IsAcknowledged)
        {
            return BadRequest("Identity database role assignment failed.");
        }

        // 4. Double check if this user already has a metadata layout created to prevent overwrites
        var existingProfile = await _profileRepository.GetProfileByUserIdAsync(user.Id!);
        if (existingProfile == null)
        {
            // Build the fresh schema base layer
            var newProfile = new UserProfile
            {
                UserId = user.Id!,
                FullName = User.Identity?.Name ?? "", // Grab name from cookie if available
                PhoneNumber = "",
                UpdatedAt = DateTime.UtcNow
            };

            // 5. Inject distinct baseline schema definitions depending on user choice
            if (request.Role == "Nurse")
            {
                newProfile.ProfileData.Add("LicenseNumber", "");
                newProfile.ProfileData.Add("Department", "");
                newProfile.ProfileData.Add("Shift", "Day");
            }
            else if (request.Role == "Patient")
            {
                newProfile.ProfileData.Add("EmergencyContactName", "");
                newProfile.ProfileData.Add("EmergencyContactPhone", "");
                newProfile.ProfileData.Add("BloodType", "Unknown");
            }

            // 6. Persist our concrete metadata model structure directly out to MongoDB
            await _profileRepository.SaveOrUpdateProfileAsync(newProfile);
        }

        return Ok();
    }
    //  This helper method extracts the frontend application's origin from the Referer header of the incoming request. It is used to set the redirect URI for authentication and logout operations, ensuring that users are redirected back to the correct frontend URL after these actions. If the Referer header is not present or cannot be parsed, it falls back to a default URL based on the environment (development or production).
    private string GetFrontendOrigin()
    {
        // Attempt to extract the frontend origin from the Referer header for dynamic redirect URI configuration
        string referrer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referrer) && Uri.TryCreate(referrer, UriKind.Absolute, out var referrerUri))
        {
            return $"{referrerUri.Scheme}://{referrerUri.Authority}/";
        }
        // Fallback to environment-based defaults if Referer is not available or valid
        bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        return isDevelopment ? "http://localhost:5265/" : "https://medreftool.netlify.app/";
    }
}
// This class represents the structure of the request body for updating a user's role. It contains a single property, Role, which is expected to be provided in the JSON payload when making a PUT request to the /update-role endpoint of the AuthController.
public class RoleUpdateRequest
{
    public string Role { get; set; } = string.Empty;
}