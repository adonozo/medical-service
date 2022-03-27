namespace QMUL.DiabetesBackend.Api.Controllers
{
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Serialization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using ServiceInterfaces;
    using Utils;

    [ApiController]
    [Route("medicationRequests/")]
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

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetMedicationRequest([FromRoute] string id)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var result = await this.medicationRequestService.GetMedicationRequest(id);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateMedicationRequest([FromBody] object request)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var parser = new FhirJsonParser(new ParserSettings
                    {AllowUnrecognizedEnums = true, AcceptUnknownMembers = true, PermissiveParsing = true});
                var parsedRequest = await parser.ParseAsync<MedicationRequest>(request.ToString());
                var result = await this.medicationRequestService.CreateMedicationRequest(parsedRequest);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateMedicationRequest([FromRoute] string id, [FromBody] object request)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var parser = new FhirJsonParser(new ParserSettings
                    {AllowUnrecognizedEnums = true, AcceptUnknownMembers = true, PermissiveParsing = true});
                var parsedRequest = await parser.ParseAsync<MedicationRequest>(request.ToString());
                var result = await this.medicationRequestService.UpdateMedicationRequest(id, parsedRequest);
                return this.Accepted(result.ToJObject());
            }, this.logger, this);
        }

        [HttpDelete]
        [Route("{id}")]
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