using MongoDB.Driver;

namespace DAL
{
    public class MongoDbConnector
    {
        private static IMongoDatabase database;
        public static void Connect(string connectionString)
        {
            var client = new MongoClient(connectionString);
            database = client.GetDatabase("test");
        }

        public IMongoDatabase GetDatabase()
        {
            return database;
        }
    }
}
