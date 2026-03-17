using Microsoft.AspNetCore.Mvc;
using Storage.Providers;
using Storage.Repositories;

namespace Storage.Controllers
{
    /// <summary>
    /// Provides API endpoints for retrieving meeting seat allocations and participant positions.
    /// </summary>
    [ApiController]
    [Route("api/seats/")]
    public class SeatsController : ControllerBase
    {
        private readonly ILogger<SeatsController> _logger;
        private readonly ISeatsProvider _seatsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeatsController"/> class.
        /// </summary>
        /// <param name="logger">Logger for recording controller operations.</param>
        /// <param name="seatsProvider">Provider for seat allocation data operations.</param>
        public SeatsController(
            ILogger<SeatsController> logger,
            ISeatsProvider seatsProvider)
        {
            _logger = logger;
            _seatsProvider = seatsProvider;
        }

        /// <summary>
        /// Retrieves seat allocation information for a specific case within a meeting.
        /// </summary>
        /// <param name="meetingId">The unique identifier of the meeting.</param>
        /// <param name="caseNumber">The case number within the meeting.</param>
        /// <returns>Returns 200 OK with seat allocation data, or 500 Internal Server Error if the operation fails.</returns>
        [HttpGet("{meetingId}/{caseNumber}")]
        public async Task<IActionResult> GetSeats(string meetingId, string caseNumber)
        {
            try
            {
                _logger.LogInformation($"GetSeats {meetingId}, {caseNumber}");
                var seats = await _seatsProvider.GetSeats(meetingId, caseNumber);
                return new OkObjectResult(seats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSeats failed");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
