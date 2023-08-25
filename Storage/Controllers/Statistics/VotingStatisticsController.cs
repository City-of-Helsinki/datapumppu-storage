using Microsoft.AspNetCore.Mvc;
using Storage.Providers.Statistics;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories.Statistics;

namespace Storage.Controllers.Statistics
{
    [ApiController]
    [Route("api/statistics/votings")]
    public class VotingStatisticsController
    {
        private readonly ILogger<VotingStatisticsController> _logger;
        private readonly IVotingStatisticsProvider _statisticsProvider;

        public VotingStatisticsController(
            ILogger<VotingStatisticsController> logger,
            IVotingStatisticsProvider statisticsProvider)
        {
            _logger = logger;
            _statisticsProvider = statisticsProvider;
        }

        [HttpGet("{year}")]
        public async Task<List<VotingStatisticsDTO>> GetStatements(int year)
        {
            _logger.LogInformation("GetStatements {0}", year);
            return await _statisticsProvider.GetStatistics(year);
        }
    }
}
