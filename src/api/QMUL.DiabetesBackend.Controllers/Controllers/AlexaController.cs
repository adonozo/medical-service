namespace QMUL.DiabetesBackend.Api.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Serialization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Model.Enums;
    using ServiceInterfaces;
    using Utils;

    [ApiController]
    [Route("patients/")]
    public class AlexaController : ControllerBase
    {
        private readonly IAlexaService alexaService;
        private readonly ILogger<AlexaController> logger;

        public AlexaController(IAlexaService alexaService, ILogger<AlexaController> logger)
        {
            this.alexaService = alexaService;
            this.logger = logger;
        }

        [HttpGet]
        [Route("{idOrEmail}/alexa/medicationRequest")]
        public async Task<IActionResult> GetMedicationRequest([FromRoute] string idOrEmail,
            [FromQuery] DateTime date,
            [FromQuery] string timezone = "UTC",
            [FromQuery] CustomEventTiming timing = CustomEventTiming.EXACT)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var result = await this.alexaService.ProcessMedicationRequest(idOrEmail, date, timing, timezone);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpGet]
        [Route("{idOrEmail}/alexa/insulinRequest")]
        public async Task<IActionResult> GetInsulinMedicationRequest([FromRoute] string idOrEmail,
            [FromQuery] DateTime date,
            [FromQuery] string timezone = "UTC",
            [FromQuery] CustomEventTiming timing = CustomEventTiming.EXACT)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var result = await this.alexaService.ProcessInsulinMedicationRequest(idOrEmail, date, timing, timezone);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpGet]
        [Route("{idOrEmail}/alexa/glucoseRequest")]
        public async Task<IActionResult> GetGlucoseServiceRequest([FromRoute] string idOrEmail,
            [FromQuery] DateTime date,
            [FromQuery] string timezone = "UTC",
            [FromQuery] CustomEventTiming timing = CustomEventTiming.EXACT)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var result = await this.alexaService.ProcessGlucoseServiceRequest(idOrEmail, date, timing, timezone);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpGet]
        [Route("{idOrEmail}/alexa/carePlan")]
        public async Task<IActionResult> GetCarePlan([FromRoute] string idOrEmail,
            [FromQuery] DateTime date,
            [FromQuery] string timezone = "UTC",
            [FromQuery] CustomEventTiming timing = CustomEventTiming.EXACT)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var result = await this.alexaService.ProcessCarePlanRequest(idOrEmail, date, timing, timezone);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpGet]
        [Route("{idOrEmail}/alexa/next")]
        public async Task<IActionResult> GetAlexaNextRequest([FromRoute] string idOrEmail,
            [FromQuery] AlexaRequestType type)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                Bundle result;
                if (type == AlexaRequestType.CarePlan)
                {
                    result = await this.alexaService.GetNextRequests(idOrEmail);
                }
                else
                {
                    result = await this.alexaService.GetNextRequests(idOrEmail, type);
                }

                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }
    }
}