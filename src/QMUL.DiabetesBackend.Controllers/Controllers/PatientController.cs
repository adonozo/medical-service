using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.Api.Controllers
{
    [ApiController]
    [Route("patients/")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService patientService;

        public PatientController(IPatientService patientService)
        {
            this.patientService = patientService;
        }

        [HttpGet]
        public ActionResult<List<Patient>> GetPatients()
        {
            var patients = this.patientService.GetPatientList();
            return this.Ok(patients);
        }

        [HttpPost]
        public ActionResult<Patient> CreatePatient([FromBody] Patient newPatient)
        {
            var createdPatient = this.patientService.CreatePatient(newPatient);
            return this.Ok(createdPatient);
        }
    }
}