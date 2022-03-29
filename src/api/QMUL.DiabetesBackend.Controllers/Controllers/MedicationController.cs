namespace QMUL.DiabetesBackend.Api.Controllers
{
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Serialization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using ServiceInterfaces;
    using Utils;

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
        [Route("")]
        public async Task<IActionResult> GetAllMedications()
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var result = await this.medicationService.GetMedicationList();
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetMedication([FromRoute] string id)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var result = await this.medicationService.GetSingleMedication(id);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateMedication([FromBody] JObject request)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var parsedRequest = await Helpers.ParseResourceAsync<Medication>(request);
                var result = await this.medicationService.CreateMedication(parsedRequest);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }
    }
}