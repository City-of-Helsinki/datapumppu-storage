using Microsoft.AspNetCore.Mvc;
using Storage.Providers.Statistics;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories.Statistics;

namespace Storage.Controllers.Statistics
{
    /// <summary>
    /// Provides API endpoints for retrieving per-person statement statistics aggregated by year.
    /// </summary>
    [ApiController]
    [Route("api/statistics/personstatements")]
    public class PersonStatementStatisticsController
    {
        private readonly ILogger<PersonStatementStatisticsController> _logger;
        private readonly IPersonStatementStatisticsProvider _statementStatisticsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonStatementStatisticsController"/> class.
        /// </summary>
        /// <param name="logger">Logger for recording controller operations.</param>
        /// <param name="statementStatisticsProvider">Provider for person statement statistics data operations.</param>
        public PersonStatementStatisticsController(
            ILogger<PersonStatementStatisticsController> logger,
            IPersonStatementStatisticsProvider statementStatisticsProvider)
        {
            _logger = logger;
            _statementStatisticsProvider = statementStatisticsProvider;
        }

        /// <summary>
        /// Retrieves statement statistics grouped by person for a specific year.
        /// </summary>
        /// <param name="year">The year for which to retrieve person statement statistics.</param>
        /// <returns>A list of person statement statistics for the specified year.</returns>
        [HttpGet("{year}")]
        public async Task<List<PersonStatementStatisticsDTO>> GetStatements(int year)
        {
            _logger.LogInformation("GetStatements {0}", year);
            return await _statementStatisticsProvider.GetStatementStatistics(year);
        }
    }
}
