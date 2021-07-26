using System;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.Model.Enums;

namespace QMUL.DiabetesBackend.ServiceImpl.Utils
{
    public static class ResourceUtils
    {
        public static DomainResource GetResourceFromEvent(this HealthEvent healthEvent)
        {
            return healthEvent.Resource.EventType switch
            {
                EventType.MedicationDosage => new MedicationRequest { },
                EventType.InsulinDosage => new MedicationRequest(),
                EventType.Measurement => new ServiceRequest(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}