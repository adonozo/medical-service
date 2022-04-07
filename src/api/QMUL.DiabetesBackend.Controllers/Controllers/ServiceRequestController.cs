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
    public class ServiceRequestController : ControllerBase
    {
        private readonly IServiceRequestService serviceRequestService;
        private readonly ILogger<ServiceRequestController> logger;

        public ServiceRequestController(IServiceRequestService serviceRequestService,
            ILogger<ServiceRequestController> logger)
        {
            this.serviceRequestService = serviceRequestService;
            this.logger = logger;
        }

        [HttpGet("serviceRequests/{id}")]
        public async Task<IActionResult> GetServiceRequest(string id)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var result = await this.serviceRequestService.GetServiceRequest(id);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpPost("serviceRequests")]
        public async Task<IActionResult> CreateServiceRequest([FromBody] object request)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var parser = new FhirJsonParser(new ParserSettings
                    {AllowUnrecognizedEnums = true, AcceptUnknownMembers = true, PermissiveParsing = true});
                var parsedRequest = await parser.ParseAsync<ServiceRequest>(request.ToString());
                var result = await this.serviceRequestService.CreateServiceRequest(parsedRequest);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpPut("serviceRequests/{id}")]
        public async Task<IActionResult> UpdateServiceRequest([FromRoute] string id, [FromBody] object request)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var parser = new FhirJsonParser(new ParserSettings
                    {AllowUnrecognizedEnums = true, AcceptUnknownMembers = true, PermissiveParsing = true});
                var parsedRequest = await parser.ParseAsync<ServiceRequest>(request.ToString());
                var result = await this.serviceRequestService.UpdateServiceRequest(id, parsedRequest);
                return this.Accepted(result.ToJObject());
            }, this.logger, this);
        }

        [HttpDelete("serviceRequests/{id}")]
        public async Task<IActionResult> DeleteActionResult([FromRoute] string id)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                await this.serviceRequestService.DeleteServiceRequest(id);
                return this.NoContent();
            }, this.logger, this);
        }
    }
}