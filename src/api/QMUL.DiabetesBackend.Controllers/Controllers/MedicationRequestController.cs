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
    public class MedicationRequestController : ControllerBase
    {
        private readonly IMedicationRequestService medicationRequestService;
        private readonly ILogger<MedicationRequestController> logger;

        public MedicationRequestController(IMedicationRequestService medicationRequestService,
            ILogger<MedicationRequestController> logger)
        {
            this.logger = logger;
            this.medicationRequestService = medicationRequestService;
        }

        [HttpGet("medicationRequests/{id}")]
        public async Task<IActionResult> GetMedicationRequest([FromRoute] string id)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var result = await this.medicationRequestService.GetMedicationRequest(id);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpPost("medicationRequests")]
        public async Task<IActionResult> CreateMedicationRequest([FromBody] JObject request)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var parsedRequest = await Helpers.ParseResourceAsync<MedicationRequest>(request);
                var result = await this.medicationRequestService.CreateMedicationRequest(parsedRequest);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpPut("medicationRequests/{id}")]
        public async Task<IActionResult> UpdateMedicationRequest([FromRoute] string id, [FromBody] JObject request)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var parsedRequest = await Helpers.ParseResourceAsync<MedicationRequest>(request);
                var result = await this.medicationRequestService.UpdateMedicationRequest(id, parsedRequest);
                return this.Accepted(result.ToJObject());
            }, this.logger, this);
        }

        [HttpDelete("medicationRequests/{id}")]
        public async Task<IActionResult> DeleteMedicationRequest([FromRoute] string id)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                await this.medicationRequestService.DeleteMedicationRequest(id);
                return this.NoContent();
            }, this.logger, this);
        }
    }
}