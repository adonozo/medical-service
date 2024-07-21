namespace QMUL.DiabetesBackend.ServiceInterfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model;

public interface IObservationTemplateService
{
    /// <summary>
    /// Adds an observation config
    /// </summary>
    /// <param name="template"></param>
    /// <returns></returns>
    Task<ObservationTemplate> AddTemplate(ObservationTemplate template);

    /// <summary>
    /// Gets a single config by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ObservationTemplate> GetTemplate(string id);

    /// <summary>
    /// Gets a collection of observations. Can be filtered out by type // TODO does it need to have filters?
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    Task<PaginatedResult<IEnumerable<ObservationTemplate>>> SearchTemplate(string? type = null);
}