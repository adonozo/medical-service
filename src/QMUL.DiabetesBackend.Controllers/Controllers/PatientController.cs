using System.Collections.Generic;
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
    }
}