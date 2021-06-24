using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.Api.Controllers
{
    [ApiController]
    [Route("medications/")]
    public class MedicationController : ControllerBase
    {
        private readonly IMedicationService medicationService;

        public MedicationController(IMedicationService medicationService)
        {
            this.medicationService = medicationService;
        }

        [HttpGet]
        public ActionResult<List<Medication>> GetMedicationList()
        {
            var medications = this.medicationService.GetMedicationList();
            return this.Ok(medications);
        }
    }
}