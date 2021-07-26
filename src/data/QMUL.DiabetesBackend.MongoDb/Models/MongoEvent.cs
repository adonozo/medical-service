using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.Model.Enums;

namespace QMUL.DiabetesBackend.MongoDb.Models
{
    public class MongoEvent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string PatientId { get; set; }
        
        public DateTime EventDateTime { get; set; }
        
        public bool ExactTimeIsSetup { get; set; }
        
        public CustomEventTiming EventTiming { get; set; }
        
        public CustomResource Resource { get; set; }
    }
}