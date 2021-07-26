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

        /// <summary>
        /// Gets the medication requests for a given time.
        /// </summary>
        /// <returns>A Bundle list containing <see cref="MedicationRequest"/></returns>
        public Task<Bundle> GetMedicationRequests(string patientEmailOrId, DateTime dateTime,
            AlexaRequestTime requestTime, CustomEventTiming timing, bool insulin);

        /// <summary>
        /// Gets the service requests for a given time.
        /// </summary>
        /// /// <returns>A Bundle list containing <see cref="ServiceRequest"/></returns>
        public Task<Bundle> GetServiceRequests(string patientEmailOrId, DateTime dateTime,
            AlexaRequestTime requestTime, CustomEventTiming timing);

        /// <summary>
        /// Gets the patient's blood glucose measures for a given time 
        /// </summary>
        /// <returns>A Bundle list containing <see cref="Measure"/></returns>
        public Task<Bundle> GetMeasurements(string patientEmailOrId, DateTime dateTime,
            AlexaRequestTime requestTime, CustomEventTiming timing);

        public DiagnosticReport SaveGlucoseMeasure(string patientId, DiagnosticReport report);
    }
}