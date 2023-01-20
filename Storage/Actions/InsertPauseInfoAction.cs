using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    public class InsertPauseInfoAction : IEventAction
    {
        public List<EventType> EventTypes { get; } = new()
            { EventType.PauseInfo };

        private readonly IPauseInfoRepository _pauseInfoRepository;

        public InsertPauseInfoAction(IPauseInfoRepository pauseInfoRepository)
        {
            _pauseInfoRepository = pauseInfoRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var breakNoticeEventDto = eventBody.ToObjectFromJson<PauseInfoEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PauseInfoEventDTO, PauseInfo>()
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var breakNotice = mapper.Map<PauseInfo>(breakNoticeEventDto);

            return _pauseInfoRepository.InsertPauseInfo(breakNotice, connection, transaction);
        }
    }
}