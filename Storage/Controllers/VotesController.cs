using Microsoft.AspNetCore.Mvc;
using Storage.Providers;
using Storage.Repositories;

namespace Storage.Controllers
{
    /// <summary>
    /// Provides API endpoints for retrieving voting data and results from meetings.
    /// </summary>
    [ApiController]
    [Route("api/voting/")]
    public class VotesController : ControllerBase
    {
        private readonly ILogger<VotesController> _logger;
        private readonly IVotesProvider _votesProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="VotesController"/> class.
        /// </summary>
        /// <param name="logger">Logger for recording controller operations.</param>
        /// <param name="votesProvider">Provider for voting data operations.</param>
        public VotesController(
            ILogger<VotesController> logger,
            IVotesProvider votesProvider)
        {
            _logger = logger;
            _votesProvider = votesProvider;
        }

        /// <summary>
        /// Retrieves voting information for a specific case within a meeting.
        /// </summary>
        /// <param name="meetingId">The unique identifier of the meeting.</param>
        /// <param name="caseNumber">The case number within the meeting.</param>
        /// <returns>Returns 200 OK with voting data including individual votes, or 500 Internal Server Error if the operation fails.</returns>
        [HttpGet("{meetingId}/{caseNumber}")]
        public async Task<IActionResult> GetVotes(string meetingId, string caseNumber)
        {
            try
            {
                _logger.LogInformation($"GetVotes {meetingId}, {caseNumber}");
                var voting = await _votesProvider.GetVoting(meetingId, caseNumber);
                return new OkObjectResult(voting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetVotes failed");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
