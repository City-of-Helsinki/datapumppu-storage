using Microsoft.AspNetCore.Mvc;
using Storage.Providers.Statistics;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories.Statistics;

namespace Storage.Controllers.Statistics
{
    [ApiController]
    [Route("api/statistics/personstatements")]
    public class PersonStatementStatisticsController
    {
        private readonly ILogger<PersonStatementStatisticsController> _logger;
        private readonly IPersonStatementStatisticsProvider _statementStatisticsProvider;

        public PersonStatementStatisticsController(
            ILogger<PersonStatementStatisticsController> logger,
            IPersonStatementStatisticsProvider statementStatisticsProvider)
        {
            _logger = logger;
            _statementStatisticsProvider = statementStatisticsProvider;
        }

        [HttpGet("{year}")]
        public async Task<List<PersonStatementStatisticsDTO>> GetStatements(int year)
        {
            _logger.LogInformation("GetStatements {0}", year);
            return await _statementStatisticsProvider.GetStatementStatistics(year);
        }
    }
}
