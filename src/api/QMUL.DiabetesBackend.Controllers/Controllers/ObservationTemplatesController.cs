namespace QMUL.DiabetesBackend.Controllers.Controllers;

using System.Threading.Tasks;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model;
using Service.Validators;
using ServiceInterfaces;
using Utils;

[ApiController]
public class ObservationTemplatesController : ControllerBase
{
    private readonly IObservationsTemplatesService observationsTemplatesService;
    private readonly ValidatorBase<ObservationTemplate> templateValidator;
    private readonly ILogger<ObservationTemplatesController> logger;

    public ObservationTemplatesController(
        IObservationsTemplatesService observationsTemplatesService,
        ILogger<ObservationTemplatesController> logger,
        ValidatorBase<ObservationTemplate> templateValidator)
    {
        this.observationsTemplatesService = observationsTemplatesService;
        this.logger = logger;
        this.templateValidator = templateValidator;
    }

    [HttpPost("observations/config")]
    public async Task<IActionResult> CreateConfig(ObservationTemplate template)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            this.templateValidator.ValidateResource(template);

            var newTemplate = await this.observationsTemplatesService.AddTemplate(template);
            return Ok(newTemplate);
        }, this.logger, this);
    }

    [HttpGet("observations/config/{id}")]
    public async Task<IActionResult> GetConfig([FromRoute] string id)
    {
        var template = await this.observationsTemplatesService.GetTemplate(id);
        return this.OkOrNotFound(template);
    }

    [HttpGet("observations/config")]
    public async Task<IActionResult> SearchConfig([FromQuery] string? type = null)
    {
        var configBundle = await this.observationsTemplatesService.SearchTemplate(type);
        return Ok(configBundle.ToJObject());
    }
}