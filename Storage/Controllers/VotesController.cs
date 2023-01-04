using Microsoft.AspNetCore.Mvc;
using Storage.Providers;
using Storage.Repositories;

namespace Storage.Controllers
{
    [ApiController]
    [Route("api/voting/")]
    public class VotesController : ControllerBase
    {
        private readonly ILogger<VotesController> _logger;
        private readonly IVotesProvider _votesProvider;

        public VotesController(
            ILogger<VotesController> logger,
            IVotesProvider votesProvider)
        {
            _logger = logger;
            _votesProvider = votesProvider;
        }

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
