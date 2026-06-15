using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MedRef.Shared.Models;

namespace MedRef.Server.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            // Changed from dr_meet_db to your team project database name
            _database = client.GetDatabase("MedRefDb");
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        public IMongoCollection<UserProfile> UserProfiles => _database.GetCollection<UserProfile>("user_profiles");
        public IMongoCollection<SavedCode> SavedCodes => _database.GetCollection<SavedCode>("saved_codes");
    }
}