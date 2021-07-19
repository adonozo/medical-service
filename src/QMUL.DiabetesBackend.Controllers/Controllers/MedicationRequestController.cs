using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QMUL.DiabetesBackend.ServiceImpl.Implementations;

namespace QMUL.DiabetesBackend.Api.Controllers
{
    [ApiController]
    [Route("medicationRequests/")]
    public class MedicationRequestController : ControllerBase
    {
        private readonly MedicationRequestService medicationRequestService;
        private readonly ILogger<MedicationRequestController> logger;

        public MedicationRequestController(ILogger<MedicationRequestController> logger, MedicationRequestService medicationRequestService)
        {
            this.logger = logger;
            this.medicationRequestService = medicationRequestService;
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetMedicationRequest([FromRoute] string id)
        {
            try
            {
                var result = this.medicationRequestService.GetMedicationRequest(id);
                return this.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning($"MedicationRequest with ID: {id} Not Found");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError($"Error getting medicationRequest with ID: {id}", exception);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public IActionResult CreateMedicationRequest([FromBody] MedicationRequest request)
        {
            try
            {
                var result = this.medicationRequestService.CreateMedicationRequest(request);
                return this.Ok(result);
            }
            catch (Exception exception)
            {
                this.logger.LogError("Error trying to create a medicationRequest", exception);
                return this.BadRequest();
            }
        }

        [HttpPut]
        [Route("{id}")]
        public IActionResult UpdateMedicationRequest([FromRoute] string id, [FromBody] MedicationRequest request)
        {
            try
            {
                var result = this.medicationRequestService.UpdateMedicationRequest(id, request);
                return this.Accepted(result);
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning($"MedicationRequest with ID: {id} Not Found");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError($"Error updating medicationRequest with ID: {id}", exception);
                return this.BadRequest();
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public IActionResult DeleteMedicationRequest([FromRoute] string id)
        {
            try
            {
                this.medicationRequestService.DeleteMedicationRequest(id);
                return this.NoContent();
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning($"MedicationRequest with ID: {id} Not Found");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError($"Error deleting medicationRequest with ID: {id}", exception);
                return this.BadRequest();
            }
        }
    }
}