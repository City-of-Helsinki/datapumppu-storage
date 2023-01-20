using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using Storage.Repositories.Models.Extensions;

namespace Storage.Providers
{
    public interface IStatementProvider
    {
        Task<List<WebApiStatementsDTO>> GetStatements(string meetingId, string caseNumber);

        Task<List<WebApiStatementsDTO>> GetStatementsByPerson(string name, int year, string lang);
    }

    public class StatementProvider : IStatementProvider
    {
        private readonly ILogger<StatementProvider> _logger;
        private readonly IStatementsRepository _statementsRepository;
        private readonly IVideoSyncRepository _videoSyncRepository;

        public StatementProvider(ILogger<StatementProvider> logger,
            IStatementsRepository statementsRepository,
            IVideoSyncRepository videoSyncRepository)
        {
            _logger = logger;
            _statementsRepository = statementsRepository;
            _videoSyncRepository = videoSyncRepository;
        }

        public async Task<List<WebApiStatementsDTO>> GetStatements(string meetingId, string caseNumber)
        {
            _logger.LogInformation($"GetStatements {meetingId} {caseNumber}");

            var statements = await _statementsRepository.GetStatements(meetingId, caseNumber);

            var videoSync = await GetVideoSync(meetingId, statements);

            return statements.Select(turn => MapToDTO(turn, videoSync)).ToList();
        }

        public async Task<List<WebApiStatementsDTO>> GetStatementsByPerson(string name, int year, string lang)
        {
            _logger.LogInformation($"GetStatementsByPerson {name} {year} {lang}");

            var statements = await _statementsRepository.GetSatementsByName(name, year, lang);

            
            var dtos = new List<WebApiStatementsDTO>();
            var videoSyncs = new Dictionary<string, List<VideoSync>>();
            foreach (var statement in statements)
            {
                if (!videoSyncs.ContainsKey(statement.MeetingID))
                {
                    videoSyncs.Add(statement.MeetingID, await _videoSyncRepository.GetVideoPositions(statement.MeetingID));
                }

                var syncs = videoSyncs[statement.MeetingID];
                var sync = syncs.Where(sync => sync.Timestamp < statement.Started).OrderBy(sync => sync.Timestamp).FirstOrDefault();

                dtos.Add(MapToDTO(statement, sync));
            }

            return dtos;
        }

        private Task<VideoSync?> GetVideoSync(string meetingId, List<Statement> statements)
        {
            var statement = statements.OrderBy(turn => turn.Started).FirstOrDefault();
            return GetVideoSync(meetingId, statement);
        }

        private Task<VideoSync?> GetVideoSync(string meetingId, Statement? statement)
        {
            var startTime = statement?.Started;
            if (startTime == null)
            {
                return Task.FromResult<VideoSync?>(null);
            }

            return _videoSyncRepository.GetVideoPosition(meetingId, startTime.Value);
        }

        private WebApiStatementsDTO MapToDTO(Statement statement, VideoSync? videoSync)
        {
            var videoPosition = videoSync.GetVideoPosition(statement.Started);
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Statement, WebApiStatementsDTO>()
                    .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.Started))
                    .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.Ended))
                    .ForMember(dest => dest.DurationSeconds, opt => opt.MapFrom(src => src.DurationSeconds))
                    .ForMember(dest => dest.VideoPosition, opt => opt.MapFrom(_ => videoPosition))
                    .ForMember(dest => dest.VideoLink, opt => opt.MapFrom(src => CreateVideoLink(src, videoPosition)));
            });
            config.AssertConfigurationIsValid();

            return config.CreateMapper().Map<WebApiStatementsDTO>(statement);
        }

        private string CreateVideoLink(Statement statement, int videoPosition)
        {
            int year = Int32.Parse(statement.MeetingID.Substring(5, 4));
            int number = Int32.Parse(statement.MeetingID.Substring(9));
            return @$"https://helsinkikanava.fi/fi/player/event/view?meeting=kvsto-{year}-{number}#T{videoPosition}";
        }
    }
}
