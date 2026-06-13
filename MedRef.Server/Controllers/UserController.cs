using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using MedRef.Shared.Models; // This is the crucial missing piece
using System.Security.Claims;

namespace MedRef.Server.Controllers // Namespace is required
{
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

        [Authorize]
        [HttpPut("update-role")]
        public async Task<IActionResult> UpdateRole([FromBody] RoleUpdateModel model)
        {
            // 'User' now refers to the ControllerBase.User property
            var email = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var collection = _mongoClient.GetDatabase("MedRefDb").GetCollection<User>("Users");
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var update = Builders<User>.Update.Set(u => u.Role, model.Role);

            var result = await collection.UpdateOneAsync(filter, update);

            return Ok(new { message = "Role updated successfully" });
        }
    }
}