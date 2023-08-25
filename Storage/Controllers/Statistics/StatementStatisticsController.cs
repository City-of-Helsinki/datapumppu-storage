using Microsoft.AspNetCore.Mvc;
using Storage.Providers.Statistics;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories.Statistics;

namespace Storage.Controllers.Statistics
{
    [ApiController]
    [Route("api/statistics/statements")]
    public class StatementStatisticsController
    {
        private readonly ILogger<StatementStatisticsController> _logger;
        private readonly IStatementStatisticsProvider _statementStatisticsProvider;

        public StatementStatisticsController(
            ILogger<StatementStatisticsController> logger,
            IStatementStatisticsProvider statementStatisticsProvider)
        {
            _logger = logger;
            _statementStatisticsProvider = statementStatisticsProvider;
        }

        [HttpGet("{year}")]
        public async Task<List<StatementStatisticsDTO>> GetStatements(int year)
        {
            _logger.LogInformation("GetStatements {0}", year);
            return await _statementStatisticsProvider.GetStatementStatistics(year);
        }
    }
}
