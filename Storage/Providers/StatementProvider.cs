using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using Storage.Repositories.Models.Extensions;
using System.Collections.Generic;

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
        private readonly IMeetingsRepository _meetingRepository;

        public StatementProvider(ILogger<StatementProvider> logger,
            IStatementsRepository statementsRepository,
            IVideoSyncRepository videoSyncRepository,
            IMeetingsRepository meetingRepository)
        {
            _logger = logger;
            _statementsRepository = statementsRepository;
            _videoSyncRepository = videoSyncRepository;
            _meetingRepository = meetingRepository;
        }

        public async Task<List<WebApiStatementsDTO>> GetStatements(string meetingId, string caseNumber)
        {
            _logger.LogInformation($"GetStatements {meetingId} {caseNumber}");

            var statements = await _statementsRepository.GetStatements(meetingId, caseNumber);

            var videoSync = await GetVideoSync(meetingId, statements);
            var statementList = new List<WebApiStatementsDTO>();
            foreach (var statement in statements)
            {
                statementList.Add(await MapToDTO(statement, videoSync));
            }

            var filteredStatements = statementList.Where(x => x.VideoPosition != 0).ToList();

            return filteredStatements;
        }

        public async Task<List<WebApiStatementsDTO>> GetStatementsByPerson(string name, int year, string lang)
        {
            _logger.LogInformation($"GetStatementsByPerson {name} {year} {lang}");

            var statements = await _statementsRepository.GetSatementsByName(name, year, lang);

            var dtos = new List<WebApiStatementsDTO>();
            var videoSyncs = new Dictionary<string, List<VideoSync>>();
            foreach (var statement in statements)
            {
                // ignore meetings in 2010 (these are test meetings)
                if (statement.MeetingID.StartsWith("029002010"))
                {
                    continue;
                }

                if (!videoSyncs.ContainsKey(statement.MeetingID))
                {
                    videoSyncs.Add(statement.MeetingID, await _videoSyncRepository.GetVideoPositions(statement.MeetingID));
                }

                var syncs = videoSyncs[statement.MeetingID] ?? new List<VideoSync>();
                var sync = syncs.Where(sync => sync.Timestamp < statement.Started).OrderBy(sync => sync.Timestamp).FirstOrDefault();

                dtos.Add(await MapToDTO(statement, sync));
            }

            var filteredDtos = dtos.Where(x => x.VideoPosition != 0).ToList();

            return filteredDtos;
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

        private async Task<WebApiStatementsDTO> MapToDTO(Statement statement, VideoSync? videoSync)
        {
            var videoPosition = videoSync.GetVideoPosition(statement.Started);
            
            var meeting = await _meetingRepository.FetchMeetingById(statement.MeetingID);
            if (meeting == null)
            {
                return new WebApiStatementsDTO();
            }

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Statement, WebApiStatementsDTO>()
                    .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.Started))
                    .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.Ended))
                    .ForMember(dest => dest.DurationSeconds, opt => opt.MapFrom(src => src.DurationSeconds))
                    .ForMember(dest => dest.VideoPosition, opt => opt.MapFrom(_ => videoPosition))
                    .ForMember(dest => dest.VideoLink, opt => opt.MapFrom(src => CreateVideoLink(src, meeting.MeetingSequenceNumber, videoPosition)));
            });
            config.AssertConfigurationIsValid();

            return config.CreateMapper().Map<WebApiStatementsDTO>(statement);
        }

        private string CreateVideoLink(Statement statement, int sequenceNumber, int videoPosition)
        {
            int year = Int32.Parse(statement.MeetingID.Substring(5, 4));
            return @$"https://www.helsinkikanava.fi/fi/player/event/view?meeting=kvsto-{year}-{sequenceNumber}#T{videoPosition}";
        }
    }
}
