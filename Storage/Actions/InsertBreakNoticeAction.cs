using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    public class InsertBreakNoticeAction : IEventAction
    {
        public List<EventType> EventTypes { get; } = new()
            { EventType.BreakNotice };

        private readonly IBreakNoticeRepository _breakNoticeRepository;

        public InsertBreakNoticeAction(IBreakNoticeRepository breakNoticeRepository)
        {
            _breakNoticeRepository = breakNoticeRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var breakNoticeEventDto = eventBody.ToObjectFromJson<BreakNoticeEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<BreakNoticeEventDTO, BreakNotice>()
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var breakNotice = mapper.Map<BreakNotice>(breakNoticeEventDto);

            return _breakNoticeRepository.InsertBreakNotice(breakNotice, connection, transaction);
        }
    }
}