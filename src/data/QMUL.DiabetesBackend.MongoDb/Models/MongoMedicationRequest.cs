namespace QMUL.DiabetesBackend.MongoDb.Models
{
    using System;
    using System.Collections.Generic;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class MongoMedicationRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string Priority { get; set; }
        
        public string Status { get; set; }

        public string Note { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public MongoReference PatientReference { get; set; }
        
        public MongoReference MedicationReference { get; set; }
        
        public MongoReference RequesterReference { get; set; }
        
        public bool IsInsulin { get; set; }

        public IEnumerable<MongoDosageInstruction> DosageInstructions { get; set; }
    }
}