using MongoDB.Driver;
using MedRef.Shared.Models;
using System.Threading.Tasks;
using MedRef.Server.Data;

namespace MedRef.Server.Services
{
    // This repository manages user profiles, allowing for saving new profiles or updating existing ones based on the user's unique identifier. It interacts with the MongoDB collection of UserProfile to persist changes in user profile data.
    public class ProfileRepository
    {
        // The MongoDbContext is injected into the repository, providing access to the UserProfiles collection. This allows the repository to perform database operations such as inserting new profiles or replacing existing ones based on the user's unique identifier (UserId).
        private readonly MongoDbContext _context;
        // Constructor injection allows the MongoDbContext to be recognized and used within the repository methods for database interactions.
        public ProfileRepository(MongoDbContext context)
        {
            _context = context;
        }

        // Saves a new profile or completely updates an existing one if the Id matches
        public async Task SaveOrUpdateProfileAsync(UserProfile profile)
        {
            // If the profile doesn't have an Id, it's a new profile and we insert it. Otherwise, we replace the existing profile with the new data. The upsert option ensures that if the profile doesn't exist, it will be created.
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
        // Retrieves a user profile based on the user's unique identifier (UserId). It constructs a filter to find the profile document in the UserProfiles collection that matches the provided UserId and returns the first matching profile or null if no profile is found.
        public async Task<UserProfile> GetProfileByUserIdAsync(string userId)
        {
            // MongoDB doesn't have a direct .Where() method like Entity Framework, so we use the Find() method with a filter to retrieve the profile based on the UserId.
            var filter = Builders<UserProfile>.Filter.Eq(p => p.UserId, userId);
            return await _context.UserProfiles.Find(filter).FirstOrDefaultAsync();
        }
    }
}