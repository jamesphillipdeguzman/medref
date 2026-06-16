using MongoDB.Driver;
using MedRef.Shared.Models;

namespace MedRef.Server.Services
{
    public class SavedCodeService
    {
        private readonly IMongoCollection<SavedCode> _savedCodesCollection;

        public SavedCodeService(IMongoDatabase mongoDatabase)
        {
            _savedCodesCollection = mongoDatabase.GetCollection<SavedCode>("SavedCodes");
        }
        public async Task<List<SavedCode>> GetSavedCodesAsync()
        {
            return await _savedCodesCollection.Find(_ => true).ToListAsync();
        }

        public async Task<SavedCode?> AddSavedCodeAsync(SavedCode code)
        {
            // Now that SavedCode has UserId and CodeValue, this will work perfectly!
            var existing = await _savedCodesCollection
                .Find(c => c.CodeValue == code.CodeValue && c.UserId == code.UserId)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                return null; // Duplicate prevented
            }

            await _savedCodesCollection.InsertOneAsync(code);
            return code;
        }
    }
}