namespace QMUL.DiabetesBackend.Api.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Serialization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Model;
    using Model.Enums;
    using Models;
    using Newtonsoft.Json.Linq;
    using ServiceInterfaces;
    using Utils;

    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService patientService;
        private readonly IAlexaService alexaService;
        private readonly ICarePlanService carePlanService;
        private readonly IObservationService observationService;
        private readonly IMedicationRequestService medicationRequestService;
        private readonly ILogger<PatientController> logger;

        public PatientController(IPatientService patientService, IAlexaService alexaService,
            ICarePlanService carePlanService, IObservationService observationService,
            IMedicationRequestService medicationRequestService, ILogger<PatientController> logger)
        {
            this.patientService = patientService;
            this.alexaService = alexaService;
            this.logger = logger;
            this.carePlanService = carePlanService;
            this.observationService = observationService;
            this.medicationRequestService = medicationRequestService;
        }

        [HttpPost("patients")]
        public async Task<IActionResult> CreatePatient([FromBody] JObject patient)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                this.logger.LogDebug("Creating patient");
                patient.Property("id")?.Remove();
                var newPatient = await Helpers.ParseResourceAsync<Patient>(patient);

                var createdPatient = await this.patientService.CreatePatient(newPatient);
                this.logger.LogDebug("Patient created with ID: {Id}", createdPatient.Id);
                return this.Ok(createdPatient.ToJObject());
            }, this.logger, this);
        }

        [HttpPost("patients/{idOrEmail}/observations")]
        public async Task<IActionResult> PostGlucoseObservation([FromRoute] string idOrEmail,
            [FromBody] JObject newObservation)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var observation = await Helpers.ParseResourceAsync<Observation>(newObservation);
                var result = await this.observationService.CreateObservation(idOrEmail, observation);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpGet("patients")]
        public async Task<IActionResult> GetPatients([FromQuery] int? limit = null, [FromQuery] string after = null)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                this.logger.LogDebug("Getting patients list");
                var pagination = new PaginationRequest(limit, after);
                var paginatedResult = await this.patientService.GetPatientList(pagination);

                this.HttpContext.SetPaginatedResult(paginatedResult);
                return this.Ok(paginatedResult.Results.ToJObject());
            }, this.logger, this);
        }

        [HttpGet("patients/{idOrEmail}")]
        public async Task<IActionResult> GetPatient([FromRoute] string idOrEmail)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var result = await this.patientService.GetPatient(idOrEmail);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpGet("patients/{idOrEmail}/carePlans")]
        [Obsolete("This merges services and medication requests. Won't work with pagination. " +
                  "Use individual requests instead.")]
        public async Task<IActionResult> GetPatientCarePlans([FromRoute] string idOrEmail)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var result = await this.carePlanService.GetCarePlanFor(idOrEmail);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpGet("patients/{idOrEmail}/medicationRequests/active")]
        public async Task<IActionResult> GetActiveMedicationRequests([FromRoute] string idOrEmail,
            [FromQuery] int? limit = null, [FromQuery] string after = null)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var paginationRequest = new PaginationRequest(limit, after);
                var paginatedResult = await this.medicationRequestService.GetActiveMedicationRequests(idOrEmail, paginationRequest);

                this.HttpContext.SetPaginatedResult(paginatedResult);
                return this.Ok(paginatedResult.Results.ToJObject());
            }, this.logger, this);
        }

        // Alexa endpoint
        [HttpGet("patients/{idOrEmail}/carePlans/active")]
        public async Task<IActionResult> GetActiveCarePlan([FromRoute] string idOrEmail)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var result = await this.carePlanService.GetActiveCarePlans(idOrEmail);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpGet("patients/{idOrEmail}/observations/{observationId}")]
        public async Task<IActionResult> GetSingleObservation([FromRoute] string idOrEmail,
            [FromRoute] string observationId)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var result = await this.observationService.GetSingleObservation(observationId);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpGet("patients/{idOrEmail}/observations/")]
        public async Task<IActionResult> GetPatientObservations([FromRoute] string idOrEmail, [FromQuery] DateTime date,
            [FromQuery] string timezone = "UTC",
            [FromQuery] CustomEventTiming timing = CustomEventTiming.EXACT,
            [FromQuery] int? limit = null, 
            [FromQuery] string after = null)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var pagination = new PaginationRequest(limit, after);
                var paginatedResult =
                    await this.observationService.GetObservationsFor(idOrEmail, timing, date, pagination, timezone);

                this.HttpContext.SetPaginatedResult(paginatedResult);
                return this.Ok(paginatedResult.Results.ToJObject());
            }, this.logger, this);
        }

        [HttpGet("patients/{idOrEmail}/all/observations/")]
        public async Task<IActionResult> GetAllPatientObservations([FromRoute] string idOrEmail,
            [FromQuery] int? limit = null, 
            [FromQuery] string after = null)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var pagination = new PaginationRequest(limit, after);
                var paginatedResult = await this.observationService.GetAllObservationsFor(idOrEmail, pagination);

                this.HttpContext.SetPaginatedResult(paginatedResult);
                return this.Ok(paginatedResult.Results.ToJObject());
            }, this.logger, this);
        }

        [HttpPut("patients/{idOrEmail}")]
        public async Task<IActionResult> UpdatePatient([FromRoute] string idOrEmail,
            [FromBody] JObject updatedPatient)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var patient = await Helpers.ParseResourceAsync<Patient>(updatedPatient);
                var result = await this.patientService.UpdatePatient(idOrEmail, patient);
                return this.Accepted(result.ToJObject());
            }, this.logger, this);
        }

        [HttpPut("patients/{idOrEmail}/timing")]
        public async Task<IActionResult> UpdatePatientTiming([FromRoute] string idOrEmail,
            [FromBody] PatientTimingRequest request)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync<IActionResult>(async () =>
            {
                var result = await this.alexaService.UpsertTimingEvent(idOrEmail, request.Timing, request.DateTime);
                return result ? this.NoContent() : this.BadRequest();
            }, this.logger, this);
        }

        [HttpPut("patients/{idOrEmail}/dosage/{dosageId}/startDate")]
        public async Task<IActionResult> UpdateDosageStartDate([FromRoute] string idOrEmail,
            [FromRoute] string dosageId, [FromBody] PatientStartDateRequest startDate)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync<IActionResult>(async () =>
            {
                var result = await this.alexaService.UpsertDosageStartDate(idOrEmail, dosageId, startDate.StartDate);
                return result ? this.NoContent() : this.BadRequest();
            }, this.logger, this);
        }
        
        [HttpPatch("patients/{idOrEmail}")]
        public async Task<IActionResult> PatchPatient([FromRoute] string idOrEmail,
            [FromBody] InternalPatient updatedPatient)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var result = await this.patientService.PatchPatient(idOrEmail, updatedPatient);
                return this.Accepted(result.ToJObject());
            }, this.logger, this);
        }
    }
}