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
            // We use the direct configuration key path to match your Render Environment Variables 
            // defined as "MongoDbSettings__ConnectionString"
            var connectionString = configuration["MongoDbSettings:ConnectionString"];

            // Diagnostic check: If this is null, it means the app isn't reading the 
            // Render environment variables correctly.
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("ConnectionString is missing! Check your Render environment variables.");
            }

            var client = new MongoClient(connectionString);

            // Changed from dr_meet_db to your team project database name
            // We can also pull the DB name from config, or default to "MedRefDb"
            var databaseName = configuration["MongoDbSettings:DatabaseName"] ?? "MedRefDb";
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        public IMongoCollection<UserProfile> UserProfiles => _database.GetCollection<UserProfile>("user_profiles");
        public IMongoCollection<SavedCode> SavedCodes => _database.GetCollection<SavedCode>("saved_codes");
    }
}