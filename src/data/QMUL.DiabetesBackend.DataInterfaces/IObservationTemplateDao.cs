namespace QMUL.DiabetesBackend.DataInterfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using Model;

public interface IObservationTemplateDao
{
    Task<ObservationTemplate> GetObservationTemplate(string id);

    Task<PaginatedResult<IEnumerable<ObservationTemplate>>> SearchObservationTemplates(string? type = null);

    Task<ObservationTemplate> CreateObservationTemplate(ObservationTemplate template);
}