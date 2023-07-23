namespace QMUL.DiabetesBackend.Controllers.Controllers;

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model;
using Models;
using Newtonsoft.Json.Linq;
using ServiceInterfaces;
using ServiceInterfaces.Validators;
using Utils;

[ApiController]
public class PatientController : ControllerBase
{
    private readonly IPatientService patientService;
    private readonly IAlexaService alexaService;
    private readonly ICarePlanService carePlanService;
    private readonly IObservationService observationService;
    private readonly IMedicationRequestService medicationRequestService;
    private readonly IResourceValidator<Observation> observationValidator;
    private readonly IResourceValidator<Patient> patientValidator;
    private readonly ILogger<PatientController> logger;

    public PatientController(IPatientService patientService,
        IAlexaService alexaService,
        ICarePlanService carePlanService,
        IObservationService observationService,
        IMedicationRequestService medicationRequestService,
        IResourceValidator<Observation> observationValidator,
        IResourceValidator<Patient> patientValidator,
        ILogger<PatientController> logger)
    {
        this.patientService = patientService;
        this.alexaService = alexaService;
        this.logger = logger;
        this.patientValidator = patientValidator;
        this.observationValidator = observationValidator;
        this.carePlanService = carePlanService;
        this.observationService = observationService;
        this.medicationRequestService = medicationRequestService;
    }

    #region POST

    [HttpPost("patients")]
    public async Task<IActionResult> CreatePatient([FromBody] JObject patient)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            this.logger.LogDebug("Creating patient");
            patient.Property("id")?.Remove();
            var newPatient = await this.patientValidator.ParseAndValidateAsync(patient);

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
            var observation = await this.observationValidator.ParseAndValidateAsync(newObservation);
            var result = await this.observationService.CreateObservation(observation, idOrEmail);
            return this.Ok(result.ToJObject());
        }, this.logger, this);
    }

    #endregion

    #region GET

    [HttpGet("patients")]
    public async Task<IActionResult> GetPatients([FromQuery] int? limit = null, [FromQuery] string? after = null)
    {
        var pagination = new PaginationRequest(limit, after);
        var paginatedResult = await this.patientService.GetPatientList(pagination);

        this.HttpContext.SetPaginatedResult(paginatedResult);
        return this.Ok(paginatedResult.Results.ToJObject());
    }

    [HttpGet("patients/{idOrEmail}")]
    public async Task<IActionResult> GetPatient([FromRoute] string idOrEmail)
    {
        var patient = await this.patientService.GetPatient(idOrEmail);
        return this.OkOrNotFound(patient);
    }

    [HttpGet("patients/{idOrEmail}/carePlans")]
    public Task<IActionResult> GetCarePlans([FromRoute] string idOrEmail,
        [FromQuery] int? limit = null, [FromQuery] string? after = null)
    {
        return ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            var paginationRequest = new PaginationRequest(limit, after);
            var paginatedResult = await this.carePlanService.GetCarePlansFor(idOrEmail, paginationRequest);

            this.HttpContext.SetPaginatedResult(paginatedResult);
            return this.Ok(paginatedResult.Results.ToJObject());
        }, this.logger, this);
    }

    [HttpGet("patients/{idOrEmail}/medicationRequests/active")]
    public async Task<IActionResult> GetActiveMedicationRequests([FromRoute] string idOrEmail,
        [FromQuery] int? limit = null, [FromQuery] string? after = null)
    {
        var paginationRequest = new PaginationRequest(limit, after);
        var paginatedResult =
            await this.medicationRequestService.GetActiveMedicationRequests(idOrEmail, paginationRequest);

        this.HttpContext.SetPaginatedResult(paginatedResult);
        return this.Ok(paginatedResult.Results.ToJObject());
    }

    // Alexa endpoint
    [HttpGet("patients/{idOrEmail}/carePlans/active")]
    public async Task<IActionResult> GetActiveCarePlan([FromRoute] string idOrEmail)
    {
        var result = await this.carePlanService.GetActiveCarePlans(idOrEmail);
        return this.OkOrNotFound(result);
    }

    [HttpGet("patients/{idOrEmail}/all/observations/")]
    public async Task<IActionResult> GetAllPatientObservations([FromRoute] string idOrEmail,
        [FromQuery] int? limit = null,
        [FromQuery] string? after = null)
    {
        var pagination = new PaginationRequest(limit, after);
        var paginatedResult = await this.observationService.GetObservations(idOrEmail, pagination);

        this.HttpContext.SetPaginatedResult(paginatedResult);
        return this.Ok(paginatedResult.Results.ToJObject());
    }

    #endregion

    #region PUT

    [HttpPut("patients/{idOrEmail}")]
    public async Task<IActionResult> UpdatePatient([FromRoute] string idOrEmail,
        [FromBody] JObject updatedPatient)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync<IActionResult>(async () =>
        {
            var patient = await this.patientValidator.ParseAndValidateAsync(updatedPatient);
            var patientUpdated = await this.patientService.UpdatePatient(idOrEmail, patient);
            return patientUpdated ? this.Accepted() : this.StatusCode(StatusCodes.Status500InternalServerError);
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
            await this.alexaService.UpsertDosageStartDate(idOrEmail, dosageId, startDate.StartDate);
            return this.NoContent();
        }, this.logger, this);
    }

    [HttpPut("patients/{idOrEmail}/serviceRequest/{serviceRequestId}/startDate")]
    public async Task<IActionResult> UpdateServiceRequestStartDate([FromRoute] string idOrEmail,
        [FromRoute] string serviceRequestId, [FromBody] PatientStartDateRequest startDate)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync<IActionResult>(async () =>
        {
            await this.alexaService.UpsertServiceRequestStartDate(idOrEmail, serviceRequestId, startDate.StartDate);
            return this.NoContent();
        }, this.logger, this);
    }

    #endregion

    [HttpPatch("patients/{idOrEmail}")]
    public async Task<IActionResult> PatchPatient([FromRoute] string idOrEmail,
        [FromBody] InternalPatient updatedPatient)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync<IActionResult>(async () =>
        {
            var patientUpdated = await this.patientService.PatchPatient(idOrEmail, updatedPatient);
            return patientUpdated ? this.Accepted() : this.StatusCode(StatusCodes.Status500InternalServerError);
        }, this.logger, this);
    }
}