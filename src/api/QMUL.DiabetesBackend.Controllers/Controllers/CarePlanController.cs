namespace QMUL.DiabetesBackend.Controllers.Controllers;

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ServiceInterfaces;
using ServiceInterfaces.Validators;
using Utils;

[ApiController]
public class CarePlanController : Controller
{
    private readonly ICarePlanService carePlanService;
    private readonly ILogger<CarePlanController> logger;
    private readonly IResourceValidator<CarePlan> carePlanValidator;
    private readonly IResourceValidator<ServiceRequest> serviceRequestValidator;
    private readonly IResourceValidator<MedicationRequest> medicationRequestValidator;

    public CarePlanController(ICarePlanService carePlanService,
        IResourceValidator<CarePlan> carePlanValidator,
        IResourceValidator<ServiceRequest> serviceRequestValidator,
        IResourceValidator<MedicationRequest> medicationRequestValidator,
        ILogger<CarePlanController> logger)
    {
        this.carePlanService = carePlanService;
        this.logger = logger;
        this.medicationRequestValidator = medicationRequestValidator;
        this.serviceRequestValidator = serviceRequestValidator;
        this.carePlanValidator = carePlanValidator;
    }

    [HttpGet("carePlans/{id}")]
    public async Task<IActionResult> GetCarePlan([FromRoute] string id)
    {
        var carePlan = await this.carePlanService.GetCarePlan(id);
        return this.OkOrNotFound(carePlan);
    }

    [HttpGet("carePlans/{id}/details")]
    public async Task<IActionResult> GetDetailedCarePlan([FromRoute] string id)
    {
        var carePlan = await this.carePlanService.GetDetailedCarePan(id);
        return this.OkOrNotFound(carePlan);
    }

    [HttpPost("carePlans")]
    public async Task<IActionResult> CreateCarePlan([FromBody] JObject request)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            var carePlan = await this.carePlanValidator.ParseAndValidateAsync(request);
            var result = await this.carePlanService.CreateCarePlan(carePlan);
            return this.Ok(result.ToJObject());
        }, this.logger, this);
    }

    [HttpPost("carePlans/{id}/serviceRequests")]
    public async Task<IActionResult> AddServiceRequest([FromRoute] string id, [FromBody] JObject request)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync<IActionResult>(async () =>
        {
            var serviceRequest = await this.serviceRequestValidator.ParseAndValidateAsync(request);
            var carePlanUpdated = await this.carePlanService.AddServiceRequest(id, serviceRequest);
            return carePlanUpdated ? this.Accepted() : this.StatusCode(StatusCodes.Status500InternalServerError);
        }, this.logger, this);
    }

    [HttpPost("carePlans/{id}/medicationRequests")]
    public async Task<IActionResult> AddMedicationRequest([FromRoute] string id, [FromBody] JObject request)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync<IActionResult>(async () =>
        {
            var medicationRequest = await this.medicationRequestValidator.ParseAndValidateAsync(request);
            var carePlanUpdated = await this.carePlanService.AddMedicationRequest(id, medicationRequest);
            return carePlanUpdated ? this.Accepted() : this.StatusCode(StatusCodes.Status500InternalServerError);
        }, this.logger, this);
    }

    [HttpPut("carePlans/{id}/activate")]
    public async Task<IActionResult> ActivateCarePlan([FromRoute] string id)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            var carePlanActivated = await this.carePlanService.ActivateCarePlan(id);
            return carePlanActivated ? this.NoContent() : this.StatusCode(StatusCodes.Status500InternalServerError);
        }, this.logger, this);
    }

    [HttpPut("carePlans/{id}/revoke")]
    public async Task<IActionResult> RevokeCarePlan([FromRoute] string id)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            var carePlanActivated = await this.carePlanService.RevokeCarePlan(id);
            return carePlanActivated ? this.NoContent() : this.StatusCode(StatusCodes.Status500InternalServerError);
        }, this.logger, this);
    }

    [HttpDelete("carePlans/{id}")]
    public Task<IActionResult> DeleteCarePlan([FromRoute] string id)
    {
        return ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            await this.carePlanService.DeleteCarePlan(id);
            return this.NoContent();
        }, this.logger, this);
    }

    [HttpDelete("carePlans/{carePlanId}/serviceRequests/{medicationRequestId}")]
    public Task<IActionResult> DeleteServiceRequest([FromRoute] string carePlanId,
        [FromRoute] string medicationRequestId)
    {
        return ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            await this.carePlanService.DeleteServiceRequest(carePlanId, medicationRequestId);
            return this.NoContent();
        }, this.logger, this);
    }

    [HttpDelete("carePlans/{carePlanId}/medicationRequests/{medicationRequestId}")]
    public Task<IActionResult> DeleteMedicationRequest([FromRoute] string carePlanId,
        [FromRoute] string medicationRequestId)
    {
        return ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            await this.carePlanService.DeleteMedicationRequest(carePlanId, medicationRequestId);
            return this.NoContent();
        }, this.logger, this);
    }
}