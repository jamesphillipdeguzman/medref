using Microsoft.AspNetCore.Mvc;
using MedRef.Shared.Models;
using MongoDB.Driver;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace MedRef.Server.Controllers;
// This controller manages the CRUD operations for users' saved medical records, ensuring that each user can only access and modify their own records. It interacts with the MongoDB collection of SavedRecord to persist data.
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

    // GET: api/saved-records
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
    // POST: api/saved-records
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SaveRecord([FromBody] SavedRecord record)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        // CRITICAL: Check if this user already has this specific code saved
        var exists = await _savedRecords.Find(r => r.UserId == userId && r.Code == record.Code).AnyAsync();

        if (exists)
        {
            // Return 409 Conflict if found
            return Conflict("This code is already in your favorites list!");
        }

        record.UserId = userId;
        record.Id = Guid.NewGuid().ToString(); // Or let MongoDB generate it
        await _savedRecords.InsertOneAsync(record);
        return Ok(record);
    }
    // PUT: api/saved-records/{id}
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateRecord(string id, [FromBody] SavedRecord record)
    {
        if (id != record.Id) return BadRequest("ID mismatch");

        // 1. Get the current user ID
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // 2. Fetch the current version from the database first
        var existingRecord = await _savedRecords.Find(r => r.Id == id).FirstOrDefaultAsync();

        // 3. Security: Check if the record exists and if it belongs to this user
        if (existingRecord == null) return NotFound();
        if (existingRecord.UserId != userId) return Forbid(); // Blocks unauthorized edits

        // 4. Force the UserId to remain what it was originally
        record.UserId = existingRecord.UserId;

        // 5. Replace with the record that now has the correct, preserved UserId
        var result = await _savedRecords.ReplaceOneAsync(r => r.Id == id, record);

        return result.MatchedCount == 0 ? NotFound() : NoContent();
    }
    // DELETE: api/saved-records/{id}
    [HttpDelete("{id}")]
    [Authorize] // Ensure only authenticated users can delete their records
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _savedRecords.DeleteOneAsync(r => r.Id == id);
        return result.DeletedCount == 0 ? NotFound() : NoContent();
    }
}
