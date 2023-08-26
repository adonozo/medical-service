namespace QMUL.DiabetesBackend.Controllers.Controllers;

using System.Net;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Constants;
using Model.Enums;
using NodaTime;
using ServiceInterfaces;
using Utils;

[ApiController]
public class AlexaController : ControllerBase
{
    public const string ResourceErrorKey = "resource";

    private readonly IAlexaService alexaService;
    private readonly IObservationService observationService;

    public AlexaController(IAlexaService alexaService,
        IObservationService observationService)
    {
        this.alexaService = alexaService;
        this.observationService = observationService;
    }

    [HttpGet("alexa/{idOrEmail}/observations/")]
    public async Task<IActionResult> GetPatientObservations([FromRoute] string idOrEmail,
        [FromQuery] LocalDate? date,
        [FromQuery] CustomEventTiming? timing,
        [FromQuery] string? timezone = "UTC",
        [FromQuery] int? limit = null,
        [FromQuery] string? after = null)
    {
        if (date is null)
        {
            ModelState.AddModelError("date", "Date is required");
            return this.UnprocessableEntity(ModelState);
        }

        var pagination = new PaginationRequest(limit, after);
        var paginatedResult =
            await this.observationService.GetObservationsFor(
                patientId: idOrEmail,
                timing: timing ?? CustomEventTiming.ALL_DAY,
                dateTime: date.Value,
                paginationRequest: pagination,
                patientTimezone: timezone!);

        this.HttpContext.SetPaginatedResult(paginatedResult);
        return this.Ok(paginatedResult.Results.ToJObject());
    }

    [HttpGet("alexa/{idOrEmail}/medication-requests")]
    public async Task<IActionResult> GetMedicationRequests([FromRoute] string idOrEmail,
        [FromQuery] LocalDate? date,
        [FromQuery] string? timezone = "UTC",
        [FromQuery] CustomEventTiming? timing = CustomEventTiming.ALL_DAY,
        [FromQuery] bool? onlyInsulin = false)
    {
        if (date is null)
        {
            ModelState.AddModelError("date", "Date is required");
            return this.UnprocessableEntity(ModelState);
        }

        var result = await this.alexaService.SearchMedicationRequests(idOrEmail,
            date.Value,
            onlyInsulin ?? false,
            timing,
            timezone);
        if (result.IsSuccess)
        {
            return this.Ok(result.Results.ToJObject());
        }

        var errorResponse = GetErrorResponse(result.Error, Constants.MedicationRequestPath);
        return this.UnprocessableEntity(errorResponse);
    }

    [HttpGet("patients/{idOrEmail}/alexa/service-requests")]
    public async Task<IActionResult> GetServiceRequests([FromRoute] string idOrEmail,
        [FromQuery] LocalDate? date,
        [FromQuery] string timezone = "UTC",
        [FromQuery] CustomEventTiming timing = CustomEventTiming.EXACT)
    {
        if (date is null)
        {
            ModelState.AddModelError("date", "Date is required");
            return this.UnprocessableEntity(ModelState);
        }

        var result = await this.alexaService.SearchServiceRequests(idOrEmail, date.Value, timing, timezone);
        if (result.IsSuccess)
        {
            return this.Ok(result.Results.ToJObject());
        }

        var errorResponse = GetErrorResponse(result.Error, Constants.ServiceRequestPath);
        return this.UnprocessableEntity(errorResponse);
    }

    private static ProblemDetails GetErrorResponse<T>(T resource, string path) where T : DomainResource => new()
    {
        Title = "Resource needs a start date",
        Detail = "Cannot find occurrences for the provided period because the resource needs a start date",
        Instance = path + resource.Id,
        Status = (int)HttpStatusCode.UnprocessableEntity,
        Extensions = { { ResourceErrorKey, resource.ToJObject() } }
    };
}