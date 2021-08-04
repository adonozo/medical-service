using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace QMUL.DiabetesBackend.MongoDb.Models
{
    public class MongoServiceRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string Status { get; set; }
        
        public string Intent { get; set; }
        
        public string PatientInstruction { get; set; }
        
        public List<MongoCode> Code { get; set; }
        
        public MongoReference PatientReference { get; set; }
        
        public MongoTiming Occurrence { get; set; }
        
        public DateTime CreateAt { get; set; }
    }
}