using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Controllers.Statistics;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories.Models;
using Storage.Repositories.Models.Statistics;
using Storage.Repositories.Statistics;

namespace Storage.Providers.Statistics
{
    /// <summary>
    /// Provides business logic for retrieving statement statistics aggregated by year.
    /// </summary>
    public interface IStatementStatisticsProvider
    {
        /// <summary>
        /// Retrieves statement statistics for a given year.
        /// Returns aggregated statement data for analysis and reporting.
        /// </summary>
        /// <param name="year">The year to retrieve statistics for.</param>
        /// <returns>A list of StatementStatisticsDTO containing aggregated statement data.</returns>
        Task<List<StatementStatisticsDTO>> GetStatementStatistics(int year);
    }

    /// <summary>
    /// Implementation of IStatementStatisticsProvider that retrieves and maps statement statistics.
    /// Uses AutoMapper to transform statement statistics entities to DTOs.
    /// </summary>
    public class StatementStatisticsProvider : IStatementStatisticsProvider
    {
        private readonly IStatementStatisticsRepository _statementStatisticsRepository;

        /// <summary>
        /// Initializes a new instance of the StatementStatisticsProvider class.
        /// </summary>
        /// <param name="statementStatisticsRepository">The repository for accessing statement statistics data.</param>
        public StatementStatisticsProvider(IStatementStatisticsRepository statementStatisticsRepository)
        {
            _statementStatisticsRepository = statementStatisticsRepository;
        }

        public async Task<List<StatementStatisticsDTO>> GetStatementStatistics(int year)
        {
            var stats = await _statementStatisticsRepository.GetStatistics(year);
            return stats.Select(MapSeatsToDTO).ToList();
        }

        private StatementStatisticsDTO MapSeatsToDTO(StatementStatistics stats)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<StatementStatistics, StatementStatisticsDTO>();
            });
            config.AssertConfigurationIsValid();

            return config.CreateMapper().Map<StatementStatisticsDTO>(stats);
        }

    }
}
