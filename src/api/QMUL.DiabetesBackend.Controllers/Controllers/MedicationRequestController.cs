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

        public MedicationRequestController(IMedicationRequestService medicationRequestService, ILogger<MedicationRequestController> logger)
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
                this.logger.LogWarning(exception, "Error getting medicationRequest with ID: {Id}", id);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateMedicationRequest([FromBody] object request)
        {

            var parser = new FhirJsonParser(new ParserSettings
            { AllowUnrecognizedEnums = true, AcceptUnknownMembers = true, PermissiveParsing = true });
            try
            {
                var parsedRequest = await parser.ParseAsync<MedicationRequest>(request.ToString());
                var result = await this.medicationRequestService.CreateMedicationRequest(parsedRequest);
                return this.Ok(result.ToJObject());
            }
            catch (FormatException exception)
            {
                this.logger.LogWarning(exception, "Couldn't parse the request");
                return this.BadRequest();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Error trying to create a medicationRequest");
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateMedicationRequest([FromRoute] string id, [FromBody] object request)
        {
            var parser = new FhirJsonParser(new ParserSettings
                { AllowUnrecognizedEnums = true, AcceptUnknownMembers = true, PermissiveParsing = true });
            try
            {
                var parsedRequest = await parser.ParseAsync<MedicationRequest>(request.ToString());
                var result = await this.medicationRequestService.UpdateMedicationRequest(id, parsedRequest);
                return this.Accepted(result);
            }
            catch (FormatException exception)
            {
                this.logger.LogWarning(exception, "Couldn't parse the request");
                return this.BadRequest();
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning("MedicationRequest with ID: {Id} Not Found", id);
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Error updating medicationRequest with ID: {Id}", id);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
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
                this.logger.LogWarning(exception, "Error deleting medicationRequest with ID: {Id}", id);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}