using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using MedRef.Shared.Models; // This is the crucial missing piece
using System.Security.Claims;

namespace MedRef.Server.Controllers // Namespace is required
{
    // This controller manages user-related operations, such as updating user roles. It interacts with the MongoDB collection of User to persist changes in user data.
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase // Must inherit from ControllerBase
    {
        private readonly IMongoClient _mongoClient;

        // Constructor injection allows _mongoClient to be recognized
        public UserController(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
        }
        // This endpoint allows an authenticated user to update their role. It retrieves the user's email from the authentication claims, then updates the corresponding User document in MongoDB with the new role provided in the request body.
        [Authorize]
        [HttpPut("update-role")]
        public async Task<IActionResult> UpdateRole([FromBody] RoleUpdateModel model)
        {
            // 'User' now refers to the ControllerBase.User property
            var email = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email)) return Unauthorized();
            // Access the Users collection and update the role for the user with the matching email
            var collection = _mongoClient.GetDatabase("MedRefDb").GetCollection<User>("Users");
            // Use MongoDB's Builders to create a filter and update definition
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            // Update the Role field of the matched user document
            var update = Builders<User>.Update.Set(u => u.Role, model.Role);
            // Execute the update operation
            var result = await collection.UpdateOneAsync(filter, update);
            // Check if the update was successful and return an appropriate response
            return Ok(new { message = "Role updated successfully" });
        }
    }
}