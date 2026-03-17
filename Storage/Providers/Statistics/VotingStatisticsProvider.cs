using AutoMapper;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories.Models.Statistics;
using Storage.Repositories.Statistics;

namespace Storage.Providers.Statistics
{
    /// <summary>
    /// Provides business logic for retrieving voting statistics aggregated by person and year.
    /// </summary>
    public interface IVotingStatisticsProvider
    {
        /// <summary>
        /// Retrieves voting statistics for all participants within a given year.
        /// Returns vote counts (for, against, empty, absent) aggregated by person.
        /// </summary>
        /// <param name="year">The year to retrieve statistics for.</param>
        /// <returns>A list of VotingStatisticsDTO containing aggregated voting data by person.</returns>
        Task<List<VotingStatisticsDTO>> GetStatistics(int year);
    }

    /// <summary>
    /// Implementation of IVotingStatisticsProvider that retrieves and maps voting statistics.
    /// Uses AutoMapper to transform voting statistics entities to DTOs.
    /// </summary>
    public class VotingStatisticsProvider : IVotingStatisticsProvider
    {
        private readonly IVotingStatisticsRepository _statisticsRepository;

        /// <summary>
        /// Initializes a new instance of the VotingStatisticsProvider class.
        /// </summary>
        /// <param name="statisticsRepository">The repository for accessing voting statistics data.</param>
        public VotingStatisticsProvider(IVotingStatisticsRepository statisticsRepository)
        {
            _statisticsRepository = statisticsRepository;
        }

        public async Task<List<VotingStatisticsDTO>> GetStatistics(int year)
        {
            var stats = await _statisticsRepository.GetStatistics(year);
            return stats.Select(MapSeatsToDTO).ToList();
        }

        private VotingStatisticsDTO MapSeatsToDTO(VotingStatistics stats)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VotingStatistics, VotingStatisticsDTO>();
            });
            config.AssertConfigurationIsValid();

            return config.CreateMapper().Map<VotingStatisticsDTO>(stats);
        }
    }
}
