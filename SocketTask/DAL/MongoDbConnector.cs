using MongoDB.Driver;

namespace DAL
{
    public class MongoDbConnector
    {
        private static IMongoDatabase database;
        public static void Connect(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var con = new MongoUrlBuilder(connectionString);
            database = client.GetDatabase(con.DatabaseName);
        }

        public IMongoDatabase GetDatabase()
        {
            return database;
        }
    }
}
