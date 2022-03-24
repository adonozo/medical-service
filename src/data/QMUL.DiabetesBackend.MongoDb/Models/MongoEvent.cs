namespace QMUL.DiabetesBackend.MongoDb.Models
{
    using System;
    using Model;
    using Model.Enums;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class MongoEvent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string PatientId { get; set; }
        
        public DateTime EventDateTime { get; set; }
        
        public bool ExactTimeIsSetup { get; set; }
        
        public CustomEventTiming EventTiming { get; set; }
        
        public ResourceReference ResourceReference { get; set; }
    }
}