namespace QMUL.DiabetesBackend.Controllers.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model;
using Service.Validators;
using ServiceInterfaces;
using Utils;

[ApiController]
public class ObservationTemplatesController : ControllerBase
{
    private readonly IObservationTemplateService observationTemplateService;
    private readonly ValidatorBase<ObservationTemplate> templateValidator;
    private readonly ILogger<ObservationTemplatesController> logger;

    public ObservationTemplatesController(
        IObservationTemplateService observationTemplateService,
        ILogger<ObservationTemplatesController> logger,
        ValidatorBase<ObservationTemplate> templateValidator)
    {
        this.observationTemplateService = observationTemplateService;
        this.logger = logger;
        this.templateValidator = templateValidator;
    }

    [HttpPost("observation-templates")]
    public async Task<IActionResult> CreateObservationTemplate(ObservationTemplate template)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            this.templateValidator.ValidateResource(template);

            var newTemplate = await this.observationTemplateService.AddTemplate(template);
            return Ok(newTemplate);
        }, this.logger, this);
    }

    [HttpGet("observation-templates/{id}")]
    public async Task<IActionResult> GetObservationTemplate([FromRoute] string id)
    {
        var template = await this.observationTemplateService.GetTemplate(id);
        return this.OkOrNotFound(template);
    }

    [HttpGet("observation-templates")]
    public async Task<IActionResult> SearchObservationTemplates(
        [FromQuery] string? type = null,
        [FromQuery] int? limit = null,
        [FromQuery] string? after = null)
    {
        var pagination = new PaginationRequest(limit, after);
        var paginatedTemplates = await this.observationTemplateService.SearchTemplate(pagination, type);
        return Ok(paginatedTemplates);
    }

    [HttpPut("observation-templates/{id}")]
    public async Task<IActionResult> UpdateObservationTemplate([FromRoute] string id,
        [FromBody] ObservationTemplate template)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            // TODO validation shouldn't throw but return a result object
            this.templateValidator.ValidateResource(template);
            var resultSuccessful = await this.observationTemplateService.UpdateObservationTemplate(id, template);
            return resultSuccessful ? this.Ok() : (IActionResult)this.BadRequest();
        }, this.logger, this);
    }

    [HttpDelete("observation-templates/{id}")]
    public async Task<IActionResult> DeleteObservationTemplate([FromRoute] string id)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            var resultSuccessful = await this.observationTemplateService.DeleteObservationTemplate(id);
            return resultSuccessful ? this.Ok() : (IActionResult)this.BadRequest();
        }, this.logger, this);
    }

    [HttpPut("observation-templates")]
    public async Task<IActionResult> SeedObservationTemplate([FromBody] ObservationTemplate template)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            this.templateValidator.ValidateResource(template);

            var createdTemplate = await this.observationTemplateService.InsertSeededTemplate(template);
            return this.Ok(createdTemplate);
        }, this.logger, this);
    }
}