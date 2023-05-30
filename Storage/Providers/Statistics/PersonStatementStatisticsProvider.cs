using AutoMapper;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories.Models.Statistics;
using Storage.Repositories.Statistics;

namespace Storage.Providers.Statistics
{
    public interface IPersonStatementStatisticsProvider
    {
        Task<List<PersonStatementStatisticsDTO>> GetStatementStatistics(int year);
    }

    public class PersonStatementStatisticsProvider : IPersonStatementStatisticsProvider
    {
        private readonly IPersonStatementStatisticsRepository _statementStatisticsRepository;

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
