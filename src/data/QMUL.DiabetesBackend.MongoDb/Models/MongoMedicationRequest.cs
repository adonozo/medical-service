using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace QMUL.DiabetesBackend.MongoDb.Models
{
    public class MongoMedicationRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string Priority { get; set; }
        
        public MongoReference ReferencePatient { get; set; }
        
        public MongoReference MedicationReference { get; set; }
        
        public MongoReference RequesterReference { get; set; }
        
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public string Note { get; set; }
        
        public IEnumerable<MongoDosageInstruction> DosageInstructions { get; set; }
    }
}