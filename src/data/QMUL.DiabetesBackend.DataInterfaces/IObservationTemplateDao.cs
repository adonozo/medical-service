namespace QMUL.DiabetesBackend.DataInterfaces;

using System.Threading.Tasks;
using Model;

public interface IObservationTemplateDao
{
    Task<ObservationTemplate?> GetObservationTemplate(string id);

    Task<PaginatedResults<ObservationTemplate>> SearchObservationTemplates(
        PaginationRequest paginationRequest,
        string? type = null);

    Task<ObservationTemplate> CreateObservationTemplate(ObservationTemplate template);

    Task<bool> UpdateObservationTemplate(ObservationTemplate template);
}