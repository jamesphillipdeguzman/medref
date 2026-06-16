using Microsoft.AspNetCore.Mvc;
using MedRef.Shared.Models;
using MongoDB.Driver;
using System.Security.Claims;

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
    public async Task<IActionResult> GetSavedRecords()
    {
        var records = await _savedRecords.Find(_ => true).ToListAsync();
        return Ok(records);
    }

    [HttpPost]
    public async Task<IActionResult> SaveRecord([FromBody] SavedRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.Id)) record.Id = Guid.NewGuid().ToString();
        await _savedRecords.InsertOneAsync(record);
        return Ok(record);
    }

    [HttpPut("{id}")]
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
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _savedRecords.DeleteOneAsync(r => r.Id == id);
        return result.DeletedCount == 0 ? NotFound() : NoContent();
    }
}
