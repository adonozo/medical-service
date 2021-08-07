using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using QMUL.DiabetesBackend.Model.Enums;

namespace QMUL.DiabetesBackend.MongoDb.Models
{
    public class MongoObservation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string Status { get; set; }
        
        public DateTime Issued { get; set; }

        public CustomEventTiming Timing { get; set; }
        
        public MongoReference PatientReference { get; set; }
        
        public List<MongoReference> PerformerReferences { get; set; }
        
        public List<MongoCode> Code { get; set; }
        
        public MongoQuantity ValueQuantity { get; set; }
    }
}