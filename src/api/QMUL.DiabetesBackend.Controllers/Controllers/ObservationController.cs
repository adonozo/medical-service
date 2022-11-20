namespace QMUL.DiabetesBackend.Controllers.Controllers;

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model;
using Newtonsoft.Json.Linq;
using ServiceInterfaces;
using ServiceInterfaces.Validators;
using Utils;

[ApiController]
public class ObservationController : ControllerBase
{
    private readonly IObservationService observationService;
    private readonly IResourceValidator<Observation> observationValidator;
    private readonly IDataTypeValidator dataTypeValidator;
    private readonly ILogger<ObservationController> logger;

    public ObservationController(IObservationService observationService,
        IResourceValidator<Observation> observationValidator,
        IDataTypeValidator dataTypeValidator,
        ILogger<ObservationController> logger)
    {
        this.observationService = observationService;
        this.observationValidator = observationValidator;
        this.dataTypeValidator = dataTypeValidator;
        this.logger = logger;
    }

    [HttpGet("observations/{id}")]
    public async Task<IActionResult> GetObservation([FromRoute] string id)
    {
        var observation = await this.observationService.GetObservation(id);
        return this.OkOrNotFound(observation);
    }

    [HttpPost("observations")]
    public async Task<IActionResult> PostObservation([FromBody] JObject request)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            var observation = await this.observationValidator.ParseAndValidateAsync(request);
            var newObservation = await this.observationService.CreateObservation(observation);
            return this.Ok(newObservation.ToJObject());
        }, this.logger, this);
    }

    [HttpPut("observations/{id}")]
    public async Task<IActionResult> PutObservation([FromRoute] string id, [FromBody] JObject request)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            var observation = await this.observationValidator.ParseAndValidateAsync(request);
            var updatedObservation = await this.observationService.UpdateObservation(id, observation);
            return this.Accepted(updatedObservation.ToJObject());
        }, this.logger, this);
    }

    [HttpPatch("observations/{id}/value")]
    public async Task<IActionResult> PatchObservationValue([FromRoute] string id, [FromBody] DataTypeWrapper wrapper)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            var value = await this.dataTypeValidator.ParseAndValidateAsync(wrapper);
            var updatedObservation = await this.observationService.UpdateValue(id, value);
            return this.Accepted(updatedObservation.ToJObject());
        }, this.logger, this);
    }

    [HttpDelete("observations/{id}")]
    public async Task<IActionResult> DeleteObservation([FromRoute] string id)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            await this.observationService.DeleteObservation(id);
            return this.NoContent();
        }, this.logger, this);
    }
}