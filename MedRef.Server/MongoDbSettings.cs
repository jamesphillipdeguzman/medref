namespace MedRef.Server.Configurations // Match this namespace to where you put the file
{
    // The MongoDbSettings class is a simple configuration class that holds the necessary settings for connecting to a MongoDB database. It includes properties for the connection string and the database name, which are typically read from the appsettings.json file or environment variables. This class is used to centralize MongoDB configuration and make it easy to inject these settings into services that require database access.   
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }
}