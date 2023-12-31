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
public class MedicationController : ControllerBase
{
    private readonly IMedicationService medicationService;
    private readonly ILogger<MedicationController> logger;
    private readonly IResourceValidator<Medication> medicationValidator;

    public MedicationController(IMedicationService medicationService,
        IResourceValidator<Medication> medicationValidator,
        ILogger<MedicationController> logger)
    {
        this.medicationService = medicationService;
        this.logger = logger;
        this.medicationValidator = medicationValidator;
    }

    [HttpGet("medications")]
    public async Task<IActionResult> GetAllMedications([FromQuery] string? name = null, [FromQuery] int? limit = null,
        [FromQuery] string? after = null)
    {
        var pagination = new PaginationRequest(limit, after);
        var paginatedResult = await this.medicationService.GetMedicationList(pagination, name);

        this.HttpContext.SetPaginatedResult(paginatedResult);
        return this.Ok(paginatedResult.Results.ToJObject());
    }

    [HttpGet("medications/{id}")]
    public async Task<IActionResult> GetMedication([FromRoute] string id)
    {
        var medication = await this.medicationService.GetMedication(id);
        return this.OkOrNotFound(medication);
    }

    [HttpPost("medications")]
    public async Task<IActionResult> CreateMedication([FromBody] JObject request)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            var parsedRequest = await this.medicationValidator.ParseAndValidateAsync(request);
            var result = await this.medicationService.CreateMedication(parsedRequest);
            return this.Ok(result.ToJObject());
        }, this.logger, this);
    }
}