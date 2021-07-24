using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<PatientController> logger;
        private static string jsonEcho = string.Empty;

        public PatientController(IPatientService patientService, ILogger<PatientController> logger)
        {
            this.patientService = patientService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetPatients()
        {
            this.logger.LogDebug("Getting patients list");
            var patients = await this.patientService.GetPatientList();
            this.logger.LogDebug($"Found {patients.Count} patients");
            return this.Ok(patients);
        }

        [HttpPost]
        public ActionResult<Patient> CreatePatient([FromBody] Patient newPatient)
        {
            this.logger.LogDebug($"Creating patient: {newPatient.FirstName} {newPatient.LastName}");
            var createdPatient = this.patientService.CreatePatient(newPatient);
            this.logger.LogDebug($"Patient created with ID: ${createdPatient.Id.ToString()}");
            return this.Ok(createdPatient);
        }

        [HttpGet]
        [Route("{idOrEmail}")]
        public IActionResult GetPatient([FromRoute] string idOrEmail)
        {
            try
            {
                var result = this.patientService.GetPatient(idOrEmail);
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
        [Route("{idOrEmail}/carePlans")]
        public IActionResult GetPatientCarePlans([FromRoute] string idOrEmail)
        {
            try
            {
                var result = this.patientService.GetPatientCarePlans(idOrEmail);
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
        [Route("{emailOrId}/alexa")]
        public IActionResult GetAlexaRequestWithTiming([FromRoute] string emailOrId, [FromQuery] AlexaRequestType type,
            [FromQuery] DateTime date,
            [FromQuery] AlexaRequestTime requestTime = AlexaRequestTime.OnEvent,
            [FromQuery] Timing.EventTiming timing = Timing.EventTiming.MORN)
        {
            /* TODO implement methods to look for medicationRequests / serviceRequests / carePlans / appointments                 
             */
            return this.Ok(new { emailOrId, type, date, requestTime, timing });
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