using System;
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
        
        /// <summary>
        /// A flag to generate event series based on this instruction. This would mean that the patient need to
        /// supply an exact time for an ambiguous event, e.g., breakfast, once a week, etc. 
        /// </summary>
        public bool RequiresSetup { get; set; }
        
        /// <summary>
        /// To keep track of when this instruction started and create events based on that. E.g., Dosage is once a week
        /// so, we take this date as start.
        /// </summary>
        public DateTime DosageStartDate { get; set; }
    }
}