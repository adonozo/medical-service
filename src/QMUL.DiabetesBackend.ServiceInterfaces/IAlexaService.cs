using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.Model.Enums;

namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    public interface IAlexaService
    {
        public List<DomainResource> ProcessRequest(string patientEmailOrId, AlexaRequestType type, DateTime dateTime,
            AlexaRequestTime requestTime, Timing.EventTiming timing);

        public DiagnosticReport SaveGlucoseMeasure(string patientId, DiagnosticReport report);
    }
}