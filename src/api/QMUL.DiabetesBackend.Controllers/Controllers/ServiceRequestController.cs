namespace QMUL.DiabetesBackend.Controllers.Controllers;

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ServiceInterfaces;
using ServiceInterfaces.Validators;
using Utils;

[ApiController]
public class ServiceRequestController : ControllerBase
{
    private readonly IServiceRequestService serviceRequestService;
    private readonly IResourceValidator<ServiceRequest> validator;
    private readonly ILogger<ServiceRequestController> logger;

    public ServiceRequestController(IServiceRequestService serviceRequestService,
        IResourceValidator<ServiceRequest> validator, ILogger<ServiceRequestController> logger)
    {
        this.serviceRequestService = serviceRequestService;
        this.validator = validator;
        this.logger = logger;
    }

    [HttpGet("serviceRequests/{id}")]
    public async Task<IActionResult> GetServiceRequest(string id)
    {
        var serviceRequest = await this.serviceRequestService.GetServiceRequest(id);
        return this.OkOrNotFound(serviceRequest);
    }

    // TODO no longer required? there is another one in carePlanController 
    [HttpPost("serviceRequests")]
    public async Task<IActionResult> CreateServiceRequest([FromBody] JObject request)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
        {
            var serviceRequest = await this.validator.ParseAndValidateAsync(request);
            var result = await this.serviceRequestService.CreateServiceRequest(serviceRequest);
            return this.Ok(result.ToJObject());
        }, this.logger, this);
    }

    [HttpPut("serviceRequests/{id}")]
    public async Task<IActionResult> UpdateServiceRequest([FromRoute] string id, [FromBody] JObject request)
    {
        return await ExceptionHandler.ExecuteAndHandleAsync<IActionResult>(async () =>
        {
            var serviceRequest = await this.validator.ParseAndValidateAsync(request);
            var resourceUpdated = await this.serviceRequestService.UpdateServiceRequest(id, serviceRequest);
            return resourceUpdated? this.Accepted() : this.StatusCode(StatusCodes.Status500InternalServerError);
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