using Microsoft.AspNetCore.Mvc;
using Storage.Providers;

namespace Storage.Controllers
{
    [ApiController]
    [Route("api/reservations/")]
    public class ReservationsController : ControllerBase
    {
        private readonly ILogger<ReservationsController> _logger;
        private readonly IReservationsProvider _reservationsProvider;

        public ReservationsController(ILogger<ReservationsController> logger,
            IReservationsProvider reservationsProvider)
        {
            _logger = logger;
            _reservationsProvider = reservationsProvider;
        }

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
