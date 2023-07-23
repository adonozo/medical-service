namespace QMUL.DiabetesBackend.Model.Extensions;

using System.Linq;
using Constants;
using Hl7.Fhir.Model;

public static class CarePlanExtensions
{
    public static string[] GetServiceRequestsIds(this CarePlan carePlan) =>
        carePlan.Activity
            .Where(resource => resource.Reference.Reference.Contains(Constants.ServiceRequestPath))
            .Select(resource => resource.Reference.GetIdFromReference())
            .ToArray();
    
    public static string[] GetMedicationRequestsIds(this CarePlan carePlan) =>
        carePlan.Activity
            .Where(resource => resource.Reference.Reference.Contains(Constants.MedicationRequestPath))
            .Select(resource => resource.Reference.GetIdFromReference())
            .ToArray();
}