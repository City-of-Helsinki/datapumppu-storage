using AutoMapper;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories.Models.Statistics;
using Storage.Repositories.Statistics;

namespace Storage.Providers.Statistics
{
    /// <summary>
    /// Provides business logic for retrieving statement statistics aggregated by person and year.
    /// </summary>
    public interface IPersonStatementStatisticsProvider
    {
        /// <summary>
        /// Retrieves statement statistics for all individuals within a given year.
        /// Returns aggregated statement data organized by person.
        /// </summary>
        /// <param name="year">The year to retrieve statistics for.</param>
        /// <returns>A list of PersonStatementStatisticsDTO containing aggregated statement data by person.</returns>
        Task<List<PersonStatementStatisticsDTO>> GetStatementStatistics(int year);
    }

    /// <summary>
    /// Implementation of IPersonStatementStatisticsProvider that retrieves and maps person-level statement statistics.
    /// Uses AutoMapper to transform person statement statistics entities to DTOs.
    /// </summary>
    public class PersonStatementStatisticsProvider : IPersonStatementStatisticsProvider
    {
        private readonly IPersonStatementStatisticsRepository _statementStatisticsRepository;

        /// <summary>
        /// Initializes a new instance of the PersonStatementStatisticsProvider class.
        /// </summary>
        /// <param name="statementStatisticsRepository">The repository for accessing person statement statistics data.</param>
        public PersonStatementStatisticsProvider(IPersonStatementStatisticsRepository statementStatisticsRepository)
        {
            _statementStatisticsRepository = statementStatisticsRepository;
        }

        public async Task<List<PersonStatementStatisticsDTO>> GetStatementStatistics(int year)
        {
            var stats = await _statementStatisticsRepository.GetStatistics(year);
            return stats.Select(MapSeatsToDTO).ToList();
        }

        private PersonStatementStatisticsDTO MapSeatsToDTO(PersonStatementStatistics stats)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PersonStatementStatistics, PersonStatementStatisticsDTO>();
            });
            config.AssertConfigurationIsValid();

            return config.CreateMapper().Map<PersonStatementStatisticsDTO>(stats);
        }

    }
}
