using Microsoft.AspNetCore.Mvc;
using Storage.Providers;

namespace Storage.Controllers
{
    /// <summary>
    /// Provides API endpoints for retrieving meeting statements and speeches.
    /// </summary>
    [ApiController]
    [Route("api/statements/")]
    public class StatementController : ControllerBase
    {
        private readonly ILogger<StatementController> _logger;
        private readonly IStatementProvider _statementProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementController"/> class.
        /// </summary>
        /// <param name="logger">Logger for recording controller operations.</param>
        /// <param name="statementProvider">Provider for statement data operations.</param>
        public StatementController(ILogger<StatementController> logger,
            IStatementProvider statementProvider)
        {
            _logger = logger;
            _statementProvider = statementProvider;
        }

        /// <summary>
        /// Retrieves all statements for a specific case within a meeting.
        /// </summary>
        /// <param name="meetingId">The unique identifier of the meeting.</param>
        /// <param name="caseNumber">The case number within the meeting.</param>
        /// <returns>Returns 200 OK with the list of statements, or 500 Internal Server Error if the operation fails.</returns>
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

        /// <summary>
        /// Retrieves all statements made by a specific person during a given year.
        /// </summary>
        /// <param name="name">The name of the person whose statements to retrieve.</param>
        /// <param name="year">The year for which to retrieve statements.</param>
        /// <param name="lang">The language code (e.g., 'fi' for Finnish, 'sv' for Swedish) for localized content.</param>
        /// <returns>Returns 200 OK with the list of statements, or 500 Internal Server Error if the operation fails.</returns>
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

        /// <summary>
        /// Retrieves statements filtered by person names and/or date range.
        /// At least one filter (names or complete date range) must be provided.
        /// </summary>
        /// <param name="names">Comma-separated list of person names to filter by (optional).</param>
        /// <param name="startDate">Start date of the date range filter (optional, must be used with endDate).</param>
        /// <param name="endDate">End date of the date range filter (optional, must be used with startDate).</param>
        /// <param name="lang">The language code (e.g., 'fi' for Finnish, 'sv' for Swedish) for localized content.</param>
        /// <returns>Returns 200 OK with filtered statements, 400 Bad Request if filter criteria are invalid, or 500 Internal Server Error if the operation fails.</returns>
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
