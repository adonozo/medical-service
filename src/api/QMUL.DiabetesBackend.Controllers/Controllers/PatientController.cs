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
    using Models;
    using ServiceInterfaces;
    using Patient = Model.Patient;

    [ApiController]
    [Route("patients/")]
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

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreatePatient([FromBody] Patient newPatient)
        {
            this.logger.LogDebug("Creating patient: {FirstName} {LastName}", newPatient.FirstName, newPatient.LastName);
            var createdPatient = await this.patientService.CreatePatient(newPatient);
            this.logger.LogDebug("Patient created with ID: {Id}", createdPatient.Id);
            return this.Ok(createdPatient);
        }

        [HttpPost]
        [Route("{idOrEmail}/observations")]
        public async Task<IActionResult> PostGlucoseObservation([FromRoute] string idOrEmail,
            [FromBody] object newObservation)
        {
            var parser = new FhirJsonParser(new ParserSettings
                {AllowUnrecognizedEnums = true, AcceptUnknownMembers = true, PermissiveParsing = true});
            try
            {
                var parsedRequest = await parser.ParseAsync<Observation>(newObservation.ToString());
                var result = await this.observationService.CreateObservation(idOrEmail, parsedRequest);
                return this.Ok(result.ToJObject());
            }
            catch (FormatException exception)
            {
                this.logger.LogWarning(exception, "Couldn't parse the request");
                return this.BadRequest();
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("Patient not found: {IdOrEmail}", idOrEmail);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Error trying to create a serviceRequest");
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetPatients()
        {
            this.logger.LogDebug("Getting patients list");
            var patients = await this.patientService.GetPatientList();
            this.logger.LogDebug("Found {PatientsCount} patients", patients.Count);
            return this.Ok(patients);
        }

        [HttpGet]
        [Route("{idOrEmail}")]
        public async Task<IActionResult> GetPatient([FromRoute] string idOrEmail)
        {
            try
            {
                var result = await this.patientService.GetPatient(idOrEmail);
                return this.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("Patient not found: {IdOrEmail}", idOrEmail);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Error getting Patient: {IdOrEmail}", idOrEmail);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [Route("{idOrEmail}/carePlans")]
        public async Task<IActionResult> GetPatientCarePlans([FromRoute] string idOrEmail)
        {
            try
            {
                var result = await this.carePlanService.GetCarePlanFor(idOrEmail);
                return this.Ok(result.ToJObject());
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("Patient not found: {IdOrEmail}", idOrEmail);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Error getting Patient: {IdOrEmail}", idOrEmail);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [Route("{idOrEmail}/medicationRequests/active")]
        public async Task<IActionResult> GetActiveMedicationRequests([FromRoute] string idOrEmail)
        {
            try
            {
                var result = await this.medicationRequestService.GetActiveMedicationRequests(idOrEmail);
                return this.Ok(result.ToJObject());
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("Patient not found: {IdOrEmail}", idOrEmail);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Error getting Patient: {IdOrEmail}", idOrEmail);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [Route("{idOrEmail}/carePlans/active")]
        public async Task<IActionResult> GetActiveCarePlan([FromRoute] string idOrEmail)
        {
            try
            {
                var result = await this.carePlanService.GetActiveCarePlans(idOrEmail);
                return this.Ok(result.ToJObject());
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("Patient not found: {IdOrEmail}", idOrEmail);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Error getting active care plans for: {IdOrEmail}", idOrEmail);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [Route("{idOrEmail}/observations/{observationId}")]
        public async Task<IActionResult> GetSingleObservation([FromRoute] string idOrEmail, [FromRoute] string observationId)
        {
            try
            {
                var result = await this.observationService.GetSingleObservation(observationId);
                return this.Ok(result.ToJObject());
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("Observation not found: {ObservationId}", observationId);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Error processing the request for: {IdOrEmail}", idOrEmail);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [Route("{idOrEmail}/observations/")]
        public async Task<IActionResult> GetPatientObservations([FromRoute] string idOrEmail, [FromQuery] DateTime date,
            [FromQuery] string timezone = "UTC",
            [FromQuery] CustomEventTiming timing = CustomEventTiming.EXACT)
        {
            try
            {
                var result = await this.observationService.GetObservationsFor(idOrEmail, timing, date, timezone);
                return this.Ok(result.ToJObject());
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("Patient not found: {IdOrEmail}", idOrEmail);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Error processing the request for: {IdOrEmail}", idOrEmail);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        
        [HttpGet]
        [Route("{idOrEmail}/all/observations/")]
        public async Task<IActionResult> GetAllPatientObservations([FromRoute] string idOrEmail)
        {
            try
            {
                var result = await this.observationService.GetAllObservationsFor(idOrEmail);
                return this.Ok(result.ToJObject());
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("Patient not found: {IdOrEmail}", idOrEmail);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Error processing the request for: {IdOrEmail}", idOrEmail);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut]
        [Route("{idOrEmail}/timing")]
        public async Task<IActionResult> UpdatePatientTiming([FromRoute] string idOrEmail, [FromBody] PatientTimingRequest request) 
        {
            try
            {
                var result = await this.alexaService.UpsertTimingEvent(idOrEmail, request.Timing, request.DateTime);
                return result ? this.NoContent() : this.BadRequest();
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("Patient not found: {IdOrEmail}", idOrEmail);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Error updating the timing for: {IdOrEmail}", idOrEmail);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut]
        [Route("{idOrEmail}/dosage/{dosageId}/startDate")]
        public async Task<IActionResult> UpdateDosageStartDate([FromRoute] string idOrEmail,
            [FromRoute] string dosageId, [FromBody] PatientStartDateRequest startDate)
        {
            try
            {
                var result = await this.alexaService.UpsertDosageStartDate(idOrEmail, dosageId, startDate.StartDate);
                return result ? this.NoContent() : this.BadRequest();
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("Patient not found: {IdOrEmail}", idOrEmail);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Error updating the timing for: {IdOrEmail}", idOrEmail);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}