using Microsoft.AspNetCore.Mvc;
using Storage.Providers.Statistics;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories.Statistics;

namespace Storage.Controllers.Statistics
{
    /// <summary>
    /// Provides API endpoints for retrieving participant attendance statistics aggregated by year.
    /// </summary>
    [ApiController]
    [Route("api/statistics/participants")]
    public class ParticipantsStatisticsController
    {
        private readonly ILogger<ParticipantsStatisticsController> _logger;
        private readonly IParticipantStatisticsProvider _statisticsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantsStatisticsController"/> class.
        /// </summary>
        /// <param name="logger">Logger for recording controller operations.</param>
        /// <param name="statisticsProvider">Provider for participant statistics data operations.</param>
        public ParticipantsStatisticsController(
            ILogger<ParticipantsStatisticsController> logger,
            IParticipantStatisticsProvider statisticsProvider)
        {
            _logger = logger;
            _statisticsProvider = statisticsProvider;
        }

        /// <summary>
        /// Retrieves participant attendance statistics for a specific year.
        /// </summary>
        /// <param name="year">The year for which to retrieve participant statistics.</param>
        /// <returns>A list of participant statistics showing attendance information for the specified year.</returns>
        [HttpGet("{year}")]
        public async Task<List<ParticipationsPersonDTO>> GetParticipants(int year)
        {
            _logger.LogInformation("GetParticipants {0}", year);
            return await _statisticsProvider.GetStatistics(year);
        }
    }
}
