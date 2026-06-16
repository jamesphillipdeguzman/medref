using MongoDB.Driver;
using MedRef.Shared.Models;

namespace MedRef.Server.Services
{
    // This service manages the saved codes for users, allowing them to save and retrieve medical codes along with associated information. It interacts with the MongoDB collection of SavedCode to persist this data.
    public class SavedCodeService
    {
        // The IMongoCollection<SavedCode> is injected into the service, providing access to the SavedCodes collection in MongoDB. This allows the service to perform database operations such as inserting new saved codes or retrieving existing ones based on user identifiers.
        private readonly IMongoCollection<SavedCode> _savedCodesCollection;
        // Constructor injection allows the IMongoCollection<SavedCode> to be recognized and used within the service methods for database interactions.
        public SavedCodeService(IMongoDatabase mongoDatabase)
        {
            _savedCodesCollection = mongoDatabase.GetCollection<SavedCode>("SavedCodes");
        }
        // Retrieves a list of saved codes for all users. In a real application, you would likely want to filter this by user ID to return only the codes saved by the authenticated user. For simplicity, this method currently returns all saved codes in the collection.
        public async Task<List<SavedCode>> GetSavedCodesAsync()
        {
            return await _savedCodesCollection.Find(_ => true).ToListAsync();
        }
        // Adds a new saved code for a user. It first checks if the code already exists for the user to prevent duplicates. If the code is unique, it inserts it into the MongoDB collection and returns the saved code. If a duplicate is found, it returns null to indicate that the code was not added.
        public async Task<SavedCode?> AddSavedCodeAsync(SavedCode code)
        {
            // Now that SavedCode has UserId and CodeValue, this will work perfectly!
            var existing = await _savedCodesCollection
                .Find(c => c.CodeValue == code.CodeValue && c.UserId == code.UserId)
                .FirstOrDefaultAsync();
            // If a duplicate is found, return null to indicate the code was not added
            if (existing != null)
            {
                return null; // Duplicate prevented
            }
            // If no duplicate is found, insert the new saved code into the collection
            await _savedCodesCollection.InsertOneAsync(code);
            return code;
        }
    }
}