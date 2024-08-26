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
    public async Task<IActionResult> CreateConfig(ObservationTemplate template)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            this.templateValidator.ValidateResource(template);

            var newTemplate = await this.observationTemplateService.AddTemplate(template);
            return Ok(newTemplate);
        }, this.logger, this);
    }

    [HttpGet("observation-templates/{id}")]
    public async Task<IActionResult> GetConfig([FromRoute] string id)
    {
        var template = await this.observationTemplateService.GetTemplate(id);
        return this.OkOrNotFound(template);
    }

    [HttpGet("observation-templates")]
    public async Task<IActionResult> SearchConfig(
        [FromQuery] string? type = null,
        [FromQuery] int? limit = null,
        [FromQuery] string? after = null)
    {
        var pagination = new PaginationRequest(limit, after);
        var paginatedTemplates = await this.observationTemplateService.SearchTemplate(pagination, type);
        return Ok(paginatedTemplates);
    }
}