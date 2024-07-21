namespace QMUL.DiabetesBackend.Service;

using System.Collections.Generic;
using System.Threading.Tasks;
using DataInterfaces;
using Microsoft.Extensions.Logging;
using Model;
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

    public async Task<ObservationTemplate> GetTemplate(string id)
    {
        return await this.templateDao.GetObservationTemplate(id);
    }

    public async Task<PaginatedResult<IEnumerable<ObservationTemplate>>> SearchTemplate(string? type = null)
    {
        return await this.templateDao.SearchObservationTemplates(type);
    }
}