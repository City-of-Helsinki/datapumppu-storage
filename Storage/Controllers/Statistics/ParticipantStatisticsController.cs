using Microsoft.AspNetCore.Mvc;
using Storage.Providers.Statistics;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories.Statistics;

namespace Storage.Controllers.Statistics
{
    [ApiController]
    [Route("api/statistics/participants")]
    public class ParticipantsStatisticsController
    {
        private readonly ILogger<ParticipantsStatisticsController> _logger;
        private readonly IParticipantStatisticsProvider _statisticsProvider;

        public ParticipantsStatisticsController(
            ILogger<ParticipantsStatisticsController> logger,
            IParticipantStatisticsProvider statisticsProvider)
        {
            _logger = logger;
            _statisticsProvider = statisticsProvider;
        }

        [HttpGet("{year}")]
        public async Task<List<ParticipationsPersonDTO>> GetParticipants(int year)
        {
            _logger.LogInformation("GetParticipants {0}", year);
            return await _statisticsProvider.GetStatistics(year);
        }
    }
}
