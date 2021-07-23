using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace QMUL.DiabetesBackend.MongoDb.Models
{
    public class MongoDosageInstruction
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public int Sequence { get; set; }

        public string Text { get; set; }

        public MongoTiming Timing { get; set; }

        public IEnumerable<MongoQuantity> DoseAndRate { get; set; }
    }
}