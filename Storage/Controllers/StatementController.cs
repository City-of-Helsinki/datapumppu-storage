using Microsoft.AspNetCore.Mvc;
using Storage.Providers;

namespace Storage.Controllers
{
    [ApiController]
    [Route("api/statements/")]
    public class StatementController : ControllerBase
    {
        private readonly ILogger<StatementController> _logger;
        private readonly ISpeakingTurnProvider _speakingTurnProvider;

        public StatementController(ILogger<StatementController> logger,
            ISpeakingTurnProvider speakingTurnProvider)
        {
            _logger = logger;
            _speakingTurnProvider = speakingTurnProvider;
        }

        [HttpGet("{meetingId}/{caseNumber}")]
        public async Task<IActionResult> GetSpeakingTurns(string meetingId, string caseNumber)
        {
            try
            {
                _logger.LogInformation($"GetSpeakingTurns {meetingId}, {caseNumber}");
                var turns = await _speakingTurnProvider.GetSpeakingTurns(meetingId, caseNumber);
                return new OkObjectResult(turns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSpeakingTurns failed");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

    }
}
