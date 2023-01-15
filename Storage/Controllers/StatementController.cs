using Microsoft.AspNetCore.Mvc;
using Storage.Providers;

namespace Storage.Controllers
{
    [ApiController]
    [Route("api/statements/")]
    public class StatementController : ControllerBase
    {
        private readonly ILogger<StatementController> _logger;
        private readonly IStatementProvider _speakingTurnProvider;

        public StatementController(ILogger<StatementController> logger,
            IStatementProvider speakingTurnProvider)
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
                var turns = await _speakingTurnProvider.GetStatements(meetingId, caseNumber);
                return new OkObjectResult(turns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSpeakingTurns failed");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("person")]
        public async Task<IActionResult> GetStatementsByPerson(
            [FromQuery]string name,
            [FromQuery]int year)
        {
            try
            {
                _logger.LogInformation($"GetStatementsByPerson {name}, {year}");
                var turns = await _speakingTurnProvider.GetStatementsByPerson(name, year);
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
