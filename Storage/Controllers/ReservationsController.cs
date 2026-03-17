using Microsoft.AspNetCore.Mvc;
using Storage.Providers;

namespace Storage.Controllers
{
    /// <summary>
    /// Provides API endpoints for managing statement and reply reservations during meetings.
    /// </summary>
    [ApiController]
    [Route("api/reservations/")]
    public class ReservationsController : ControllerBase
    {
        private readonly ILogger<ReservationsController> _logger;
        private readonly IReservationsProvider _reservationsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationsController"/> class.
        /// </summary>
        /// <param name="logger">Logger for recording controller operations.</param>
        /// <param name="reservationsProvider">Provider for reservation data operations.</param>
        public ReservationsController(ILogger<ReservationsController> logger,
            IReservationsProvider reservationsProvider)
        {
            _logger = logger;
            _reservationsProvider = reservationsProvider;
        }

        /// <summary>
        /// Retrieves all reservations (speaking turns) for a specific case within a meeting.
        /// </summary>
        /// <param name="meetingId">The unique identifier of the meeting.</param>
        /// <param name="caseNumber">The case number within the meeting.</param>
        /// <returns>Returns 200 OK with the list of reservations, or 500 Internal Server Error if the operation fails.</returns>
        [HttpGet("{meetingId}/{caseNumber}")]
        public async Task<IActionResult> GetReservations(string meetingId, string caseNumber)
        {
            try
            {
                _logger.LogInformation($"GetReservations {meetingId}, {caseNumber}");
                var turns = await _reservationsProvider.GetReservations(meetingId, caseNumber);
                return new OkObjectResult(turns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetReservations failed");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

    }
}
