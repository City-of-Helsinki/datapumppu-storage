using Microsoft.AspNetCore.Mvc;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Providers;

namespace Storage.Controllers
{
    [ApiController]
    [Route("api/statements/")]
    public class StatementController : ControllerBase
    {
        private readonly ILogger<StatementController> _logger;
        private readonly IStatementProvider _statementProvider;

        public StatementController(ILogger<StatementController> logger,
            IStatementProvider statementProvider)
        {
            _logger = logger;
            _statementProvider = statementProvider;
        }

        [HttpGet("{meetingId}/{caseNumber}")]
        public async Task<IActionResult> GetStatements(string meetingId, string caseNumber)
        {
            try
            {
                _logger.LogInformation($"GetStatements {meetingId}, {caseNumber}");
                var turns = await _statementProvider.GetStatements(meetingId, caseNumber);
                var filteredTurns = turns.Where(x => x.VideoPosition != 0).ToList();
                return new OkObjectResult(filteredTurns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetStatements failed");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("person")]
        public async Task<IActionResult> GetStatementsByPerson(
            [FromQuery]string name,
            [FromQuery]int year,
            [FromQuery]string lang)
        {
            try
            {
                _logger.LogInformation($"GetStatementsByPerson {name}, {year} {lang}");
                var turns = await _statementProvider.GetStatementsByPerson(name, year, lang);
                var filteredTurns = turns.Where(x => x.VideoPosition != 0).ToList();
                return new OkObjectResult(filteredTurns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetStatementsByPerson failed");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
