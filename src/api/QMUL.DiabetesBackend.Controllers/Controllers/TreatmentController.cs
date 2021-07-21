using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.Api.Controllers
{
    [ApiController]
    [Route("patients/{patientId}/treatments")]
    public class TreatmentController : ControllerBase
    {
        private readonly ITreatmentService treatmentService;
        private ILogger<TreatmentController> logger;

        public TreatmentController(ITreatmentService treatmentService, ILogger<TreatmentController> logger)
        {
            this.treatmentService = treatmentService;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult GetPatientTreatments([FromRoute] string patientId)
        {
            this.logger.LogDebug($"Getting treatments for patient with ID: {patientId}");
            if (!Guid.TryParse(patientId, out var patientGuid))
            {
                this.logger.LogDebug("Patient not found");
                return this.BadRequest();
            }

            var treatments = this.treatmentService.GetPatientTreatments(patientGuid);
            this.logger.LogDebug($"Found {treatments.Count} treatments for patient with ID {patientId}");
            return this.Ok(treatments);
        }
        
        [HttpGet]
        [Route("{treatmentId}")]
        public IActionResult GetPatientTreatment([FromRoute] string patientId, [FromRoute] string treatmentId)
        {
            this.logger.LogDebug($"Getting treatment with ID: {treatmentId}");
            if (!Guid.TryParse(patientId, out var patientGuid) || !Guid.TryParse(treatmentId, out var treatmentGuid))
            {
                this.logger.LogDebug("Patient or treatment not found");
                return this.BadRequest();
            }
            
            var treatment = this.treatmentService.GetSinglePatientTreatment(treatmentGuid);
            this.logger.LogDebug($"Treatment with ID {treatmentId} found");
            return this.Ok(treatment);
        }
    }
}