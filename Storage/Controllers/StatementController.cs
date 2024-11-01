using Microsoft.AspNetCore.Mvc;
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
                return new OkObjectResult(turns);
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
                return new OkObjectResult(turns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetStatementsByPerson failed");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetStatementsByPersonOrDate(
            [FromQuery]string? names,
            [FromQuery]DateTime? startDate,
            [FromQuery]DateTime? endDate,
            [FromQuery]string lang)
        {
            try
            {
                var nameList = string.IsNullOrWhiteSpace(names) 
                       ? new List<string>() 
                       : names.Split(',')
                              .Select(name => name.Trim())
                              .Where(name => !string.IsNullOrEmpty(name))
                              .ToList();

                // Check if both dates are provided together or none at all
                if ((startDate.HasValue && !endDate.HasValue) || (!startDate.HasValue && endDate.HasValue))
                {
                    return BadRequest(new { Message = "Sekä startDate että endDate on asetettava, jos päivämääräsuodatus on käytössä" });
                }

                // Ensure at least one filter is provided (either names or complete date range)
                if (!nameList.Any() && !startDate.HasValue && !endDate.HasValue)
                {
                    return BadRequest(new { Message = "Vähintään yksi hakusuodatin on oltava asetettuna (nimilista (names) tai päivämäärien väli (startDate ja endDate))" });
                }

                if (endDate.HasValue)
                {
                    endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
                }

                _logger.LogInformation($"GetStatementsByPersonOrDate {string.Join(", ", nameList)}, {startDate} {endDate} {lang}");

                var statements = await _statementProvider.GetStatementsByPersonOrDate(nameList, startDate, endDate, lang);
                return new OkObjectResult(statements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetStatementsByPersonOrDate failed");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
