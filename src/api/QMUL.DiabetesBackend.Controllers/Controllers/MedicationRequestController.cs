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
public class MedicationRequestController : ControllerBase
{
    private readonly IMedicationRequestService medicationRequestService;
    private readonly IResourceValidator<MedicationRequest> validator;
    private readonly ILogger<MedicationRequestController> logger;

    public MedicationRequestController(IMedicationRequestService medicationRequestService,
        IResourceValidator<MedicationRequest> validator, ILogger<MedicationRequestController> logger)
    {
        this.logger = logger;
        this.validator = validator;
        this.medicationRequestService = medicationRequestService;
    }

    [HttpGet("medicationRequests/{id}")]
    public async Task<IActionResult> GetMedicationRequest([FromRoute] string id)
    {
        var result = await this.medicationRequestService.GetMedicationRequest(id);
        return this.OkOrNotFound(result);
    }

    [HttpPost("medicationRequests")]
    public async Task<IActionResult> CreateMedicationRequest([FromBody] JObject request)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            var medicationRequest = await this.validator.ParseAndValidateAsync(request);
            var result = await this.medicationRequestService.CreateMedicationRequest(medicationRequest);
            return this.Ok(result.ToJObject());
        }, this.logger, this);
    }

    [HttpPut("medicationRequests/{id}")]
    public async Task<IActionResult> UpdateMedicationRequest([FromRoute] string id, [FromBody] JObject request)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            var medicationRequest = await this.validator.ParseAndValidateAsync(request);
            await this.medicationRequestService.UpdateMedicationRequest(id, medicationRequest);
            return this.Accepted();
        }, this.logger, this);
    }

    [HttpDelete("medicationRequests/{id}")]
    public async Task<IActionResult> DeleteMedicationRequest([FromRoute] string id)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            await this.medicationRequestService.DeleteMedicationRequest(id);
            return this.NoContent();
        }, this.logger, this);
    }
}