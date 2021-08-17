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
    [Route("medications/")]
    public class MedicationController : ControllerBase
    {
        private readonly IMedicationService medicationService;
        private readonly ILogger<MedicationController> logger;

        public MedicationController(IMedicationService medicationService, ILogger<MedicationController> logger)
        {
            this.medicationService = medicationService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMedications()
        {
            try
            {
                var result = await this.medicationService.GetMedicationList();
                return this.Ok(result.ToJObject());
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "Error getting medications");
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetMedicationRequest([FromRoute] string id)
        {
            try
            {
                var result = await this.medicationService.GetSingleMedication(id);
                return this.Ok(result.ToJObject());
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning($"Medication with ID: {id} Not Found");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, $"Error getting medication with ID: {id}");
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
                    var parsedRequest = await parser.ParseAsync<Medication>(request.ToString());
                    var result = await this.medicationService.CreateMedication(parsedRequest);
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
                this.logger.LogError(exception, "Error trying to create a medication");
                return this.BadRequest();
            }
        }
    }
}