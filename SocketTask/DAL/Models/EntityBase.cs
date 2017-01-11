using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL.Models
{
    public class EntityBase
    {
        [BsonId]
        public string Id { get; set; }
//        [BsonIgnore]
//        public static string EntityName { get; set; }
    }
}
