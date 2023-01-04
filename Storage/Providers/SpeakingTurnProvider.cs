using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Mappers;
using Storage.Providers.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace Storage.Providers
{
    public interface ISpeakingTurnProvider
    {
        Task<List<WebApiSpeakingTurnDTO>> GetSpeakingTurns(string meetingId, string caseNumber);
    }

    public class SpeakingTurnProvider : ISpeakingTurnProvider
    {
        private readonly ILogger<SpeakingTurnProvider> _logger;
        private readonly ISpeakingTurnsRepository _speakingTurnsRepository;

        public SpeakingTurnProvider(ILogger<SpeakingTurnProvider> logger,
            ISpeakingTurnsRepository speakingTurnsRepository)
        {
            _logger = logger;
            _speakingTurnsRepository = speakingTurnsRepository;
        }

        public async Task<List<WebApiSpeakingTurnDTO>> GetSpeakingTurns(string meetingId, string caseNumber)
        {
            var speakingTurns = await _speakingTurnsRepository.GetSpeakingTurns(meetingId, caseNumber);

            return speakingTurns.Select(MapToDTO).ToList();

        }

        private WebApiSpeakingTurnDTO MapToDTO(SpeakingTurn seat)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SpeakingTurn, WebApiSpeakingTurnDTO>();
            });

            return config.CreateMapper().Map<WebApiSpeakingTurnDTO>(seat);
        }
    }
}
