using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Mappers;
using Storage.Providers.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using Storage.Repositories.Models.Extensions;

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
        private readonly IVideoSyncRepository _videoSyncRepository;

        public StatementProvider(ILogger<StatementProvider> logger,
            ISpeakingTurnsRepository speakingTurnsRepository,
            IVideoSyncRepository videoSyncRepository)
        {
            _logger = logger;
            _speakingTurnsRepository = speakingTurnsRepository;
            _videoSyncRepository = videoSyncRepository;
        }

        public async Task<List<WebApiStatementsDTO>> GetSpeakingTurns(string meetingId, string caseNumber)
        {
            _logger.LogInformation($"GetSpeakingTurns {meetingId} {caseNumber}");

            var speakingTurns = await _speakingTurnsRepository.GetSpeakingTurns(meetingId, caseNumber);

            var videoSync = await GetVideoSync(meetingId, speakingTurns);

            return speakingTurns.Select(turn => MapToDTO(turn, videoSync)).ToList();
        }

        private Task<VideoSync?> GetVideoSync(string meetingId, List<Statement> statements)
        {
            var startTime = statements.OrderBy(turn => turn.Started).FirstOrDefault()?.Started;
            if (startTime == null)
            {
                return Task.FromResult<VideoSync?>(null);
            }

            return _videoSyncRepository.GetVideoPosition(meetingId, startTime.Value);
        }

        private WebApiStatementsDTO MapToDTO(Statement seat, VideoSync? videoSync)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Statement, WebApiStatementsDTO>()
                    .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.Started))
                    .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.Ended))
                    .ForMember(dest => dest.DurationSeconds, opt => opt.MapFrom(src => src.DurationSeconds))
                    .ForMember(dest => dest.VideoPosition, opt => opt.MapFrom(src => videoSync.GetVideoPosition(src.Started)));
            });
            config.AssertConfigurationIsValid();

            return config.CreateMapper().Map<WebApiStatementsDTO>(seat);
        }
    }
}
