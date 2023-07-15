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

    [HttpGet("patients/{idOrEmail}/alexa/medicationRequest")]
    public async Task<IActionResult> GetMedicationRequest([FromRoute] string idOrEmail,
        [FromQuery] DateTime date,
        [FromQuery] string timezone = "UTC",
        [FromQuery] CustomEventTiming timing = CustomEventTiming.EXACT)
    {
        var result = await this.alexaService.ProcessMedicationRequest(idOrEmail, date, timing, timezone);
        return this.OkOrNotFound(result);
    }

    [HttpGet("patients/{idOrEmail}/alexa/insulinRequest")]
    public async Task<IActionResult> GetInsulinMedicationRequest([FromRoute] string idOrEmail,
        [FromQuery] DateTime date,
        [FromQuery] string timezone = "UTC",
        [FromQuery] CustomEventTiming timing = CustomEventTiming.EXACT)
    {
        var result = await this.alexaService.ProcessInsulinMedicationRequest(idOrEmail, date, timing, timezone);
        return this.OkOrNotFound(result);
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

    [HttpGet("patients/{idOrEmail}/alexa/carePlan")]
    public async Task<IActionResult> GetCarePlan([FromRoute] string idOrEmail,
        [FromQuery] DateTime date,
        [FromQuery] string timezone = "UTC",
        [FromQuery] CustomEventTiming timing = CustomEventTiming.EXACT)
    {
        var result = await this.alexaService.ProcessCarePlanRequest(idOrEmail, date, timing, timezone);
        return this.OkOrNotFound(result);
    }

    [HttpGet("patients/{idOrEmail}/alexa/next")]
    public async Task<IActionResult> GetAlexaNextRequest([FromRoute] string idOrEmail,
        [FromQuery] AlexaRequestType type)
    {
        Bundle? result;
        if (type == AlexaRequestType.CarePlan)
        {
            result = await this.alexaService.GetNextRequests(idOrEmail);
        }
        else
        {
            result = await this.alexaService.GetNextRequests(idOrEmail, type);
        }

        return this.OkOrNotFound(result);
    }
}