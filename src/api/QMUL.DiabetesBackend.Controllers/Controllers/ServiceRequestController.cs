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
    [Route("serviceRequests/")]
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

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetServiceRequest(string id)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var result = await this.serviceRequestService.GetServiceRequest(id);
                return this.Ok(result.ToJObject());
            }, this.logger, this);
        }

        [HttpPost]
        [Route("")]
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

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateServiceRequest([FromRoute] string id, [FromBody] object request)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
            {
                var parser = new FhirJsonParser(new ParserSettings
                    {AllowUnrecognizedEnums = true, AcceptUnknownMembers = true, PermissiveParsing = true});
                var parsedRequest = await parser.ParseAsync<ServiceRequest>(request.ToString());
                var result = await this.serviceRequestService.UpdateServiceRequest(id, parsedRequest);
                return this.Accepted(result);
            }, this.logger, this);
        }

        [HttpDelete]
        [Route("{id}")]
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