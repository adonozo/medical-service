namespace QMUL.DiabetesBackend.MongoDb.Utils
{
    using Model;
    using Models;

    /// <summary>
    /// Maps FHIR objects into custom Mongo objects and vice-versa.
    /// </summary>
    public static class Mapper
    {
        // TODO use mapper instead or stop using MongoEvent
        public static MongoEvent ToMongoEvent(this HealthEvent healthEvent)
        {
            return new()
            {
                Id = healthEvent.Id,
                PatientId = healthEvent.PatientId,
                EventDateTime = healthEvent.EventDateTime,
                EventTiming = healthEvent.EventTiming,
                ExactTimeIsSetup = healthEvent.ExactTimeIsSetup,
                ResourceReference = healthEvent.ResourceReference
            };
        }

        public static HealthEvent ToHealthEvent(this MongoEvent mongoEvent)
        {
            return new()
            {
                Id = mongoEvent.Id,
                PatientId = mongoEvent.PatientId,
                EventDateTime = mongoEvent.EventDateTime,
                EventTiming = mongoEvent.EventTiming,
                ExactTimeIsSetup = mongoEvent.ExactTimeIsSetup,
                ResourceReference = mongoEvent.ResourceReference
            };
        }
    }
}