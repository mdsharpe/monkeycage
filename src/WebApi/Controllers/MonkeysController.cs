using Microsoft.AspNetCore.Mvc;
using MonkeyCage.Models;
using MonkeyCage.MonkeyBusiness;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonkeysController : ControllerBase
    {
        private readonly ILogger<MonkeysController> _logger;
        private readonly MonkeyService _typewriterService;

        public MonkeysController(
            ILogger<MonkeysController> logger,
            MonkeyService typewriterService)
        {
            _logger = logger;
            _typewriterService = typewriterService;
        }

        [HttpPost("sync")]
        public async Task<IActionResult> RunSynchronously(RequestModel request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received request for {MonkeyCount} monkeys to find '{TargetText}' in {Timeout}.", request.MonkeyCount, request.TargetText, request.Timeout);

            var result = await _typewriterService.ProcessRequest(request, cancellationToken);

            return Ok(result);
        }
    }
}
