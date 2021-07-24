using System;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.Model.Enums;

namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    public interface IAlexaService
    {
        public Task<Bundle> ProcessRequest(string patientEmailOrId, AlexaRequestType type, DateTime dateTime,
            AlexaRequestTime requestTime, CustomEventTiming timing);

        public DiagnosticReport SaveGlucoseMeasure(string patientId, DiagnosticReport report);
    }
}