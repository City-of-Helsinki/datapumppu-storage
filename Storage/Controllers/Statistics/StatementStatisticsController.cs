using Microsoft.AspNetCore.Mvc;
using Storage.Providers.Statistics;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories.Statistics;

namespace Storage.Controllers.Statistics
{
    /// <summary>
    /// Provides API endpoints for retrieving statement statistics aggregated by year.
    /// </summary>
    [ApiController]
    [Route("api/statistics/statements")]
    public class StatementStatisticsController
    {
        private readonly ILogger<StatementStatisticsController> _logger;
        private readonly IStatementStatisticsProvider _statementStatisticsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementStatisticsController"/> class.
        /// </summary>
        /// <param name="logger">Logger for recording controller operations.</param>
        /// <param name="statementStatisticsProvider">Provider for statement statistics data operations.</param>
        public StatementStatisticsController(
            ILogger<StatementStatisticsController> logger,
            IStatementStatisticsProvider statementStatisticsProvider)
        {
            _logger = logger;
            _statementStatisticsProvider = statementStatisticsProvider;
        }

        /// <summary>
        /// Retrieves statement statistics for a specific year.
        /// </summary>
        /// <param name="year">The year for which to retrieve statement statistics.</param>
        /// <returns>A list of statement statistics for the specified year.</returns>
        [HttpGet("{year}")]
        public async Task<List<StatementStatisticsDTO>> GetStatements(int year)
        {
            _logger.LogInformation("GetStatements {0}", year);
            return await _statementStatisticsProvider.GetStatementStatistics(year);
        }
    }
}
