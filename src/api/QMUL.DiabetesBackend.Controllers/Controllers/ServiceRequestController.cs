namespace QMUL.DiabetesBackend.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Serialization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using ServiceInterfaces;

    [ApiController]
    [Route("serviceRequests/")]
    public class ServiceRequestController : ControllerBase
    {
        private readonly IServiceRequestService serviceRequestService;
        private readonly ILogger<ServiceRequestController> logger;

        public ServiceRequestController(IServiceRequestService serviceRequestService, ILogger<ServiceRequestController> logger)
        {
            this.serviceRequestService = serviceRequestService;
            this.logger = logger;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetServiceRequest(string id)
        {
            try
            {
                var result = await this.serviceRequestService.GetServiceRequest(id);
                return this.Ok(result.ToJObject());
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("ServiceRequest with ID: {Id} Not Found", id);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Error getting serviceRequest with ID: {Id}", id);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateServiceRequest([FromBody] object request)
        {

            var parser = new FhirJsonParser(new ParserSettings
                { AllowUnrecognizedEnums = true, AcceptUnknownMembers = true, PermissiveParsing = true });
            try
            {
                var parsedRequest = await parser.ParseAsync<ServiceRequest>(request.ToString());
                var result = await this.serviceRequestService.CreateServiceRequest(parsedRequest);
                return this.Ok(result.ToJObject());
            }
            catch (FormatException exception)
            {
                this.logger.LogWarning(exception, "Couldn't parse the request");
                return this.BadRequest();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Error trying to create a serviceRequest");
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateServiceRequest([FromRoute] string id, [FromBody] object request)
        {
            var parser = new FhirJsonParser(new ParserSettings
                { AllowUnrecognizedEnums = true, AcceptUnknownMembers = true, PermissiveParsing = true });
            try
            {
                var parsedRequest = await parser.ParseAsync<ServiceRequest>(request.ToString());
                var result = await this.serviceRequestService.UpdateServiceRequest(id, parsedRequest);
                return this.Accepted(result);
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("ServiceRequest with ID: {Id} Not Found", id);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Error updating serviceRequest with ID: {Id}", id);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteActionResult([FromRoute] string id)
        {
            try
            {
                await this.serviceRequestService.DeleteServiceRequest(id);
                return this.NoContent();
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("ServiceRequest with ID: {Id} Not Found", id);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Error deleting serviceRequest with ID: {Id}", id);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}