namespace QMUL.DiabetesBackend.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Serialization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Model.Enums;
    using ServiceInterfaces;

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
        [Route("{idOrEmail}/alexa")]
        public async Task<IActionResult> GetAlexaRequest([FromRoute] string idOrEmail,
            [FromQuery] AlexaRequestType type,
            [FromQuery] DateTime date,
            [FromQuery] string timezone = "UTC",
            [FromQuery] CustomEventTiming timing = CustomEventTiming.EXACT)
        {
            try
            {
                var result = await this.alexaService.ProcessRequest(idOrEmail, type, date, timing, timezone);
                return this.Ok(result.ToJObject());
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("Patient not found: {IdOrEmail}", idOrEmail);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "Error processing the request for: {IdOrEmail}", idOrEmail);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [Route("{idOrEmail}/alexa/next")]
        public async Task<IActionResult> GetAlexaNextRequest([FromRoute] string idOrEmail,
            [FromQuery] AlexaRequestType type)
        {
            try
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
            }
            catch (KeyNotFoundException exception)
            {
                this.logger.LogWarning("Not found: {Message}", exception.Message);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "Error processing the request for: {IdOrEmail}", idOrEmail);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}