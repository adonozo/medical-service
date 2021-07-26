using System;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace QMUL.DiabetesBackend.Api.Controllers
{
    [ApiController]
    [Route("valueSet/")]
    public class ValueSetController : ControllerBase
    {
        private ILogger<ValueSetController> logger;

        public ValueSetController(ILogger<ValueSetController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult GetTimeUnits([FromQuery] string type)
        {
            string[] result;
            switch (type)
            {
                case "unitsOfTime":
                    result = Enum.GetNames(typeof(Timing.UnitsOfTime));
                    break;
                case "dayEvent":
                    result = Enum.GetNames(typeof(Timing.EventTiming));
                    break;
                case "daysOfWeek":
                    result = Enum.GetNames(typeof(DaysOfWeek));
                    break;
                default:
                    result = new string[0];
                    break;
            }
            
            return this.Ok(result);
        }
    }
}