namespace QMUL.DiabetesBackend.Api.Controllers
{
    using System;
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
        private readonly ILogger<ObservationController> logger;

        public ObservationController(IObservationService observationService,
            IResourceValidator<Observation> observationValidator, ILogger<ObservationController> logger)
        {
            this.observationService = observationService;
            this.observationValidator = observationValidator;
            this.logger = logger;
        }

        [HttpGet("observations")]
        public async Task<IActionResult> GetObservations([FromQuery] string patientIdOrEmail = null,
            [FromQuery] int? limit = null, [FromQuery] string after = null)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var pagination = new PaginationRequest(limit, after);
                var paginatedResult = await this.observationService.GetObservations(pagination, patientIdOrEmail);

                this.HttpContext.SetPaginatedResult(paginatedResult);
                return this.Ok(paginatedResult.Results.ToJObject());
            }, this.logger, this);
        }

        [HttpGet("observations/{id}")]
        public async Task<IActionResult> GetObservation([FromRoute] string id)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var observation = await this.observationService.GetObservation(id);
                return this.Ok(observation.ToJObject());
            }, this.logger, this);
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
                return this.Ok(updatedObservation.ToJObject());
            }, this.logger, this);
        }

        [HttpPatch("observations/{id}/value")]
        public async Task<IActionResult> PatchObservationValue([FromRoute] string id, [FromBody] JObject request)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                throw new NotImplementedException();
                return this.Ok();
            }, this.logger, this);
        }
    }
}