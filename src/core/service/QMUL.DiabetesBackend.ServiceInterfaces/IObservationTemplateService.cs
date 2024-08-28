namespace QMUL.DiabetesBackend.ServiceInterfaces;

using System.Threading.Tasks;
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
    Task<ObservationTemplate?> GetTemplate(string id);

    /// <summary>
    /// Gets a collection of observations. Can be filtered out by type
    /// </summary>
    /// <param name="paginationRequest"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    Task<PaginatedResults<ObservationTemplate>> SearchTemplate(
        PaginationRequest paginationRequest,
        string? type = null);

    Task<bool> UpdateObservationTemplate(string id, ObservationTemplate template);

    Task<bool> DeleteObservationTemplate(string id);
}