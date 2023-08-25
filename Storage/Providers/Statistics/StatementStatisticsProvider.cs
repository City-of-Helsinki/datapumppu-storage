using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Controllers.Statistics;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories.Models;
using Storage.Repositories.Models.Statistics;
using Storage.Repositories.Statistics;

namespace Storage.Providers.Statistics
{
    public interface IStatementStatisticsProvider
    {
        Task<List<StatementStatisticsDTO>> GetStatementStatistics(int year);
    }

    public class StatementStatisticsProvider : IStatementStatisticsProvider
    {
        private readonly IStatementStatisticsRepository _statementStatisticsRepository;

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
