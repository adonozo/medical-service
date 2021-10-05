namespace QMUL.DiabetesBackend.MongoDb.Utils
{
    using System.Linq;
    using Hl7.Fhir.Model;
    using Models;
    using static System.Enum;

    /// <summary>
    /// Maps FHIR Service Request objects into custom Mongo objects and vice-versa.
    /// </summary>
    public static class ServiceRequestMapper
    {
        public static MongoServiceRequest ToMongoServiceRequest(this ServiceRequest request)
        {
            var mongoRequest = new MongoServiceRequest
            {
                Id = request.Id,
                Status = request.Status.ToString(),
                Intent = request.Intent.ToString(),
                PatientInstruction = request.PatientInstruction,
                PatientReference = new MongoReference
                {
                    ReferenceId = request.Subject.ElementId,
                    ReferenceName = request.Subject.Display
                },
                Code = request.Code.Coding.Select(Mapper.ToMongoCode).ToList(),
            };

            if (request.Occurrence is Timing timing)
            {
                mongoRequest.Occurrence = timing.ToMongoTiming();
            }

            return mongoRequest;
        }

        public static ServiceRequest ToServiceRequest(this MongoServiceRequest request)
        {
            var hasStatus = TryParse<RequestStatus>(request.Status, out var status);
            var hasIntent = TryParse<RequestIntent>(request.Intent, out var intent);
            
            return new ServiceRequest
            {
                Id = request.Id,
                Status = hasStatus ? status : null,
                Intent = hasIntent ? intent : null,
                PatientInstruction = request.PatientInstruction,
                Subject = request.PatientReference.ToResourceReference(),
                Code = new CodeableConcept
                {
                    Coding = request.Code.Select(Mapper.ToCoding).ToList()
                },
                Occurrence = request.Occurrence?.ToTiming()
            };
        }
    }
}