using Microsoft.AspNetCore.Mvc;
using MedRef.Shared.Models;
using MongoDB.Driver;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace MedRef.Server.Controllers;

[ApiController]
[Route("api/saved-records")] // Matches your frontend calls
public class SavedRecordsController : ControllerBase
{
    private readonly IMongoCollection<SavedRecord> _savedRecords;

    // Use the collection injected from Program.cs
    public SavedRecordsController(IMongoCollection<SavedRecord> savedRecords)
    {
        _savedRecords = savedRecords;
    }

    [HttpGet]
    [Authorize] // Ensure only authenticated users can access their saved records
    public async Task<IActionResult> GetSavedRecords()
    {
        // 1. Get the current user's ID
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // 2. Use the _savedRecords collection injected in the constructor
        // Since MongoDB collections don't use .Where() like Entity Framework, 
        // you use Find() with a filter:
        var userRecords = await _savedRecords.Find(r => r.UserId == userId).ToListAsync();

        return Ok(userRecords);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SaveRecord([FromBody] SavedRecord record)
    {
        // 1. Extract the ID
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // 2. Explicitly handle the null case
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User identity could not be verified.");
        }

        // 3. Assign the validated ID
        record.UserId = userId;

        if (string.IsNullOrWhiteSpace(record.Id))
            record.Id = Guid.NewGuid().ToString();

        await _savedRecords.InsertOneAsync(record);
        return Ok(record);
    }
    [HttpPut("{id}")]
    [Authorize] // Ensure only authenticated users can update their records
    public async Task<IActionResult> UpdateRecord(string id, [FromBody] SavedRecord record)
    {
        // Ensure the ID in the URL matches the object being sent
        if (id != record.Id) return BadRequest("ID mismatch");

        // Replace the existing document in MongoDB
        var result = await _savedRecords.ReplaceOneAsync(r => r.Id == id, record);

        // Return 204 No Content on success, or 404 if the record wasn't found
        return result.MatchedCount == 0 ? NotFound() : NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize] // Ensure only authenticated users can delete their records
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _savedRecords.DeleteOneAsync(r => r.Id == id);
        return result.DeletedCount == 0 ? NotFound() : NoContent();
    }
}
