namespace QMUL.DiabetesBackend.MongoDb.Models
{
    using System;
    using System.Collections.Generic;
    using Hl7.Fhir.Model;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class MongoPatient : Patient
    {
        public string Email { get; set; }
        
        public Dictionary<string, DateTimeOffset> ExactEventTimes { get; set; }
    }
}