namespace QMUL.DiabetesBackend.Service;

using System.Threading.Tasks;
using DataInterfaces;
using Microsoft.Extensions.Logging;
using Model;
using Model.Exceptions;
using ServiceInterfaces;

public class ObservationTemplateService : IObservationTemplateService
{
    private readonly ILogger<ObservationTemplateService> logger;
    private readonly IObservationTemplateDao templateDao;

    public ObservationTemplateService(ILogger<ObservationTemplateService> logger, IObservationTemplateDao templateDao)
    {
        this.logger = logger;
        this.templateDao = templateDao;
    }

    public async Task<ObservationTemplate> AddTemplate(ObservationTemplate template)
    {
        logger.LogInformation("Inserting new observation template {Code}", template.Code);
        return await this.templateDao.CreateObservationTemplate(template);
    }

    public async Task<ObservationTemplate?> GetTemplate(string id)
    {
        return await this.templateDao.GetObservationTemplate(id);
    }

    public async Task<PaginatedResults<ObservationTemplate>> SearchTemplate(
        PaginationRequest paginationRequest,
        string? type = null)
    {
        return await this.templateDao.SearchObservationTemplates(paginationRequest, type);
    }

    public async Task<bool> UpdateObservationTemplate(string id, ObservationTemplate template)
    {
        var templateExists = await this.templateDao.GetObservationTemplate(id) is not null;
        if (!templateExists)
        {
            throw new NotFoundException();
        }

        template.Id = id;
        return await this.templateDao.UpdateObservationTemplate(template);
    }
}