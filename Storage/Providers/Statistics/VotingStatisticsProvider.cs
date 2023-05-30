using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Controllers.Statistics;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories.Models;
using Storage.Repositories.Models.Statistics;
using Storage.Repositories.Statistics;

namespace Storage.Providers.Statistics
{
    public interface IVotingStatisticsProvider
    {
        Task<List<VotingStatisticsDTO>> GetStatistics(int year);
    }

    public class VotingStatisticsProvider : IVotingStatisticsProvider
    {
        private readonly IVotingStatisticsRepository _statisticsRepository;

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
