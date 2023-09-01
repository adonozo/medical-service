namespace QMUL.DiabetesBackend.Model.Extensions;

using System.Linq;
using Constants;
using Hl7.Fhir.Model;

public static class CarePlanExtensions
{
    /// <summary>
    /// Gets the IDs of service requests contained in a <see cref="CarePlan"/> activity. It looks in the activity's
    /// <see cref="ResourceReference"/> field
    /// </summary>
    /// <param name="carePlan">The <see cref="CarePlan"/></param>
    /// <returns>An array of service request IDs</returns>
    public static string[] GetServiceRequestsIds(this CarePlan carePlan) =>
        carePlan.Activity
            .Where(resource => resource.PlannedActivityReference.Reference.Contains(Constants.ServiceRequestPath))
            .Select(resource => resource.PlannedActivityReference.GetIdFromReference())
            .ToArray();

    /// <summary>
    /// Gets the IDs of medication requests contained in a <see cref="CarePlan"/> activity. It looks in the activity's
    /// <see cref="ResourceReference"/> field
    /// </summary>
    /// <param name="carePlan">The <see cref="CarePlan"/></param>
    /// <returns>An array of medication request IDs</returns>
    public static string[] GetMedicationRequestsIds(this CarePlan carePlan) =>
        carePlan.Activity
            .Where(resource => resource.PlannedActivityReference.Reference.Contains(Constants.MedicationRequestPath))
            .Select(resource => resource.PlannedActivityReference.GetIdFromReference())
            .ToArray();
}