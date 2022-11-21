namespace QMUL.DiabetesBackend.Controllers.Controllers;

using System;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using Model.Enums;
using ServiceInterfaces;
using Utils;

[ApiController]
public class AlexaController : ControllerBase
{
    private readonly IAlexaService alexaService;

    public AlexaController(IAlexaService alexaService)
    {
        this.alexaService = alexaService;
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