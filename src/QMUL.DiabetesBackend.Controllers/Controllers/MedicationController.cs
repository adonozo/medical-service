using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.Api.Controllers
{
    [ApiController]
    [Route("medications/")]
    public class MedicationController : ControllerBase
    {
        private readonly IMedicationService medicationService;
        private readonly ILogger<MedicationController> logger;

        public MedicationController(IMedicationService medicationService, ILogger<MedicationController> logger)
        {
            this.medicationService = medicationService;
            this.logger = logger;
        }

        [HttpGet]
        public ActionResult<List<Medication>> GetMedicationList()
        {
            this.logger.LogDebug("Getting medication list");
            var medications = this.medicationService.GetMedicationList();
            this.logger.LogDebug($"Found: {medications.Count} medications");
            return this.Ok(medications);
        }
    }
}