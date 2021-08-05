using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QMUL.DiabetesBackend.Api.Models;
using QMUL.DiabetesBackend.Model.Enums;
using QMUL.DiabetesBackend.ServiceInterfaces;
using Patient = QMUL.DiabetesBackend.Model.Patient;

namespace QMUL.DiabetesBackend.Api.Controllers
{
    [ApiController]
    [Route("patients/")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService patientService;
        private readonly IAlexaService alexaService;
        private readonly ICarePlanService carePlanService;
        private readonly IObservationService observationService;
        private readonly ILogger<PatientController> logger;
        private static string jsonEcho = string.Empty;

        public PatientController(IPatientService patientService, IAlexaService alexaService,
            ILogger<PatientController> logger, ICarePlanService carePlanService, IObservationService observationService)
        {
            this.patientService = patientService;
            this.alexaService = alexaService;
            this.logger = logger;
            this.carePlanService = carePlanService;
            this.observationService = observationService;
        }

        [HttpPost]
        public async Task<ActionResult<Patient>> CreatePatient([FromBody] Patient newPatient)
        {
            this.logger.LogDebug($"Creating patient: {newPatient.FirstName} {newPatient.LastName}");
            var createdPatient = await this.patientService.CreatePatient(newPatient);
            this.logger.LogDebug($"Patient created with ID: {createdPatient.Id}");
            return this.Ok(createdPatient);
        }

        [HttpPost]
        [Route("{idOrEmail}/observations")]
        public async Task<IActionResult> PostGlucoseObservation([FromRoute] string idOrEmail,
            [FromBody] object newObservation)
        {
            try
            {
                var parser = new FhirJsonParser(new ParserSettings
                    {AllowUnrecognizedEnums = true, AcceptUnknownMembers = true, PermissiveParsing = true});
                try
                {
                    var parsedRequest = await parser.ParseAsync<Observation>(newObservation.ToString());
                    var result = await this.observationService.CreateObservation(parsedRequest);
                    return this.Ok(result.ToJObject());
                }
                catch (Exception exception)
                {
                    this.logger.LogError(exception, "Couldn't parse the request");
                    return this.BadRequest();
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "Error trying to create a serviceRequest");
                return this.BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPatients()
        {
            this.logger.LogDebug("Getting patients list");
            var patients = await this.patientService.GetPatientList();
            this.logger.LogDebug($"Found {patients.Count} patients");
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
                this.logger.LogWarning($"Patient not found: {idOrEmail}");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, $"Error getting Patient: {idOrEmail}");
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [Route("{idOrEmail}/carePlans")]
        public async Task<IActionResult> GetPatientCarePlans([FromRoute] string idOrEmail)
        {
            try
            {
                var result = await this.patientService.GetPatientCarePlans(idOrEmail);
                return this.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning($"Patient not found: {idOrEmail}");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError($"Error getting Patient: {idOrEmail}", exception);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [Route("{idOrEmail}/medicationRequests/active")]
        public async Task<IActionResult> GetActiveMedicationRequests([FromRoute] string idOrEmail)
        {
            try
            {
                var result = await this.patientService.GetActiveMedicationRequests(idOrEmail);
                return this.Ok(result.ToJObject());
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning($"Patient not found: {idOrEmail}");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, $"Error getting Patient: {idOrEmail}");
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
                this.logger.LogWarning($"Patient not found: {idOrEmail}");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, $"Error getting active care plans for: {idOrEmail}");
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [Route("{idOrEmail}/alexa")]
        public async Task<IActionResult> GetAlexaRequest([FromRoute] string idOrEmail, [FromQuery] AlexaRequestType type,
            [FromQuery] DateTime date,
            [FromQuery] AlexaRequestTime requestTime = AlexaRequestTime.ExactTime,
            [FromQuery] CustomEventTiming timing = CustomEventTiming.EXACT)
        {
            try
            {
                var result = await this.alexaService.ProcessRequest(idOrEmail, type, date, requestTime, timing);
                return this.Ok(result.ToJObject());
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning($"Patient not found: {idOrEmail}");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, $"Error processing the request for: {idOrEmail}");
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
                this.logger.LogWarning($"Observation not found: {observationId}");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, $"Error processing the request for: {idOrEmail}");
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [Route("{idOrEmail}/observations/")]
        public async Task<IActionResult> GetPatientObservations([FromRoute] string idOrEmail, [FromQuery] DateTime date,
            [FromQuery] CustomEventTiming timing = CustomEventTiming.EXACT)
        {
            try
            {
                var result = await this.observationService.GetObservationsFor(idOrEmail, timing, date);
                return this.Ok(result.ToJObject());
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning($"Patient not found: {idOrEmail}");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, $"Error processing the request for: {idOrEmail}");
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
                this.logger.LogWarning($"Patient not found: {idOrEmail}");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, $"Error updating the timing for: {idOrEmail}");
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
                this.logger.LogWarning($"Patient not found: {idOrEmail}");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, $"Error updating the timing for: {idOrEmail}");
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("echo")]
        public IActionResult SaveJsonEcho([FromBody] object jsonObject)
        {
            this.logger.LogDebug("Echoing a message");
            jsonEcho = JsonSerializer.Serialize(jsonObject);
            return this.Accepted();
        }

        [HttpGet]
        [Route("echo")]
        public ActionResult<string> GetJsonEcho()
        {
            return this.Ok(jsonEcho);
        } 
    }
}