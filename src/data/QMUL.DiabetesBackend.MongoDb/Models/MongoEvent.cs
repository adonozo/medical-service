// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace QMUL.DiabetesBackend.MongoDb.Models
{
    using System;
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

        [BsonRepresentation(BsonType.String)]
        public CustomEventTiming EventTiming { get; set; }

        public MongoResourceReference ResourceReference { get; set; }

        public class MongoResourceReference
        {
            [BsonRepresentation(BsonType.String)]
            public EventType EventType { get; set; }

            [BsonRepresentation(BsonType.ObjectId)]
            public string ResourceId { get; set; }

            [BsonRepresentation(BsonType.ObjectId)]
            public string EventReferenceId { get; set; }

            public string Text { get; set; }

            [BsonRepresentation(BsonType.DateTime)]
            public DateTimeOffset? StartDate { get; set; }
        }
    }
}