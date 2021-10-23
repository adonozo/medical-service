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
    [Route("medicationRequests/")]
    public class MedicationRequestController : ControllerBase
    {
        private readonly IMedicationRequestService medicationRequestService;
        private readonly ILogger<MedicationRequestController> logger;

        public MedicationRequestController(ILogger<MedicationRequestController> logger, IMedicationRequestService medicationRequestService)
        {
            this.logger = logger;
            this.medicationRequestService = medicationRequestService;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetMedicationRequest([FromRoute] string id)
        {
            try
            {
                var result = await this.medicationRequestService.GetMedicationRequest(id);
                return this.Ok(result.ToJObject());
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("MedicationRequest with ID: {Id} Not Found", id);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "Error getting medicationRequest with ID: {Id}", id);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateMedicationRequest([FromBody] object request)
        {
            try
            {
                var parser = new FhirJsonParser(new ParserSettings
                    {AllowUnrecognizedEnums = true, AcceptUnknownMembers = true, PermissiveParsing = true});
                try
                {
                    var parsedRequest = await parser.ParseAsync<MedicationRequest>(request.ToString());
                    var result = await this.medicationRequestService.CreateMedicationRequest(parsedRequest);
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
                this.logger.LogError(exception, "Error trying to create a medicationRequest");
                return this.BadRequest();
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateMedicationRequest([FromRoute] string id, [FromBody] MedicationRequest request)
        {
            try
            {
                var result = await this.medicationRequestService.UpdateMedicationRequest(id, request);
                return this.Accepted(result);
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("MedicationRequest with ID: {Id} Not Found", id);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "Error updating medicationRequest with ID: {Id}", id);
                return this.BadRequest();
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteMedicationRequest([FromRoute] string id)
        {
            try
            {
                await this.medicationRequestService.DeleteMedicationRequest(id);
                return this.NoContent();
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("MedicationRequest with ID: {Id} Not Found", id);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "Error deleting medicationRequest with ID: {Id}", id);
                return this.BadRequest();
            }
        }
    }
}