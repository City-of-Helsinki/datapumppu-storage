using System.Data;
using AutoMapper;
using Storage.Controllers.Event.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace Storage.Actions
{
    public class UpdateMeetingStatusAction : IEventAction
    {
        public List<EventType> EventTypes { get; } = new()
            { EventType.MeetingStarted, EventType.MeetingEnded };

        private readonly IMeetingsRepository _meetingsRepository;

        public UpdateMeetingStatusAction(IMeetingsRepository meetingsRepository)
        {
            _meetingsRepository = meetingsRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var meetingStatusEvent = eventBody.ToObjectFromJson<SimpleEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SimpleEventDTO, Meeting>()
                    .ForMember(dest => dest.MeetingStartedEventID, opt =>
                    {
                        opt.PreCondition(src => src.EventType == EventType.MeetingStarted);
                        opt.MapFrom(x => eventId);
                    })
                    .ForMember(dest => dest.MeetingStarted, opt =>
                    {
                        opt.PreCondition(src => src.EventType == EventType.MeetingStarted);
                        opt.MapFrom(src => src.Timestamp);
                    })
                    .ForMember(dest => dest.MeetingEndedEventID, opt =>
                    {
                        opt.PreCondition(src => src.EventType == EventType.MeetingEnded);
                        opt.MapFrom(x => eventId);
                    })
                    .ForMember(dest => dest.MeetingEnded, opt =>
                    {
                         opt.PreCondition(src => src.EventType == EventType.MeetingEnded);
                         opt.MapFrom(src => src.Timestamp);
                    });
            });
            var mapper = config.CreateMapper();
            var meeting = mapper.Map<Meeting>(meetingStatusEvent);

            if (meetingStatusEvent.EventType == EventType.MeetingStarted)
            {
                return _meetingsRepository.UpsertMeetingStartTime(meeting, connection, transaction);
            }
            return _meetingsRepository.UpdateMeetingEndTime(meeting, connection, transaction);
        }
    }
}
