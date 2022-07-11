using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Internal;

namespace MonkeyCage.WebApi.Controllers
{
    [Route("")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ISystemClock _systemClock;

        public HomeController(ISystemClock systemClock)
        {
            _systemClock = systemClock;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return NoContent();
        }

        [HttpGet("health-check")]
        public IActionResult HealthCheck()
        {
            if (_systemClock.UtcNow.Minute % 10 >= 5)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            return Ok();
        }
    }
}
