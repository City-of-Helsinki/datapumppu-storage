using System.Data;
using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace Storage.Actions
{
    public class UpdateMeetingSeatsAction : IEventAction
    {
        public List<EventType> EventTypes { get; } = new()
            { EventType.Attendees };

        private readonly IMeetingSeatsRepository _meetingSeatsRepository;

        public UpdateMeetingSeatsAction(IMeetingSeatsRepository meetingSeatsRepository)
        {
            _meetingSeatsRepository = meetingSeatsRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var attendeesEventDto = eventBody.ToObjectFromJson<AttendeesEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AttendeesEventDTO, MeetingSeatUpdate>()
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
                cfg.CreateMap<MeetingSeatDTO, MeetingSeat>();
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var meetingSeats = attendeesEventDto.MeetingSeats.Select(meetingSeatDto => mapper.Map<MeetingSeat>(meetingSeatDto)).ToList();
            var meetingSeatUpdate = mapper.Map<MeetingSeatUpdate>(attendeesEventDto);

            return _meetingSeatsRepository.InsertMeetingSeatUpdate(meetingSeatUpdate, meetingSeats, connection, transaction);
        }
    }
}
