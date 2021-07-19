using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.ServiceInterfaces;

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
        public ActionResult<List<Patient>> GetPatients()
        {
            this.logger.LogDebug("Getting patients list");
            var patients = this.patientService.GetPatientList();
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
        [Route("patients/{id}/alexa")]
        public IActionResult GetAlexaRequest([FromQuery] string type, [FromQuery] DateTime date, [FromQuery] string timing)
        {
            throw new NotImplementedException();
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