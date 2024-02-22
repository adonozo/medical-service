namespace QMUL.DiabetesBackend.Controllers.Controllers;

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ServiceInterfaces;
using ServiceInterfaces.Validators;
using Utils;

[ApiController]
public class ObservationsConfigController : ControllerBase
{
    private readonly IObservationsConfigService observationsConfigService;
    private readonly IResourceValidator<Observation> observationValidator; // TODO config validator
    private readonly ILogger<ObservationsConfigController> logger;

    public ObservationsConfigController(
        IObservationsConfigService observationsConfigService,
        ILogger<ObservationsConfigController> logger,
        IResourceValidator<Observation> observationValidator)
    {
        this.observationsConfigService = observationsConfigService;
        this.logger = logger;
        this.observationValidator = observationValidator;
    }

    [HttpPost("observations/config")]
    public async Task<IActionResult> CreateConfig(JObject partialObservation)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            var observation = await this.observationValidator.ParseAndValidateAsync(partialObservation);
            var configObservation = await this.observationsConfigService.AddConfig(observation);
            return Ok(configObservation.ToJObject());
        }, this.logger, this);
    }

    [HttpGet("observations/config/{id}")]
    public async Task<IActionResult> GetConfig([FromRoute] string id)
    {
        var observation = await this.observationsConfigService.GetConfig(id);
        return this.OkOrNotFound(observation);
    }

    [HttpGet("observations/config")]
    public async Task<IActionResult> SearchConfig([FromQuery] string? type = null)
    {
        var configBundle = await this.observationsConfigService.SearchConfig(type);
        return Ok(configBundle.ToJObject());
    }
}