using Microsoft.AspNetCore.Mvc;

namespace MonkeyCage.WebApi.Controllers
{
    [Route("")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return NoContent();
        }
    }
}
