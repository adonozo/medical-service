using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.MongoDb.Models;
using static System.Enum;

namespace QMUL.DiabetesBackend.MongoDb.Utils
{
    public static class ObservationMapper
    {
        public static MongoObservation ToMongoObservation(this Observation observation)
        {
            var hasQuantity = observation.Value is Quantity;
            var patientReference = new MongoReference
            {
                ReferenceId = observation.Subject.ElementId,
                ReferenceName = observation.Subject.Display
            }; 
            return new MongoObservation
            {
                Id = observation.Id,
                Status = observation.Status.ToString(),
                PatientReference = patientReference,
                Issued = observation.Issued?.UtcDateTime ?? DateTime.UtcNow,
                Code = observation.Code.Coding.Select(Mapper.ToMongoCode).ToList(),
                PerformerReferences = new List<MongoReference> { patientReference },
                ValueQuantity = hasQuantity ? ((Quantity)observation.Value).ToMongoQuantity() : new MongoQuantity()
            };
        }

        public static Observation ToObservation(this MongoObservation observation)
        {
            var hasStatus = TryParse<ObservationStatus>(observation.Status, out var status);

            return new Observation
            {
                Id = observation.Id,
                Status = hasStatus ? status : null,
                Issued = DateTime.SpecifyKind(observation.Issued, DateTimeKind.Utc),
                Subject = observation.PatientReference.ToResourceReference(),
                Performer = observation.PerformerReferences.Select(Mapper.ToResourceReference).ToList(),
                Code = new CodeableConcept
                {
                    Coding = observation.Code.Select(Mapper.ToCoding).ToList()
                },
                Value = observation.ValueQuantity.ToQuantity()
            };
        }
    }
}