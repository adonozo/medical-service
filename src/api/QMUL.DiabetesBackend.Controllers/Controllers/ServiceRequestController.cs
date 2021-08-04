using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.Api.Controllers
{
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
                this.logger.LogWarning($"ServiceRequest with ID: {id} Not Found");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError($"Error getting serviceRequest with ID: {id}", exception);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateServiceRequest([FromBody] object request)
        {
            try
            {
                var parser = new FhirJsonParser(new ParserSettings
                    {AllowUnrecognizedEnums = true, AcceptUnknownMembers = true, PermissiveParsing = true});
                try
                {
                    var parsedRequest = await parser.ParseAsync<ServiceRequest>(request.ToString());
                    var result = await this.serviceRequestService.CreateServiceRequest(parsedRequest);
                    return this.Ok(result.ToJObject());
                }
                catch (Exception exception)
                {
                    this.logger.LogError(exception, "Couldn't parse the request");
                    return this.BadRequest();
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "Error trying to create a serviceRequest");
                return this.BadRequest();
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateServiceRequest([FromRoute] string id, [FromBody] ServiceRequest request)
        {
            try
            {
                var result = await this.serviceRequestService.UpdateServiceRequest(id, request);
                return this.Accepted(result);
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning($"ServiceRequest with ID: {id} Not Found");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError($"Error updating serviceRequest with ID: {id}", exception);
                return this.BadRequest();
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
                this.logger.LogWarning($"ServiceRequest with ID: {id} Not Found");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError($"Error deleting serviceRequest with ID: {id}", exception);
                return this.BadRequest();
            }
        }
    }
}