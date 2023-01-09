using Microsoft.AspNetCore.Mvc;
using Storage.Providers;
using Storage.Repositories;

namespace Storage.Controllers
{
    [ApiController]
    [Route("api/seats/")]
    public class SeatsController : ControllerBase
    {
        private readonly ILogger<SeatsController> _logger;
        private readonly ISeatsProvider _seatsProvider;

        public SeatsController(
            ILogger<SeatsController> logger,
            ISeatsProvider seatsProvider)
        {
            _logger = logger;
            _seatsProvider = seatsProvider;
        }

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
