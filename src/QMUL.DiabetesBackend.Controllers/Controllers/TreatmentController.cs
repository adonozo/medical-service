using System;
using Microsoft.AspNetCore.Mvc;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.Api.Controllers
{
    [ApiController]
    [Route("patients/")]
    public class TreatmentController : ControllerBase
    {
        private readonly ITreatmentService treatmentService;

        public TreatmentController(ITreatmentService treatmentService)
        {
            this.treatmentService = treatmentService;
        }

        [HttpGet]
        [Route("{patientId}/treatments/")]
        public IActionResult GetPatientTreatments([FromRoute] string patientId)
        {
            if (!Guid.TryParse(patientId, out var patientGuid))
            {
                return this.BadRequest();
            }

            var treatments = this.treatmentService.GetPatientTreatments(patientGuid);
            return this.Ok(treatments);
        }
        
        [HttpGet]
        [Route("{patientId}/treatments/{treatmentId}")]
        public IActionResult GetPatientTreatment([FromRoute] string patientId, [FromRoute] string treatmentId)
        {
            if (!Guid.TryParse(patientId, out var patientGuid) || !Guid.TryParse(treatmentId, out var treatmentGuid))
            {
                return this.BadRequest();
            }
            
            var treatment = this.treatmentService.GetSinglePatientTreatment(treatmentGuid);
            return this.Ok(treatment);
        }
    }
}