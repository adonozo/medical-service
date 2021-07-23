using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.MongoDb.Models
{
    public class MongoHealthEvent : HealthEvent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public new string Id { get; set; }
    }
}