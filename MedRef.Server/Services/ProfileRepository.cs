using MongoDB.Driver;
using MedRef.Shared.Models;
using System.Threading.Tasks;
using MedRef.Server.Data;

namespace MedRef.Server.Services
{
    public class ProfileRepository
    {
        private readonly MongoDbContext _context;

        public ProfileRepository(MongoDbContext context)
        {
            _context = context;
        }

        // Saves a new profile or completely updates an existing one if the Id matches
        public async Task SaveOrUpdateProfileAsync(UserProfile profile)
        {
            if (string.IsNullOrEmpty(profile.Id))
            {
                await _context.UserProfiles.InsertOneAsync(profile);
            }
            else
            {
                var filter = Builders<UserProfile>.Filter.Eq(p => p.Id, profile.Id);
                await _context.UserProfiles.ReplaceOneAsync(filter, profile, new ReplaceOptions { IsUpsert = true });
            }
        }

        public async Task<UserProfile> GetProfileByUserIdAsync(string userId)
        {
            var filter = Builders<UserProfile>.Filter.Eq(p => p.UserId, userId);
            return await _context.UserProfiles.Find(filter).FirstOrDefaultAsync();
        }
    }
}