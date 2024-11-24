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
        await this.AssertTemplateExists(id);
        template.Id = id;
        return await this.templateDao.UpdateObservationTemplate(template);
    }

    public async Task<bool> DeleteObservationTemplate(string id)
    {
        await this.AssertTemplateExists(id);
        return await this.templateDao.DeleteObservationTemplate(id);
    }

    public async Task<ObservationTemplate> InsertSeededTemplate(ObservationTemplate template)
    {
        var existingTemplate = await this.templateDao.GetObservationTemplateByCode(template.Code.Coding.Code,
            template.Code.Coding.System);
        if (existingTemplate is not null)
        {
            return existingTemplate;
        }

        return await this.AddTemplate(template);
    }

    private async Task AssertTemplateExists(string observationTemplateId)
    {
        var templateExists = await this.templateDao.GetObservationTemplate(observationTemplateId) is not null;
        if (!templateExists)
        {
            throw new NotFoundException();
        }
    }
}