using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.Api.Controllers
{
    [ApiController]
    [Route("carePlans/")]
    public class CarePlanController : ControllerBase
    {
        private readonly ICarePlanService carePlanService;
        private readonly ILogger<CarePlanController> logger;

        public CarePlanController(ICarePlanService carePlanService, ILogger<CarePlanController> logger)
        {
            this.carePlanService = carePlanService;
            this.logger = logger;
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetCarePlan([FromRoute] string id)
        {
            try
            {
                var result = this.carePlanService.GetCarePlan(id);
                return this.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning($"CarePlan with ID: {id} Not Found");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError($"Error getting serviceRequest with ID: {id}", exception);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public IActionResult CreateCarePlan([FromBody] CarePlan carePlan)
        {
            try
            {
                var result = this.carePlanService.CreateCarePlan(carePlan);
                return this.Ok(result);
            }
            catch (Exception exception)
            {
                this.logger.LogError("Error trying to create a carePlan", exception);
                return this.BadRequest();
            }
        }

        [HttpPut]
        [Route("{id}")]
        public IActionResult UpdateCarePlan([FromRoute] string id, [FromBody] CarePlan carePlan)
        {
            try
            {
                var result = this.carePlanService.UpdateCarePlan(id, carePlan);
                return this.Accepted(result);
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning($"CarePlan with ID: {id} Not Found");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError($"Error updating carePlan with ID: {id}", exception);
                return this.BadRequest();
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public IActionResult DeleteActionPlan([FromRoute] string id)
        {
            try
            {
                this.carePlanService.DeleteCarePlan(id);
                return this.NoContent();
            }
            catch (KeyNotFoundException)
            {
                this.logger.LogWarning($"CarePlan with ID: {id} Not Found");
                return this.NotFound();
            }
            catch (Exception exception)
            {
                this.logger.LogError($"Error deleting carePlan with ID: {id}", exception);
                return this.BadRequest();
            }
        }
    }
}