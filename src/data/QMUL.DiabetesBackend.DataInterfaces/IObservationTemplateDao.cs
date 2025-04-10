namespace QMUL.DiabetesBackend.DataInterfaces;

using System.Threading.Tasks;
using Model;

public interface IObservationTemplateDao
{
    Task<ObservationTemplate?> GetObservationTemplate(string id);

    Task<ObservationTemplate?> GetObservationTemplateByCode(string code, string system);

    Task<PaginatedResults<ObservationTemplate>> SearchObservationTemplates(
        PaginationRequest paginationRequest,
        string? type = null);

    Task<ObservationTemplate> CreateObservationTemplate(ObservationTemplate template);

    Task<bool> UpdateObservationTemplate(ObservationTemplate template);

    Task<bool> DeleteObservationTemplate(string id);
}