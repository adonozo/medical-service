using System.Linq;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.MongoDb.Models;
using static System.Enum;

namespace QMUL.DiabetesBackend.MongoDb.Utils
{
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
                Code = request.Code.Coding.Select(ToMongoCode).ToList(),
                
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
                Subject = new ResourceReference
                {
                    ElementId = request.PatientReference.ReferenceId,
                    Display = request.PatientReference.ReferenceName
                },
                Code = new CodeableConcept
                {
                    Coding = request.Code.Select(ToCoding).ToList()
                },
                Occurrence = request.Occurrence?.ToTiming()
            };
        }

        private static MongoCode ToMongoCode(this Coding code)
        {
            return new()
            {
                Display = code.Display,
                Code = code.Code,
                System = code.System
            };
        }

        private static Coding ToCoding(this MongoCode code)
        {
            return new()
            {
                Display = code.Display,
                Code = code.Code,
                System = code.System
            };
        }
    }
}