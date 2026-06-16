using Microsoft.AspNetCore.Mvc;
using MedRef.Shared.Models;
using MedRef.Server.Services;
using System.Security.Claims;

namespace MedRef.Server.Controllers
{
    // This controller manages user profiles, allowing users to view and update their profile information. It interacts with the ProfileRepository to persist profile data in MongoDB.
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly ProfileRepository _profileRepository;

        public ProfileController(ProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        // GET: api/profile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            // Pull user's identity info out of their secure session claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _profileRepository.GetProfileByUserIdAsync(userId);
            if (profile == null) return NotFound("Profile not initialized yet.");

            return Ok(profile);
        }

        // PUT: api/profile/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfile updatedProfile)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || updatedProfile.UserId != userId) return Unauthorized();

            updatedProfile.UpdatedAt = DateTime.UtcNow;
            await _profileRepository.SaveOrUpdateProfileAsync(updatedProfile);
            return Ok(updatedProfile);
        }
    }
}