using Microsoft.AspNetCore.Mvc;
using Storage.Providers.Statistics;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories.Statistics;

namespace Storage.Controllers.Statistics
{
    /// <summary>
    /// Provides API endpoints for retrieving voting statistics aggregated by year.
    /// </summary>
    [ApiController]
    [Route("api/statistics/votings")]
    public class VotingStatisticsController
    {
        private readonly ILogger<VotingStatisticsController> _logger;
        private readonly IVotingStatisticsProvider _statisticsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="VotingStatisticsController"/> class.
        /// </summary>
        /// <param name="logger">Logger for recording controller operations.</param>
        /// <param name="statisticsProvider">Provider for voting statistics data operations.</param>
        public VotingStatisticsController(
            ILogger<VotingStatisticsController> logger,
            IVotingStatisticsProvider statisticsProvider)
        {
            _logger = logger;
            _statisticsProvider = statisticsProvider;
        }

        /// <summary>
        /// Retrieves voting statistics for a specific year.
        /// </summary>
        /// <param name="year">The year for which to retrieve voting statistics.</param>
        /// <returns>A list of voting statistics for the specified year.</returns>
        [HttpGet("{year}")]
        public async Task<List<VotingStatisticsDTO>> GetStatements(int year)
        {
            _logger.LogInformation("GetStatements {0}", year);
            return await _statisticsProvider.GetStatistics(year);
        }
    }
}
