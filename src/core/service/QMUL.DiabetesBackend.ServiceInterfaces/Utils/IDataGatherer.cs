namespace QMUL.DiabetesBackend.ServiceInterfaces.Utils;

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;
using ResourceReference = Hl7.Fhir.Model.ResourceReference;

public interface IDataGatherer
{
    Task<InternalPatient> GetReferenceInternalPatientOrThrow(ResourceReference reference);
    
    Task<Patient> GetReferencePatientOrThrow(ResourceReference reference);

    Task<bool> ResourceHasActiveCarePlan(DomainResource resource);
}