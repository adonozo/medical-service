namespace QMUL.DiabetesBackend.ServiceInterfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

public interface IObservationsConfigService
{
    /// <summary>
    /// Adds an observation config
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    Task<Observation> AddConfig(Observation config);

    /// <summary>
    /// Gets a single config by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Observation> GetConfig(string id);

    /// <summary>
    /// Gets a collection of observations. Can be filtered out by type // TODO does it need to have filters?
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    Task<Bundle> SearchConfig(string? type = null);
}