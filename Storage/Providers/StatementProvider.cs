using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Mappers;
using Storage.Providers.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace Storage.Providers
{
    public interface IStatementProvider
    {
        Task<List<WebApiStatementsDTO>> GetSpeakingTurns(string meetingId, string caseNumber);
    }

    public class StatementProvider : IStatementProvider
    {
        private readonly ILogger<StatementProvider> _logger;
        private readonly ISpeakingTurnsRepository _speakingTurnsRepository;

        public StatementProvider(ILogger<StatementProvider> logger,
            ISpeakingTurnsRepository speakingTurnsRepository)
        {
            _logger = logger;
            _speakingTurnsRepository = speakingTurnsRepository;
        }

        public async Task<List<WebApiStatementsDTO>> GetSpeakingTurns(string meetingId, string caseNumber)
        {
            var speakingTurns = await _speakingTurnsRepository.GetSpeakingTurns(meetingId, caseNumber);

            return speakingTurns.Select(MapToDTO).ToList();

        }

        private WebApiStatementsDTO MapToDTO(Statement seat)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Statement, WebApiStatementsDTO>();
            });

            return config.CreateMapper().Map<WebApiStatementsDTO>(seat);
        }
    }
}
