namespace QMUL.DiabetesBackend.Controllers.Controllers;

using System;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Enums;
using ServiceInterfaces;
using Utils;

[ApiController]
public class AlexaController : ControllerBase
{
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
        [FromQuery] DateTime? date,
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
                timing ?? CustomEventTiming.ALL_DAY,
                date.Value,
                paginationRequest: pagination,
                patientTimezone: timezone!);

        this.HttpContext.SetPaginatedResult(paginatedResult);
        return this.Ok(paginatedResult.Results.ToJObject());
    }

    [HttpGet("alexa/{idOrEmail}/medicationRequests")]
    public async Task<IActionResult> GetMedicationRequest([FromRoute] string idOrEmail,
        [FromQuery] DateTime? date,
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
        return this.Ok(result.ToJObject());
    }

    [HttpGet("patients/{idOrEmail}/alexa/glucoseRequest")]
    public async Task<IActionResult> GetGlucoseServiceRequest([FromRoute] string idOrEmail,
        [FromQuery] DateTime date,
        [FromQuery] string timezone = "UTC",
        [FromQuery] CustomEventTiming timing = CustomEventTiming.EXACT)
    {
        var result = await this.alexaService.ProcessGlucoseServiceRequest(idOrEmail, date, timing, timezone);
        return this.OkOrNotFound(result);
    }
}