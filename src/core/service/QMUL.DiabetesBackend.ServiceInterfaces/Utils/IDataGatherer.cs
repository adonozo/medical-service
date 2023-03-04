namespace QMUL.DiabetesBackend.ServiceInterfaces.Utils;

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;

public interface IDataGatherer
{
    Task<Patient> GetPatientOrThrow(string patientId);
    
    Task<InternalPatient> GetInternalPatientOrThrow(string patientId);

    Task<bool> ResourceHasActiveCarePlan(DomainResource resource);
}